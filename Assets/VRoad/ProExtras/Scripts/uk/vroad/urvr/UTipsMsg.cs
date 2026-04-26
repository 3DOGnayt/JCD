using System.Collections.Generic;
using uk.vroad.api;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UTipsMsg : MonoBehaviour
    {
        public enum StateIndicator
        {
            WaitingForTripChoice,
            Navigating,
        }

        public GameObject visibleParent;
        public GameObject fullTextPanel;  // Set this to the tip panel
        public StateIndicator stateIndicator;
        
        private AppState activeState;
        private Text fullText;
        private Text floatMsgText;

        private Game game;
        private int countdownShowing;
        private int countdownBlank;
        private string[] messageLines = new string[0];
        private int nextLineIndex;
        private bool readyToShowNext;
        
        private const int TICKS_BLANK_INIT = 500;
        private const int TICKS_BLANK_INTER = 500;
        private const int TICKS_SHOWING = 750;

        private void Awake()
        {
            game = Game.AwakeInstance();
            floatMsgText = visibleParent.GetComponentInChildren<Text>();
            fullText = fullTextPanel.GetComponentInChildren<Text>();

            if (stateIndicator == StateIndicator.WaitingForTripChoice) activeState = GameState.WaitingForTripChoice;
            else if (stateIndicator == StateIndicator.Navigating) activeState = GameState.Navigating;
            
            string fullTextPara = Babyl.Translation(fullText.name);

            List<string> nonBlanks = new List<string>();
            string[] linesIncBlank = KTools.SplitQuick(fullTextPara, CC.NEWLN);
            foreach (string line in linesIncBlank)
            {
                string lineT = line.Trim();
                if (lineT.Length == 0) continue;

                // Remove final full stop
                if (lineT.EndsWith(SC.N + CC.DOT)) lineT = lineT.Substring(0, lineT.Length - 1);
                
                nonBlanks.Add(lineT);
            }

            messageLines = nonBlanks.ToArray();
            countdownBlank = TICKS_BLANK_INIT;
            
            visibleParent.SetActive(false);

            string prefsKey = ActiveStateTipsPref();
            if (prefsKey != null) nextLineIndex = KPrefs.GetInt(prefsKey, 0);

            if (UDbg.TIPS) nextLineIndex = 0;
            if (UDbg.TIPS) SU.Report(SU.TIPS_01, activeState, nextLineIndex);
        }

        private string ActiveStateTipsPref()
        {
            if (activeState == GameState.WaitingForTripChoice) return SF.PREFS_HELP_TIPS_TRIP;
            if (activeState == GameState.Navigating) return SF.PREFS_HELP_TIPS_PLAY;
            return null;
        }
        void FixedUpdate()
        {
            if (game.Gsm().CurrentState() != activeState || nextLineIndex >= messageLines.Length)
            {
                if (visibleParent.activeSelf) visibleParent.SetActive(false);
                return;
            }

            if (countdownShowing > 0)
            {
                countdownShowing--;

                if (countdownShowing == 0)
                {
                    visibleParent.SetActive(false);
                    nextLineIndex++;

                    string prefsKey = ActiveStateTipsPref();
                    if (prefsKey != null) KPrefs.SetInt(prefsKey, nextLineIndex);

                    if (UDbg.TIPS) SU.Report(SU.TIPS_02, activeState, nextLineIndex);

                    if (nextLineIndex >= messageLines.Length)
                    {
                        if (UDbg.TIPS) SU.Report(SU.TIPS_03, activeState);
                        return;
                    }
                    
                    countdownBlank = TICKS_BLANK_INTER;
                }
                
                // In case tip was showing, but user paused and then resumed
                else if (!visibleParent.activeSelf) visibleParent.SetActive(true);
            }

            if (countdownBlank > 0)
            {
                countdownBlank--;

                if (countdownBlank == 0)
                {
                    readyToShowNext = true;
                }
            }

            if (readyToShowNext)
            {
                bool showNow = true;
                IPed playerPed = Player.ActivePlayer()?.Ped();
                // Add conditions for showing tip here - e.g. Taxi tips only when in a taxi
                if (activeState == GameState.Navigating && playerPed != null)
                {
                    if (nextLineIndex == 5 && (!playerPed.IsBlocked() || !(playerPed.IsWaitingAtCrossing()))) showNow = false;
                    
                    if (nextLineIndex == 6 && (playerPed.GetWalkway() == null || ! playerPed.GetWalkway().HasTaxiZone())) showNow = false;
                    
                    if (nextLineIndex == 7 && playerPed.GetTaxi() == null) showNow = false;
                    
                    if (nextLineIndex == 8 && (playerPed.GetWalkway() == null || ! playerPed.GetWalkway().HasStops())) showNow = false;

                    // Show 'choose bus route' tip only if bus stop has more than one route
                    if (nextLineIndex == 9 && (!playerPed.IsWaitingAtBusStop() || playerPed.CurrentHalt().GetStop().HaltsWithArrivals() < 2)) showNow = false;
                    
                    if (nextLineIndex == 10 && (! playerPed.IsAboard() || playerPed.GetBus() == null)) showNow = false;
                    
                    if (nextLineIndex == 11 && ! playerPed.IsAboard()) showNow = false;
                }

                if (showNow)
                {
                    floatMsgText.text = messageLines[nextLineIndex];
                    countdownShowing = TICKS_SHOWING;
                    visibleParent.SetActive(true);
                    readyToShowNext = false;
                }

            }
        }

    }
}
