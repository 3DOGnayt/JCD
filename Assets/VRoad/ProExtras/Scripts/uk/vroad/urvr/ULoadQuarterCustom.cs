namespace uk.vroad.urvr
{
    public class ULoadQuarterCustom : ULoadQuarter
    {
        private bool buildMapLater;

        private bool IsActive()
        {
            // If it has not been Awakened then lm will be null
            return lm != null && lm.CustomQuarterActive();  // for the current level
        }
        protected override void Awake()
        {
            base.Awake();
            quarterIndex = 5;
        }

        protected override bool QuarterMapExists()
        {
            return lm.CustomBuildExists();
        }
        protected override bool CanLoadQuarter()
        {
            return lm.CustomBuildExists() && base.CanLoadQuarter();
        }

        protected override void TryBuildMap()
        {
            buildMapLater = true;
        }
        
        protected override void LateUpdate() 
        {
            if (buildMapLater)
            {
                buildMapLater = false;

                if (quarterIndex != lm.SelectedQuarter()) return; // should not be here unless they match
                if (lm.CustomBuildExists()) return; // should not be here if custom map already built
            
                lm.SetSelectedQuarter(0);
                
                UChooseQuarter.InstanceQ().ShowBrowser();
                
                // If user clicks OK in browser to build map, this will call UChooseQuarter.Instance().BuildNewModel()
                // The buildClicked callback is setup in BrowserCallback.Awake()
            }
            else
            {
                base.LateUpdate(); // follow this path if custom map was previously built
            }
        }

        public override void LoadSnapshot()
        {
            if (IsActive()) 
                base.LoadSnapshot();
            
            // See UGameStateHandler.TakeScreenShotCo()
        }

        public override void SetColourAndSize()
        {
            if (IsActive()) base.SetColourAndSize();
        }
         
      

    }
}
