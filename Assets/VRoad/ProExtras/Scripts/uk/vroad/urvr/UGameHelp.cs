using System.Collections.Generic;
using System.Linq;
using uk.vroad.api;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    // This is active on the spinning earth scene, and changes the state to MenuAtMapChoice the very
    // first time the game is opened, after a delay (to allow the player to see the spinning earth)
    //
    // See also UMenuMap
    public class UGameHelp : MonoBehaviour
    {
        public Text missionText;
        public Slider missionSlider;
        public UAutoResume autoResume;
        
        private Game game;
        private float showControlsAt;
        
        private const float SHOW_MISSION_TIME = 10f;

        void Awake()
        {
            game = Game.AwakeInstance();
        }

        void Start()
        {
            bool showMission = KPrefs.GetInt(SF.PREFS_HIDE_INIT_HELP, 0) == 0;

            // test //showMission = true;
            
            if (showMission)
            {
                showControlsAt = Time.unscaledTime + SHOW_MISSION_TIME;
                missionText.transform.parent.gameObject.SetActive(true);
                KPrefs.SetInt(SF.PREFS_HIDE_INIT_HELP, 1);
            }
        }

        private void FixedUpdate()
        {
            if (showControlsAt > 0)
            {
                float now = Time.unscaledTime;
                float remaining = showControlsAt - now;
                bool choosingMap = game.Gsm().CurrentState() == AppState.WaitingForMapChoice;

                if (remaining < 0 || !choosingMap) // If ! choosingMap, must have already moved to another state: hide 
                {
                    showControlsAt = 0;
                    missionText.transform.parent.gameObject.SetActive(false);

                    if (choosingMap) //  && Gamepad.current != null) // show controls only if using a gamepad
                    {
                        game.Gsm().MakeTransition(AppStateTransition.openMapMenu);

                        autoResume.AutoResumeIn(10);
                    }
                }
                else missionSlider.value = (SHOW_MISSION_TIME - remaining) / SHOW_MISSION_TIME;
            }
        }

        /// <summary>
        /// Make one call to this from each scene. 
        ///
        /// In the release application, this reads the (translated) string for a Text field and applies it.
        /// The strings are keyed by the name of the text field.  It is possible for multiple text fields to
        /// have the same name, this is useful for common labels like OK, Cancel, etc. However, be careful not to
        /// unintentionally give two fields the same name, as there is only one hash, for all strings, not one
        /// hash per scene. 
        /// 
        /// In the editor, this saves the latest values created in the editor into the base language (en) folder
        /// into a file en/(scene).txt
        /// 
        /// </summary>
 
        public static void ReadTextFromBabyl(string sceneTopGO, Text[] babylTexts, Text[] noLangTexts)
        {
#if UNITY_EDITOR
            if (KEnv.LocalLanguageAndRegion().Equals(SC.EN_GB))
            {
#if VROAD_RVR_RELEASE             
                 if (UDbg.BABYL_SAVE)  // This is not available 
                 {
                     
                    int nb = babylTexts.Length;
                    Babyl[] babyls = new Babyl[nb];
                    for (int bi = 0; bi < nb; bi++)
                    {
                        string key = babylTexts[bi].name;
                        string value = babylTexts[bi].text;
                        babyls[bi] = new Babyl(key, value, false);
                    }
 
                    Babyl.Save(babyls, sceneTopGO);
                    
                 }
#endif
                
                // Iterate over all UI Text objects in this scene, checking that they are in one of two arrays
                List<GameObject> rootObjects = new List<GameObject>();
                Scene scene = SceneManager.GetActiveScene();
                scene.GetRootGameObjects( rootObjects );

                foreach (GameObject go in rootObjects)
                {
                    Text[] goTexts = go.GetComponentsInChildren<Text>(true);

                    foreach (Text text in goTexts)
                    {
                        if (! babylTexts.Contains(text) && !noLangTexts.Contains(text))
                        {
                            bool missing = true;
                            
                            if (text.name.StartsWith(SG.KEYS_PREFIX)) // Might be a duplicated Keys*  item
                            {
                                foreach (Text babylText in babylTexts)
                                {
                                    if (babylText == null) continue; // empty element in array, component deleted?
                                    
                                    if (babylText.name.Equals(text.name))
                                    {
                                        missing = false;
                                        break;
                                    }
                                }
                            }
                            
                            if (missing) SU.Report(SU.BABYL_MISSING_UI, sceneTopGO, text.name, text.text);
                        }
                    }
                }
                
            }
            else // We are translating, check that key-value pair is in file
            {
                foreach (Text babylText in babylTexts)
                {
                    if (babylText == null) continue; // empty element in array, component deleted?
                    
                    if (!Babyl.TranslationExists(babylText.name))
                    {
                        SU.Report(SU.BABYL_MISSING_PAIR, sceneTopGO, babylText.name);
                    }
                }
            }
#endif

            foreach (Text babylText in babylTexts)
            {
                if (babylText == null) continue; // empty element in array, component deleted?
                
                string localtext = Babyl.Translation(babylText.name);
                babylText.text = localtext;

                if (babylText.name.StartsWith(SG.KEYS_PREFIX))
                {
                    // For controller diagrams, there are alternative layouts for XBox, PS4 and Keyboard controls
                    // and within those there are Text objects with the same name. So for example there are three 
                    // text objects named KeysPlayer, and these three objects have the same grandparent
                    GameObject grandParent = babylText.transform.parent?.parent?.gameObject;
                    if (grandParent == null) continue;

                    Text[] cousinTexts = grandParent.GetComponentsInChildren<Text>(true);
                    foreach (Text cousin in cousinTexts)
                    {
                        if (cousin != babylText && cousin.name.Equals(babylText.name)) cousin.text = localtext;
                    }
                }
            }

        }
    }
}
