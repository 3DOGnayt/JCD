using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.geom;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UBotHandler : UaBotHandler
    {
        protected static Rng.Vein VEIN_SHAPES = Rng.Vein.SHAPES;

        public GameObject uPlayerTop;
        public UAutoResume autoResume;

#if VROAD_RVR_RELEASE
        public Material darkerSkin;
        private int nLight;
        private int nDark;
#endif
        
        private const int sizeOfMedicGroup = 30;
        private const int sizeOfCrowdGroup = 60;
        private const int sizeOfUnusualGroup = 7;
        // comedy group is remainder: 100-sum


        private static int avatarChoice = -1;
        private static readonly int BEAT_MULTIPLIER = Animator.StringToHash(SG.Anim_beatMultiplier);

        private bool justStartedPlaying;

        private Game game;
        
        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
        }
        protected override App App() { return game; }

        protected override AppStateTransition StartSimTransition() { return GameStateTransition.startSimulation; }

        public override void AppStateChanged(AppStateTransition ast)
        {
            if (ast == GameStateTransition.startNavigating)
            {
                justStartedPlaying = true;
            }
            else if (ast == GameStateTransition.escapeFromQuarterWait)
            {
                destroyAllUBotsOnUpdate = true;
            }
            else if (ast == GameStateTransition.escapeFromQuarterPlay)
            {
                destroyAllUBotsOnUpdate = true;
            }
            else if (ast == GameStateTransition.arrivalAtTargetLevelUp)
            {
                destroyAllUBotsOnUpdate = true;
            }
            else if (ast == GameStateTransition.removeGameOverMsg)
            {
                destroyAllUBotsOnUpdate = true;
               
            }
            
            base.AppStateChanged(ast);
        }


        protected override void FixedUpdate()
        {
            base.FixedUpdate();
           
            if (justStartedPlaying)
            {
                justStartedPlaying = false;

                bool showPlayHelpOnFirstOpen = KPrefs.GetInt(SF.PREFS_HIDE_INIT_HELP, 0) <= 2;
                if (showPlayHelpOnFirstOpen) //  && Gamepad.current != null)
                {
                    KPrefs.SetInt(SF.PREFS_HIDE_INIT_HELP, 3);

                    game.Ew().FireAppDigitalEvent(GameDigitalFn.Pause, true);
                    
                    autoResume.AutoResumeIn(15);
                }
            }
        }

       
        public static void ChooseAvatar(int index)
        {
            if (index < 0 || index > 4) return;
            
            avatarChoice = index;
            
            KPrefs.SetInt(SF.PREFS_AVATAR, index);
            KPrefs.Save();
        }

        public static int AvatarChoice()
        {
            if (avatarChoice < 0)
            {
                avatarChoice = KPrefs.GetInt(SF.PREFS_AVATAR, 0);
            }

            return avatarChoice;
        }
        
        protected override Transform parentTransform(IPed ped)
        {
            return game.IsPlayer(ped) ? uPlayerTop.transform : uPedsTop.transform;
        }

        protected override void ConfigureNewPed(ITrip trip, IPed ped, GameObject ubod)
        {
#if VROAD_RVR_RELEASE
            bool useDarkerSkin = game.IsPlayer(ped) ? PlayerUseDarkerSkin(trip) : UseDarkerSkin(trip);

            if (useDarkerSkin)
            {
                SkinnedMeshRenderer smr = ubod.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material = darkerSkin;
                nDark++;
            }
            else nLight++; 
            
            Animator animator = ubod.GetComponent<Animator>();
            animator.SetFloat(BEAT_MULTIPLIER, UAudioController.BeatMultiplier());
#endif

        }

        protected override int RandomAvatarIndex(IPed ped)
        {
#if VROAD_RVR_RELEASE
            if (game.IsPlayer(ped))
            {
                if (AvatarChoice() == 0)
                {
                    int ri = Rng.NextInt(VEIN_SHAPES, 100);
                    return ri % 2; // 0-1 are skaters (player) skater-man, skater-woman
                }

                // 1 = dark woman, 2 = fair man, 3 = dark man, 4 = fair woman 
                if (AvatarChoice() == 1 || AvatarChoice() == 4) return 1;
                return 0;
            }
            
            int g1 = sizeOfMedicGroup;
            int g2 = g1 + sizeOfCrowdGroup;
            int g3 = g2 + sizeOfUnusualGroup;
            // If g1,g2,g3 are greater than 100, this fails safely: no avatars in higher groups are generated
           
            // First dice is for character group
            int groupI = Rng.NextInt(VEIN_SHAPES, 100);
            int avatarI;                                                         // 0-1 are skaters (player)
            if      (groupI < g1) avatarI = 2 + Rng.NextInt(VEIN_SHAPES, 8);   // 2-9 are medics  (8)
            else if (groupI < g2) avatarI = 10 + Rng.NextInt(VEIN_SHAPES, 32); // 10-41 are crowd characters (32)
            else if (groupI < g3) avatarI = 42 + Rng.NextInt(VEIN_SHAPES, 15); // 42-56 are unusual characters (15)
            else /*            */ avatarI = 57 + Rng.NextInt(VEIN_SHAPES, 9);  // 57-65 are comedy characters (9)

            if (avatarI < 0 || avatarI >= prefabPeds.Length) avatarI = 2;
            
            return avatarI;
#else
            return 0;
#endif
    }
      
        private bool PlayerUseDarkerSkin(ITrip trip)
        {
            if (AvatarChoice() == 0) return UseDarkerSkin(trip);

            // 1 = dark woman, 2 = fair man, 3 = dark man, 4 = fair woman 
            if (AvatarChoice() == 1 || AvatarChoice() == 3) return true;
            return false;
        }
        
        private bool UseDarkerSkin(ITrip trip)
        {
            double rngv = Strand.RngDouble(trip, Strand.SKIN, 100);
            return rngv < game.Map().PcDarkerSkinByCountry();
        }
       
#if VROAD_RVR_RELEASE
        public string metrics()
        {
            return KFormat.Sprintf(SU.METRICS_04, nLight, nDark);
        }
#endif
        
        protected override Quaternion RotatePlayer(IPed ped, Quaternion rotation)
        {
            // While waiting for taxi, rotate avatar to face approaching taxi
            if (ped.IsWaitingAtTaxiBay() && !ped.IsAboard() && ped.Speed() < 0.1)
            {
                ITaxi taxiApproaching = ped.GetTaxiApproaching();

                if (taxiApproaching != null)
                {
                    Xyz f2 = taxiApproaching.Centre().Minus(ped.Centre()).Normalized();

                    return Quaternion.LookRotation(f2.ToVector3());
                }
            }

            return rotation;
        }

    }

}
