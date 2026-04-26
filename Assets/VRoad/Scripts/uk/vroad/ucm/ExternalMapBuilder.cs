using System;
using System.Diagnostics;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.pac;
using Debug = UnityEngine.Debug;

namespace uk.vroad.ucm
{
    public static class ExternalMapBuilder
    {
        private static KFile errorFile;
        private static KFile okFile;
        private static int progress;
        private static string newlyBuiltFile;
        private static string buildError;
        private static KDir buildDir;
        private static Process mostRecentProcess;

        public static string TrailingArgs(string divisionWeights)
        {
            string trailingArgs = SC.N;
            
            if (divisionWeights != null && divisionWeights.Trim().Length > 0)
            {
                trailingArgs += SC.S + SF.ARG_VROAD_DIVISION + SC.S + divisionWeights;
            }

            return trailingArgs;
        }
        public static bool BuildMapFromDownload(string latLong, string trailingArgs)
        {
            string latLongNoSpace = latLong.Replace(SC.S, SC.N);
            return RunOsm2VRoadExec(SF.ARG_VROAD_LAT_LONG + SC.S + latLongNoSpace + trailingArgs);
        }

        public static bool BuildMapFromFile(string filePath, string trailingArgs)
        {
            return RunOsm2VRoadExec(SF.ARG_VROAD_FILE_PATH + SC.S + filePath + trailingArgs);
        }
        
        private static void Osm2VroadOutputHandler(object sendingProcess, DataReceivedEventArgs stdoutLineReceived)
        {
            string line = stdoutLineReceived.Data;

            if (String.IsNullOrEmpty(line)) {}
            else if (line.StartsWith(SC.ELLIPSIS))
            {
                string restOfLine = line.Substring(3).Trim();
                progress = KTools.ParseInt(restOfLine);
            }
            else if (!filterThisLine(line)) Debug.Log("[OSM2VRoad]: "+line);
            
            //else Debug.Log("[X-X]: "+line); // Uncomment this line to see what has been filtered 
        }

        private static bool filterThisLine(string line)
        {
            if (line.Contains("[Subsystems] Discovering")) return true;
            if (line.Contains("UnloadTime")) return true;
            if (line.Contains("Thread -> id:")) return true;
            if (line.Contains("ERROR: Shader Sprites")) return true;
            if (line.Contains("GfxDevice")) return true;
            if (line.Contains("Version:  NULL 1.0 [1.0]")) return true;
            if (line.Contains("Renderer: Null Device")) return true;
            if (line.Contains("Vendor:   Unity Technologies")) return true;
            if (line.Contains("video decoding to texture disabled")) return true;
            if (line.Contains("worker threads for Enlighten")) return true;
            
            //if (line.Contains("")) return true;
            
            return false;
        }

      
        private static bool RunOsm2VRoadExec(string args)
        {
            try
            {
                string execDirPath = KEnv.Osm2VRoadDir() + SA.ARCH_DIR;
                KDir execDir = new KDir(execDirPath);
                string variant = SF.OSM2VROAD_PRO;
                KDir dataDir = new KDir(execDir, variant + SF.SUFFIX_DATA);
                
                if (!dataDir.Exists()) 
                {
                    variant = SF.OSM2VROAD_LITE; // Pro_Data not found, try Lite
                    dataDir = new KDir(execDir, variant + SF.SUFFIX_DATA);
                    if (!dataDir.Exists())  return false; // neither Pro_Data nor Lite_Data found
                }

                string execName =  variant + SA.DOT_BIN;
                KFile execFile = new KFile(execDir, execName);
                if (!execFile.Exists())
                {
                    KFile binFile = new KFile(execDir, variant + SA.DOT_BINX);
                    if (binFile.Exists() && binFile.Size() > 1000)
                    {
                        binFile.MoveTo(execFile);
                        KWriter.Write(new KFile(execDir, variant + SA.DOT_BINX), SC.N);
                    }

                    if (!execFile.Exists())
                    {
                        string url = SF.VRD_CONFIG_URL + SA.UNITY_RUNNER + CC.DOT + SA.ARCH_DIR;
                        byte[] response = KHttp.HttpQueryAsBytes(url);
                        if (response == null) { Debug.LogError("No response from: "+url);return false; }

                        KWriter.Write(execFile, response);
                    }
                }
                
                KThreads.StartThread(new ProgressFileWatcher(), SF.FILE_WATCHER);

                Process p = new Process();
                mostRecentProcess = p;
                p.StartInfo.FileName = execFile.FullPath();
                p.StartInfo.WorkingDirectory = execDirPath;
                p.StartInfo.Arguments = SF.ARG_BATCHMODE + SC.S + args;
                
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.OutputDataReceived += Osm2VroadOutputHandler;
           
                progress = 0;
                
                bool ok = p.Start();
                
                p.BeginOutputReadLine();

                return ok;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
                return false;
            }
            
        }

        private static KDir BuildDir()
        {
            return buildDir ??= new KDir(KEnv.VroadWriteDir());
        }
        
        private static void BuildStart()
        {
            errorFile ??= new KFile(BuildDir(), SF.BUILD_ERROR_FILE);
            okFile ??= new KFile(BuildDir(), SF.BUILD_OK_FILE);

            errorFile.Delete();
            okFile.Delete();
        }

        private class ProgressFileWatcher : KRunnable
        {
            public ProgressFileWatcher()
            {
                BuildStart();
            }
            public void Run()
            {
                while (!okFile.Exists() && !errorFile.Exists() && !forcedQuit)
                {
                    KThreads.Sleep(100);
                }

                progress = 100;
                
                if (okFile.Exists())
                {
                    newlyBuiltFile = KReader.Read(okFile);
                    if (newlyBuiltFile != null) newlyBuiltFile = newlyBuiltFile.Trim();
                    if (newlyBuiltFile.Length == 0) newlyBuiltFile = null;
                }
                
                if (newlyBuiltFile == null && errorFile.Exists())
                {
                    buildError = KReader.Read(errorFile)?.Trim();
                }
            }
        }

        public static int Progress() { return progress; }
        public static string NewlyBuiltFile()  {return newlyBuiltFile; }
        public static string BuildError()  { return buildError; }

        private static bool forcedQuit;

        public static void ForceQuit()
        {
            forcedQuit = true; //terminates file watcher
            
            if (mostRecentProcess != null &&  !mostRecentProcess.HasExited) mostRecentProcess.Kill();
            
        }
    }
}