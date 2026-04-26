using uk.vroad.api;
using uk.vroad.api.events;
using uk.vroad.api.input;
using uk.vroad.api.str;
using uk.vroad.rvr;
using uk.vroad.ucm;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.UI;

namespace uk.vroad.urvr
{
   
    
    public abstract class UaGameMenu: UaMenu
    {
        
        protected abstract Text ControlsHelpText();

        public Image imageControlsXBox;
        public Image imageControlsPS4;
        public Image imageControlsKeyboard;
       
        private Image imageControlsActive;
        private GameObject helpControlsPanel;
        
        private const int GP_NONE = 0;
        private const int GP_XBOX = 1;
        private const int GP_SONY = 2;
        
        private Button[] graphicsButtons = NO_BUTTONS;
        private Image[] graphicsImages = NO_IMAGES;

        private int gamePadConnected;
        private double gamePadTestCountdown;

        protected override void Awake()
        {
            base.Awake();
            
                        
            imageControlsActive = imageControlsKeyboard; // so that we do not need to test for non-null
            TestForGamePadChange();
            SetActiveControls();
            
            helpControlsPanel = imageControlsActive.gameObject.transform.parent.gameObject;

        }

        protected override GameObject CentralPanel() { return helpControlsPanel; }

        protected override void Update()
        {
            gamePadTestCountdown -= Time.unscaledDeltaTime;
            if (gamePadTestCountdown < 0)
            {
                gamePadTestCountdown = 1.0;
                if (TestForGamePadChange())
                {
                    SetActiveControls();
                    rebuildMenuInTicks = 1;
                }

            }
            
            base.Update();
        }

        // This is called in Update to set colour/fill of menu buttons if selection has changed

        protected override void UpdateMenuFunction(AppDigitalFn fn, bool isSel)
        {
            base.UpdateMenuFunction(fn, isSel);
            
            if (fn == GameDigitalFn.MenuControls) helpControlsPanel.SetActive(isSel);

            if (fn == GameDigitalFn.MenuGraphics)
            {
                int ng = graphicsButtons.Length;
                if (isSel)
                {
                    Color stdCol = new Color(0.44f, 0.44f, 0.44f);
                    Color actCol = new Color(0, 0.44f, 0);
                    int currentQ = QualitySettings.GetQualityLevel();
                    for (int gi = 0; gi < ng; gi++)
                    {
                        bool activeQ = gi == currentQ;
                        graphicsImages[gi].fillCenter = activeQ;
                        graphicsImages[gi].color = activeQ ? actCol : stdCol;
                        graphicsImages[gi].gameObject.SetActive(true);
                    }
                }
                else
                {
                    for (int gi = 0; gi < ng; gi++)
                    {
                        graphicsImages[gi].gameObject.SetActive(false);
                    }
                }
            }

        }

        protected override void RebuildMenu()
        {
            foreach (Button gb in graphicsButtons) Destroy(gb.gameObject);
            graphicsButtons = NO_BUTTONS;
            graphicsImages = NO_IMAGES;

            bool menuActive = App().Asm().CurrentState() == MenuState();
            imageControlsActive.gameObject.SetActive(menuActive); 
            ControlsHelpText().gameObject.SetActive(menuActive);

            base.RebuildMenu();
        }

        protected override void BuildSubMenu(AppDigitalFn fn, float lx, float y, float buttonWS, float buttonW,
            float buttonH)
        {
            if (fn == GameDigitalFn.MenuGraphics)
            {
                BuildGraphicsButtons(lx, y, buttonWS, buttonW, buttonH);
            }
        }
        private void SetActiveControls()
        {
            imageControlsActive.gameObject.SetActive(false);

            switch (gamePadConnected)
            {
                case GP_XBOX: imageControlsActive = imageControlsXBox; break;
                case GP_SONY: imageControlsActive = imageControlsPS4; break;
                default:      imageControlsActive = imageControlsKeyboard; break;
            }

            imageControlsActive.gameObject.SetActive(true);
        }

        protected override bool HandleMenuLeftRight(AppDigitalFn selFn, bool menuRight)
        {
            bool menuLeft = !menuRight;

            if (selFn == GameDigitalFn.MenuGraphics)
            {
                int currentQ = QualitySettings.GetQualityLevel();
                int nQ = QualitySettings.names.Length;

                if (menuLeft && currentQ > 0)
                {
                    QualitySettings.SetQualityLevel(currentQ-1, true);
                    return true;
                }
                if (menuRight && currentQ < nQ - 1)
                {
                    QualitySettings.SetQualityLevel(currentQ+1, true);
                    return true;
                }
            }

            return base.HandleMenuLeftRight(selFn, menuRight);
        }

        private void BuildGraphicsButtons(float lx, float y, float buttonWS, float buttonW, float buttonH)
        {
            string[] names = QualitySettings.names;
            int nQ = names.Length;
            graphicsButtons = new Button[nQ];
            graphicsImages = new Image[nQ];

            float x = lx;

            for (int qi = 0; qi < nQ; qi++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject buttonGO = Instantiate(menuButtonPrefab, position, Quaternion.identity, helpPanel.transform);
                RectTransform rt = buttonGO.GetComponentInChildren<RectTransform>();
                rt.sizeDelta = new Vector2(buttonW, buttonH);
                Text buttonLabel = buttonGO.GetComponentInChildren<Text>();
                buttonLabel.text = names[qi];
                int index = qi; // cannot use loop variable, this is equivalent of 'final int'
                Button button = buttonGO.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    SelectGraphicsQuality(index);
                });
                graphicsImages[qi] = buttonGO.GetComponent<Image>();
                graphicsButtons[qi] = button;
                
                x += buttonW + buttonWS;
            }
        }

        private void SelectGraphicsQuality(int index)
        {
            QualitySettings.SetQualityLevel(index, true);
        }

        private bool TestForGamePadChange()
        {
            int gamePadWas = gamePadConnected;
            Gamepad gp = Gamepad.current;
            gamePadConnected = gp == null? GP_NONE: IsSonyGamepad(gp) ? GP_SONY : GP_XBOX;

            return gamePadConnected != gamePadWas;
        }
        
        
        public static bool IsSonyGamepad(Gamepad gp)
        {
            InputDeviceDescription desc = gp.description;
            bool isPS = desc.manufacturer.ToLower().Contains(SA.SONY);

            // Only real SONY gamepads will return this:
            // Some generic gamepads that have a layout like a sony PS4 return exactly the same information
            // as a generic gamepad with an XBox Layout, there appears to be no way of telling them apart.
           
            return isPS;
        }

    }
}