using System;
using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.str;
using uk.vroad.api.xmpl;
using uk.vroad.apk;
using uk.vroad.pac;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;


namespace uk.vroad.xmpl
{
    public class UBuildMap: MonoBehaviour
    {
        public static UBuildMap MostRecentInstance { get; private set;  }

        public string osmFilePath;
        public string latLong;
        public string divisionWeights;
        public Text buildErrorText;
        public Slider osmSlider;
        public Slider vRoadSlider;

        private App app;
        private bool hasUI;
        private bool buildInProgress;
        private bool loadInProgress;
        private string progressActivity;
        private int progressRaw;
        private int progressSmoothed;

        public void SetupBuild(string osmPath, string ll)
        {
            osmFilePath = osmPath;
            latLong = ll;
        }

        protected virtual void Awake()
        {
            app = ExampleApp.AwakeInstance();
            
            MostRecentInstance = this;
            hasUI = buildErrorText != null && osmSlider != null && vRoadSlider != null;

            string trailingArgs = ExternalMapBuilder.TrailingArgs(divisionWeights);
            
            KFile osmFile = FindOsmFile();
            if (osmFile != null && osmFile.Exists()) // should be OSM (.json) file 
            {
                StartExternalBuild(ExternalMapBuilder.BuildMapFromFile(osmFile.FullPath(), trailingArgs));
            }
            else if (LooksLikeLatLong(latLong))
            {
                StartExternalBuild(ExternalMapBuilder.BuildMapFromDownload(latLong, trailingArgs));
            }
            else
            {
                Debug.LogWarning("No location for map build. " +
                                 "Expected OSM JSON file ("+osmFilePath+") or Lat-Long ("+latLong+")");
            }
        }
        
        private void StartExternalBuild(bool callStatus)
        {
            if (hasUI) vRoadSlider.gameObject.SetActive(false);
            buildInProgress = callStatus;
            loadInProgress = false;
            progressRaw = 0;
            progressSmoothed = 0;
            progressActivity = callStatus? "Building Map": "Failed to Build";
            if (buildInProgress && hasUI) osmSlider.gameObject.SetActive(true);
        }
        
        
        private void StartLoad(KFile mapFile)
        {
            buildInProgress = false;
            loadInProgress = true;
            progressRaw = 0;
            progressSmoothed = 0;
            progressActivity = "Loading Map";
            if (hasUI) vRoadSlider.gameObject.SetActive(true);

            Reporter.ProgressPartsUI(1);
            VRoad.Load(app, mapFile);
        }
        void FixedUpdate()
        {
            if (buildInProgress)
            {
                progressRaw = ExternalMapBuilder.Progress();
                string newBuild = ExternalMapBuilder.NewlyBuiltFile();
                string buildError = ExternalMapBuilder.BuildError();

                if (newBuild == null && buildError == null)
                {
                    int diff = (progressRaw * 100) - progressSmoothed;
                    if (diff > 0)
                    {
                        int inc = diff > 500? 20: 5;
                        progressSmoothed += inc;
                        if (hasUI) osmSlider.value = progressSmoothed;
                    }
                }
                else
                {
                    if (hasUI) osmSlider.gameObject.SetActive(false);
                    
                    buildInProgress = false;
                    progressRaw = 0;
                    progressSmoothed = 0;
                    
                    KFile mapFile = newBuild != null? new KFile(newBuild): null;
                    if (mapFile != null && mapFile.Exists() && AppTools.SuitableAppFile(mapFile))
                    {
                        if (hasUI) vRoadSlider.gameObject.SetActive(true);
                        StartLoad(mapFile);
                    }
                    else
                    {
                        if (buildError == null) buildError = "No generated file, no error?";

                        if (hasUI)
                        {
                            buildErrorText.gameObject.SetActive(true);
                            buildErrorText.text = buildError;
                        }
                        else Debug.Log(buildError);
                    }
                }
            }
            else if (loadInProgress)
            {
                progressRaw = Reporter.ProgressTotal();

                if (progressRaw < 100)
                {
                    int diff = (progressRaw * 100) - progressSmoothed;
                    if (diff > 0)
                    {
                        int inc = diff > 1000? 100: diff > 500? 20: 5;
                        progressSmoothed += inc;
                        if (hasUI) vRoadSlider.value = progressSmoothed;
                    }
                }
                else
                {
                    if (hasUI) vRoadSlider.gameObject.SetActive(false);
                    loadInProgress = false;
                    
                }
            }
        }

        public int Progress() { return progressRaw; }
        public string ProgressActivity() { return progressActivity; }

        private KFile FindOsmFile()
        {
            // ** ALWAYS ** Call OsmDir here (to initialize in correct thread)
            string userOsmDir = KEnv.OsmDir();
            
            if (osmFilePath == null) return null;
            osmFilePath = osmFilePath.Trim();
            if (osmFilePath.Length < 5) return null;
            
            // First check absolute path
            KFile mapFile = new KFile(osmFilePath);
            if (mapFile.Exists()) return mapFile;
            
            mapFile = new KFile(userOsmDir, osmFilePath); // .. then relative name of OSM file
            if (mapFile.Exists()) return mapFile;

            return null;
        }


        protected void OnApplicationQuit()
        {
            ExternalMapBuilder.ForceQuit();
        }
        
        
        public static bool LooksLikeLatLong(string s)
        {
            if (s == null) return false;
            s = s.Trim();
            if (s.Length < 20 || s.Length > 60) return false;
            
            char separator = CC.COMMA;
            if (s.IndexOf(separator) < 1) return false;

            double LATMAX = 85; // See also BoundsBox.LAT_MAX_PRACTICAL
            double LNGMAX = 180.001;
            string[] parts = KTools.SplitQuick(s, separator);
            
            if (parts.Length == 4)
            {
                double[] na = Numbers(parts);
                if (na == null) return false;

                if (-LATMAX > na[0] || na[0] > LATMAX) return false;
                if (-LNGMAX > na[1] || na[1] > LNGMAX) return false;
            }
           
            if (parts.Length == 2)
            {
                double[] na = Numbers(parts);
                if (na == null) return false;

                if (-LATMAX > na[0] || na[0] > LATMAX) return false;
                if (-LNGMAX > na[1] || na[1] > LNGMAX) return false;
            }
            
            // More tests could go here

            return true;
        }
        private static double[] Numbers(string[] parts)
        {
            double[] na = new double[parts.Length];
            for ( int i = 0; i < parts.Length; i++) 
            {
                try { na[i] = KTools.ParseDouble(parts[i]); }
                catch (FormatException) { return null; }
            }
            return na;
        }

    }
    
}