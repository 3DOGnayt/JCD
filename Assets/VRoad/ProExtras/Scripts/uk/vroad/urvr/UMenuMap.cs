using uk.vroad.api;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
    public class UMenuMap : UaGameMenu
    {
        public GameObject helpStoryPanel;
        public Text creditsText; 
        public GameObject messageReset;
        public Text helpText;
        public Sprite[] avatarSprites;
        
        protected override Text ControlsHelpText() { return  helpText; }

       
        private Button[] avatarButtons = NO_BUTTONS;
        private Image[] avatarImages = NO_IMAGES;

        private Button[] resetButtons = NO_BUTTONS;
        private Image[] resetImages = NO_IMAGES;
        
        private GameObject creditsPanel;
        
        private int resetCancelConfirm; // 0 == no buttons, 1 = cancel filled, 2 = confirm filled

        protected override App App() { return game; }
        private Game game;
        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
       
            creditsPanel = creditsText.transform.parent.gameObject;

            creditsText.text =
                Babyl.Translation(BabylKey.MapCredits) +
                Babyl.Translation(BabylKey.PhotoCreditIntro) +
                Babyl.Translation(BabylKey.PhotoCredits) +
                Babyl.Translation(BabylKey.MusicCredit) +
                Babyl.Translation(BabylKey.PluginCredit) +
                SC.N;

        }

        protected override void UpdateMenuFunction(AppDigitalFn fn, bool isSel)
        {
            base.UpdateMenuFunction(fn, isSel);
            
            if (fn == GameDigitalFn.MenuStory) helpStoryPanel.SetActive(isSel);
            
            if (fn == GameDigitalFn.MenuCredits) creditsPanel.SetActive(isSel);

            if (fn == GameDigitalFn.MenuAvatar)
            {
                int na = avatarImages.Length;
                for (int ai = 0; ai < na; ai++)
                {
                    Image image = avatarImages[ai];
                    image.gameObject.SetActive(isSel);

                    image.color = ai == UBotHandler.AvatarChoice() ? Color.white : Color.grey;
                }
            }
            
            if (fn == GameDigitalFn.MenuReset)
            {
                int nr = resetImages.Length;
                for (int ri = 0; ri < nr; ri++)
                {
                    Image image = resetImages[ri];

                    if (isSel && resetCancelConfirm > 0)
                    {
                        image.gameObject.SetActive(true);
                        image.fillCenter = (ri + 1) == resetCancelConfirm;

                        messageReset.SetActive(resetCancelConfirm == 2);
                    }
                    else
                    {
                        image.gameObject.SetActive(false);
                        messageReset.SetActive(false);
                    }
                }
            }
        }

        protected override AppState MenuState() { return AppState.MenuAtMapChoice; }
        
        protected override bool MenuItemPressed(AppDigitalFn fn)
        {
            if (base.MenuItemPressed(fn)) return true;
            
            if (fn == GameDigitalFn.MenuReset)
            {
                if (resetCancelConfirm == 0)
                {
                    resetCancelConfirm = 1; // show cancel and confirm
                    messageReset.SetActive(true);
                }
                else if (resetCancelConfirm == 1)
                {
                    messageReset.SetActive(false);
                    resetCancelConfirm = 0; // cancel: hide buttons
                }
                else if (resetCancelConfirm == 2)
                {
                    messageReset.SetActive(false);
                    ResetConfirm();
                    resetCancelConfirm = 0; // hide buttons
                }

                return true;
            }
            resetCancelConfirm = 0;

            return false;
        }

        protected override bool HandleMenuLeftRight(AppDigitalFn selFn, bool menuRight)
        {
            if (base.HandleMenuLeftRight(selFn, menuRight)) return true;
            
            bool menuLeft = !menuRight;

            if (selFn == GameDigitalFn.MenuAvatar)
            {
                int na = avatarImages.Length;
                int choice = UBotHandler.AvatarChoice();
                if (menuLeft && choice > 0) UBotHandler.ChooseAvatar(choice - 1);
                else if (menuRight && choice < na - 1)     UBotHandler.ChooseAvatar(choice + 1);
               
            }
            
            if (selFn == GameDigitalFn.MenuReset)
            {
                if (menuLeft && resetCancelConfirm > 0) { resetCancelConfirm--; return true; }
                if (menuRight && resetCancelConfirm < 2) { resetCancelConfirm++; return true; }
            }

            return false;
        }
        protected override void RebuildMenu()
        {
            // graphics buttons in base
            
            foreach (Button ab in avatarButtons) Destroy(ab.gameObject);
            avatarButtons = NO_BUTTONS;
            avatarImages = NO_IMAGES;
            
            foreach (Button rb in resetButtons) Destroy(rb.gameObject);
            resetButtons = NO_BUTTONS;
            resetImages = NO_IMAGES;

            base.RebuildMenu();
        }

        protected override void BuildSubMenu(AppDigitalFn fn, float lx, float y, float buttonWS, float buttonW, float buttonH)
        {
            base.BuildSubMenu(fn, lx, y, buttonWS, buttonW, buttonH);
            
            if (fn == GameDigitalFn.MenuReset)
            {
                BuildResetButtons(lx, y, buttonWS, buttonW, buttonH);
            }

            if (fn == GameDigitalFn.MenuAvatar)
            {
                BuildAvatarButtons(lx, y, buttonWS, buttonW, buttonH);
            }

        }

        private void BuildAvatarButtons(float lx, float y, float buttonWS, float buttonW, float buttonH)
        {
            int na = avatarSprites.Length;
            
            avatarImages = new Image[na];
            avatarButtons = new Button[na];
            
            float x = lx;

            for (int ai = 0; ai < na; ai++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject buttonGO = Instantiate(menuButtonPrefab, position, Quaternion.identity, helpPanel.transform);
                RectTransform rt = buttonGO.GetComponentInChildren<RectTransform>();
                rt.sizeDelta = new Vector2(buttonW, buttonH);
                Text buttonLabel = buttonGO.GetComponentInChildren<Text>();
                buttonLabel.text = SC.N;
                int index = ai; // cannot use loop variable, this is equivalent of 'final int'
                Button button = buttonGO.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    UBotHandler.ChooseAvatar(index);
                });
                avatarButtons[ai] = button;
                avatarImages[ai] = buttonGO.GetComponent<Image>();
                avatarImages[ai].sprite = avatarSprites[ai];
                
                x += buttonW + buttonWS;
            }
        }

        private void BuildResetButtons(float lx, float y, float buttonWS, float buttonW, float buttonH)
        {
            string cancel = Babyl.Translation(BabylKey.Opt_Cancel);
            string confirm = Babyl.Translation(BabylKey.Opt_Confirm);
            
            string[] names = { cancel, confirm, };
            int nR = names.Length;
            resetButtons = new Button[nR];
            resetImages = new Image[nR];
            
            float x = lx;

            for (int ri = 0; ri < nR; ri++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject buttonGO = Instantiate(menuButtonPrefab, position, Quaternion.identity, helpPanel.transform);
                RectTransform rt = buttonGO.GetComponentInChildren<RectTransform>();
                rt.sizeDelta = new Vector2(buttonW, buttonH);
                Text buttonLabel = buttonGO.GetComponentInChildren<Text>();
                buttonLabel.text = names[ri];
                int index = ri; // cannot use loop variable, this is equivalent of 'final int'
                Button button = buttonGO.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    ResetFunction(index);
                });
                resetImages[ri] = buttonGO.GetComponent<Image>();
                resetButtons[ri] = button;
                
                x += buttonW + buttonWS;
            }

            resetImages[1].color = Color.red;
            
                                            
            RectTransform mrt = messageReset.GetComponentInChildren<RectTransform>();
            mrt.anchoredPosition =  new Vector2(lx,y - (2f * buttonH));
        }

        // This is called when you mouse click on either cancel (index==0) or confirm (1) buttons
        private void ResetFunction(int index)
        {
            if (index == 0) // If you click on cancel, then hide button, even if confirm is filled in
            {
                messageReset.SetActive(false);
                resetCancelConfirm = 0; // cancel: hide buttons
            }
            else if (index == 1)
            {
                if (resetCancelConfirm == 2)
                {
                    messageReset.SetActive(false);
                    ResetConfirm();
                    resetCancelConfirm = 0; // hide buttons
                }
                else
                {
                    resetCancelConfirm = 2; // fill in confirm button, 
                }
            }

        }
        
        
        public void ResetConfirm()
        {
            KPrefs.ClearEverything();
            
#if VROAD_RVR_RELEASE  
            SteamRoboVanRush.ResetAllScoresAndAchievementsOnSteam();
#endif
            game.Lm().ResetCustomMaps();
            UExitHandler.AppExitStatic(App());
        }
    }
}
