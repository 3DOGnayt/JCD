using uk.vroad.api;
using uk.vroad.rvr;
using uk.vroad.ucm;
using uk.vroad.api.input;
using uk.vroad.api.str;

using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace uk.vroad.urvr
{
    // This is attached to an always-on but invisible game object, UGameConsole
    //
    // We cannot attach it to an object under DebugUI, because we want that whole hierarchy to be
    // inactive by default. An inactive object does not call Awake(), so this would never register
    // for the event that would make it active
    public class UGameConsole : MonoBehaviour, LAppInput
    {
        public GameObject debugUI;
        public Text consoleText;

        private static readonly StringBuilder msgBuffer = new StringBuilder();
        private static bool refreshRequired;
        private static bool visible;

        private Game game;
       void Awake()
        {
            game = Game.AwakeInstance();
            msgBuffer.Append(SC.S + CC.MORE + SC.NL);
            refreshRequired = true;

           game.AddEventConsumer(this);
        }

        
        public bool DeregisterFireMapChange() { return true; } // Must deregister this on scene unload
       
        void Update()
        {
            if (refreshRequired)
            {
                refreshRequired = false;

                if (IsVisible()) consoleText.text = MsgBuffered();
                
                if (IsVisible() != debugUI.gameObject.activeSelf) debugUI.gameObject.SetActive(IsVisible());
            }

        }

        public static void SetVisible(bool v)
        {
            visible = v;
            refreshRequired = true;
        }
        
        private static bool IsVisible()  { return visible; }
        
        public static void Log(string s)
        {
            if (s == null) return;
            lock (msgBuffer)
            {
                msgBuffer.Append(s + SC.NL);
                refreshRequired = true;
            }
           
        }

        private static string MsgBuffered()
        {
            lock (msgBuffer)
            {
                return msgBuffer.ToString();
            }
        }
        public bool AppInputDigitalEvent(AppDigitalFn dfn, bool isOn)
        {
            if (dfn == AppDigitalFn.Console)
            {
                SetVisible(!IsVisible());
            }

            else if (dfn == AppDigitalFn.CopyConsoleToClipboard && visible)
            {
                TextEditor textEditor = new TextEditor {text = consoleText.text};
                textEditor.SelectAll();
                textEditor.Copy();

                Log(SA.COPY_TO_CLIPBOARD);
            }

            return false;
        }

        public bool AppInputAnalogEvent(AppAnalogFn afn, double value)  { return false; }
    }
}
