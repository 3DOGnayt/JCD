using System;
using System.Collections.Generic;
using uk.vroad.api;
using uk.vroad.api.enums;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.route;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;

using uk.vroad.pac;
using UnityEngine;
using UnityEngine.Serialization;

namespace uk.vroad.ucm
{
    public abstract class UaBotHandler : MonoBehaviour, LAppState, LBotDepart, LBotArrive
    {
        public GameObject[] prefabPeds;
        public GameObject[] prefabCars;
        public GameObject[] prefabTaxis;
        public GameObject[] prefabBuses;
        public GameObject[] prefabCoaches;
        public GameObject[] prefabRigidTrucks;
        public GameObject[] prefabTractorTrucks;
        public GameObject[] prefabTrailers;

        protected abstract AppStateTransition StartSimTransition();
        
        public static UaBotHandler Instance { get; private set; }
        private static readonly int IS_IDLE_ACTIVE = Animator.StringToHash(SA.Anim_isIdleActive);
        protected static readonly int IS_WALKING = Animator.StringToHash(SA.Anim_isWalking);

        private static readonly Vector3 PED_LIFT_ABOARD_TAXI = new Vector3(0, 0.25f, 0);
        private static readonly Vector3 PED_LIFT_ABOARD_BUS = new Vector3(0, 0.40f, 0);
        
        protected GameObject uPedsTop;
        private GameObject uCarsTop;
        private GameObject uCoachesTop;
        private GameObject uTrucksTop;
        private GameObject uTaxisTop;
        private GameObject uBusesTop;
        
        protected SimBrake simBrake;

        protected bool destroyAllUBotsOnUpdate;
        readonly List<IBot> justDeparted = new List<IBot>();
        readonly List<IBot> justArrived = new List<IBot>();
        
        readonly KHash<IBit, GameObject> activeBotToUBot = new KHash<IBit, GameObject>();
        readonly KHash<GameObject, IBit> activeUBotToBot = new KHash<GameObject, IBit>();
        readonly KHashSet<IPed> walkingPeds = new KHashSet<IPed>();

        private const int probabilityOfIdleAction = 50;

        private bool foundParentGameObjects;

        private KHash<IType, GameObject> typeToPrefab = new KHash<IType, GameObject>();
        
        protected abstract App App();
        
        protected virtual void Awake()
        {
            Instance = this;
            App().AddEventConsumer(this);

            if (transform.childCount >= 6)
            {
                uPedsTop = transform.GetChild(0).gameObject;
                uCarsTop = transform.GetChild(1).gameObject;
                uCoachesTop = transform.GetChild(2).gameObject;
                uTrucksTop = transform.GetChild(3).gameObject;
                uTaxisTop = transform.GetChild(4).gameObject;
                uBusesTop = transform.GetChild(5).gameObject;
                
                foundParentGameObjects = true;
            }
            else Debug.Log(SA.NO_BOT_PARENTS);

            CreateDrvTypesRigid(prefabCars, Purpose.Car);
            CreateDrvTypesRigid(prefabTaxis, Purpose.Taxi);
            CreateDrvTypesRigid(prefabRigidTrucks, Purpose.Truck);
            CreateDrvTypesRigid(prefabCoaches, Purpose.Coach);
            
            IType[] trailers = CreateDrvTypesTrailer(prefabTrailers, Purpose.Truck);
            
            CreateDrvTypesTractor(prefabTractorTrucks, Purpose.Truck, trailers);
            
            CreateBusTypes(prefabBuses);

        }

        protected virtual void FixedUpdate()
        {
            UpdateDisplay();

        }
        public bool DeregisterFireMapChange()
        {
            return true;
        }

        public virtual void AppStateChanged(AppStateTransition ast)
        {
            if (ast == StartSimTransition() && simBrake == null)
            {
                simBrake = new SimBrake(App());
            }
            else if (ast.after == AppState.ReadyToSimulate)
            {
                App().Sim().SetCountPedAvatars(prefabPeds.Length);
            }
        }



        public IBit LookupBit(GameObject go)
        {
            if (go == null) return null;
            return activeUBotToBot[go];
        }
        public GameObject LookupUBod(IBit bit)
        {
            if (bit == null) return null;
            return activeBotToUBot[bit];
        }

        private bool finished;
        
        private void UpdateDisplay()
        {
            if (destroyAllUBotsOnUpdate)
            {
                destroyAllUBotsOnUpdate = false;
                GameObject[] keys = activeUBotToBot.KeysAsArray();
                foreach (GameObject vgo in keys)
                {
                    Destroy(vgo);
                }

                activeUBotToBot.Clear();
                activeBotToUBot.Clear();
                
                justDeparted.Clear();
                justArrived.Clear();

                finished = true;
            }

            if (simBrake != null)
            {
                if (finished || simBrake.StepResultsWaiting())
                {
                    if (!finished) CreateMoveDestroyUBots();

                    simBrake.RenderingCompleted();
                }
            }
        }

        protected virtual void ConfigureNewPed(ITrip trip, IPed ped, GameObject ubod)
        {
            
        }

        private void CreateMoveDestroyUBots()
        {
            if (!foundParentGameObjects) return;
            
            IBot[] jda;
            IBot[] jaa;

            lock (this)
            {
                jda = justDeparted.ToArray();
                justDeparted.Clear();

                jaa = justArrived.ToArray();
                justArrived.Clear();
            }


            // CREATE NEW
            foreach (IBot bot in jda)
            {
                if (activeBotToUBot.TryGetValue(bot, out GameObject _))
                {
                    // probably a vehicle being re-used for another trip
                    continue;
                }

                ITrip trip = bot.GetTrip();
                if (trip == null) continue;
                
                string tripName = trip.ToString();
                switch (bot)
                {
                    case IPed ped:
                    {
                        int pi = RandomAvatarIndex(ped); 

                        GameObject ubod = CreateNewUBod(bot, prefabPeds[pi], tripName);
                        
                        
                        walkingPeds.Add(ped);
                        ped.AvatarType(1+pi);

                        ConfigureNewPed(trip, ped, ubod);
                        
                        break;
                    }
                    case IVkl vkl:
                    {
                        //IVkl vkl = vkl1;
                        IType type = vkl.GetBitType();

                        bool addPassengers = type.IsBus();

                        if (type.IsBus() && !(vkl is IBus))
                        {
                            Reporter.Sink(vkl);
                        }
                        
                        GameObject prefab = typeToPrefab.Get(type);
                        if (prefab == null) break;
                        
                        GameObject uVkl = CreateNewUBod(bot, prefab, tripName);

                        IBit puller = bot;
                        int trailerIndex = 1;
                        while (puller.GetAttachment() != null)
                        {
                            IBit trailer = puller.GetAttachment();
                            IType trailerType = trailer.GetBitType();
                            
                            GameObject trailerPrefab = typeToPrefab.Get(trailerType);
                            if (trailerPrefab == null) break;
                            
                            CreateNewUBod(trailer, trailerPrefab, tripName +SC.TRAILER_SUFFIX +trailerIndex);

                            puller = trailer;
                            trailerIndex++;
                        }

                        if (addPassengers)
                        {
                            IBus bus = (IBus)vkl;
                            int[] seatNos = bus.BallastRiderSeatNos();
                            int ns = seatNos.Length;
                            int npp = prefabPeds.Length;
                            
                            // Each seat that has a ballast passenger should (internally) store a negative value
                            // which is negated in bus.BallastAvatarType to return a positive value 1..N
                            for (int si = 0; si < ns; si++)
                            {
                                int seatNo = seatNos[si];
                                int bpti = bus.BallastAvatarType(seatNo); // returns value 1..NPP
                                int pi = bpti - 1; // reduce value to 0 .. NPP-1

                                if (pi < 0 || pi >= npp) 
                                    pi = 0;
                                
                                // The ballast passenger is created as a child of the uBus object (transform)
                                // so it does not need to be moved by our code when the bus moves,
                                // that will happen automatically
                               
                                CreateNewBallastPassenger(bus, seatNo, prefabPeds[pi], uVkl);
                            }
                        }

                        break; // end case
                    }
                }
            }

            // DESTROY ARRIVED
            foreach (IBot bot in jaa)
            {
                if (bot is ITaxi taxi)
                {
                    // when a Taxi arrives at a parking zone, do not destroy it
                    if (taxi.GetDestination() is ITaxiZone) continue;

                    if (taxi.GetRoad().GetZone() is ITaxiZone)
                    {
                        continue;
                    }
                }

                if (bot is IPed ped) 
                    walkingPeds.Remove(ped);

                if (activeBotToUBot.TryGetValue(bot, out GameObject ubot))
                {
                    activeBotToUBot.Remove(bot);
                    activeUBotToBot.Remove(ubot);
                    Destroy(ubot);
                }

                IBit rearmost = bot;
                while (rearmost.GetAttachment() != null)
                {
                    IBit trailer = rearmost.GetAttachment();

                    if (activeBotToUBot.TryGetValue(trailer, out GameObject uTrailer))
                    {
                        activeBotToUBot.Remove(trailer);
                        activeUBotToBot.Remove(uTrailer);
                        Destroy(uTrailer);
                    }
                    rearmost = trailer;
                }
            }

            foreach (IBit bit in activeBotToUBot.Keys)
            {
                if (bit is IBot bot)
                {
                    if (bot.Finished())
                    {
                        justArrived.Add(bot); // this will destroy the u
                        continue;
                    }
                }

                GameObject ubot = activeBotToUBot[bit];

                if (ubot == null || !ubot.activeSelf) continue;

                Vector3 position;
                Quaternion rotation;
                
                if (bit is IPed ped)
                {
                    position = ped.Centre().ToVector3(); 
                    rotation = Quaternion.LookRotation(ped.Forward().ToVector3());

                    if (ped.IsAboard())
                    {
                        if (ped.GetBus() != null) position += PED_LIFT_ABOARD_BUS;
                        else position += PED_LIFT_ABOARD_TAXI;
                        
                        // Could change animation to be seated here
                    }

                    if (App().IsPlayer(ped)) 
                    {
                        rotation = RotatePlayer(ped, rotation);

                        CameraFollowsPed(ped, position);
                    }
                    
                    // Animation setting for peds 
                    Animator animator = ubot.GetComponent<Animator>();
                    if (animator != null)
                    {
                        bool wasWalking = walkingPeds.Contains(ped);
                        bool isActivelyBoarding = false;

                        if (ped.IsBoarding())
                        {
                            IBus bus = ped.GetBus();
                            ITaxi taxi = ped.GetTaxi();
                            isActivelyBoarding = bus != null || (taxi != null && ped.TaxiBoardingQuotient(taxi) > 0);
                        }

                        bool isWalking = isActivelyBoarding || (!ped.IsAboard() && ped.Speed() > 0.1);

                        if (wasWalking != isWalking)
                        {
                            if (wasWalking) walkingPeds.Remove(ped);
                            else walkingPeds.Add(ped);

                            animator.SetBool(IS_WALKING, isWalking);

                            
                            if (isWalking)
                            {
                                animator.SetBool(IS_IDLE_ACTIVE, false);
                            }
                            else
                            {
                                if ( Rng.NextInt(Rng.Vein.SHAPES, 100) < probabilityOfIdleAction)
                                {
                                    animator.SetBool(IS_IDLE_ACTIVE, true);
                                    //idleAnimationStart.Put(ped, now);
                                }
                            }
                        }

                        else if (!isWalking)
                        {
                            // Set 'Has Exit Time' on Transition IdleActive -> Idle, and set Exit Time parameter
                            // in fold-down 'Settings' to 1.0 so that the animation plays one loop before checking
                            // on the boolean parameters.
                            //
                            // Thus even if we set idleActive to false here (which will happen on the next call in to 
                            // this method after the idle animation starts, this will no cause immediate termination of
                            // the animation
                            
                            animator.SetBool(IS_IDLE_ACTIVE, false);
                            
                            /*
                            double startedIdleAnimation = idleAnimationStart.Get(ped);
                            if (now > startedIdleAnimation + 10)
                            {
                                animator.SetBool(IS_IDLE_ACTIVE, false);

                                idleAnimationStart.Remove(ped);
                            }
                            //*/
                        }
                    }
                }
                else // Vkl
                {
                    position = bit.Centre().ToVector3(); // + halfHeight;
                    rotation = Quaternion.LookRotation(bit.ForwardGrad().ToVector3());
                }

                
                ubot.transform.position = position;
                ubot.transform.rotation = rotation;
              
            }
        }

        private void CameraFollowsPed(IPed ped, Vector3 position)
        {
            // Gather position, bearing and speed for use by camera
            Angle bearing = ped.Forward().AsBearing();
            IVkl vkl = ped.GetVkl();
            double speed = vkl != null ? vkl.Speed():ped.Speed();
           
            UaCamControllerMain.MostRecentInstance.PlayerPosition(position, bearing, speed, ped.IsAboard());

        }
        
        protected virtual Quaternion RotatePlayer(IPed ped, Quaternion rotation)  { return rotation; }
       
        public void Depart(IBot bot)
        {
            lock (this)
            {
                justDeparted.Add(bot);
            }
        }

        public void Arrive(IBot bot, IZone z)
        {
            lock (this)
            {
                if (bot is ITaxi && z is ITaxiZone) return;
                
                justArrived.Add(bot);

                if (bot is IPed && App().IsPlayer((IPed)bot)) UaCamControllerMain.MostRecentInstance.PlayerArrived();
            }
        }

        protected virtual Transform parentTransform(IPed ped)
        {
            return uPedsTop.transform;
        }
        
        protected virtual GameObject CreateNewUBod(IBit bit, GameObject prefab, string bitName)
        {
            Transform parent;
            switch (bit)
            {
                case IPed ped: parent = parentTransform(ped); break;
                case ITaxi _:   parent = uTaxisTop.transform;  break;
                case IBus _:   parent = uBusesTop.transform; break;
                case IDrv drv:
                {
                    if (drv.IsCoach()) parent = uCoachesTop.transform;
                    else if (drv.IsTruck()) parent = uTrucksTop.transform;
                    else parent = uCarsTop.transform;
                    break;
                }
                default: parent = uTrucksTop.transform; break; // trailers will be here
            }

            Vector3 startPosition = bit.Centre().ToVector3();
            Quaternion rotation = Quaternion.LookRotation(bit.Forward().ToVector3());
            GameObject uBit = Instantiate(prefab, startPosition, rotation, parent);

            SetLayerRecursive(uBit, UaMapMesh.LAYER_SIM_MAP);
            uBit.name = bitName;
            
            
            if (bit is IPed)
            {
                float pedHeight;
                CharacterController cc = uBit.GetComponent<CharacterController>();
                if (cc != null) pedHeight = cc.height;
                else pedHeight = BodBounds(prefab).y;
                
                float scale = (float) bit.Height() / pedHeight;
                
                Vector3 scaleVec = new Vector3(scale, scale, scale);
                uBit.transform.localScale = scaleVec;
            }

            activeBotToUBot.Add(bit, uBit);
            activeUBotToBot.Add(uBit, bit);

            return uBit;
        }

        // The ballast passenger GO will be created as a child of the bus GO (transform), so its relative position
        // needs to be set only once, and it will then move with the bus.
        private void CreateNewBallastPassenger(IBus bus, int seatNo, GameObject prefab, GameObject uBus)
        {
            Xyz offsetAbs = bus.PositionInBus(seatNo);
            Vector3 startPositionAbs = bus.Centre().Plus(offsetAbs).ToVector3();
            startPositionAbs += PED_LIFT_ABOARD_BUS;
            Quaternion rotationAbs = Quaternion.Euler(0, (float) bus.Forward().AsBearing().Degrees(), 0);
            GameObject uBallastPed = Instantiate(prefab, startPositionAbs, rotationAbs, uBus.transform);
            
            Animator animator = uBallastPed.GetComponent<Animator>();
            if (animator != null) animator.SetBool(IS_WALKING, false);
        }

        protected virtual int RandomAvatarIndex(IPed ped)
        {
            return 0;
        }
        private static readonly Vector3 UNIT_SIZE = new Vector3(1, 1, 1);

        private Vector3 BodBounds(GameObject prefab)
        {
            Vector3 bounds = UNIT_SIZE;

            if (prefab.transform.childCount >= 1) //&& prefab.transform.GetChild(0).childCount >= 1)
            {
                Transform txChild0 = prefab.transform.GetChild(0);
                if (txChild0.childCount >= 1)
                {
                    GameObject grandChild0 = prefab.transform.GetChild(0).GetChild(0).gameObject;
                    Renderer rend = grandChild0.GetComponent<Renderer>();
                    if (rend != null) bounds = rend.bounds.size;
                }
                else
                {
                    GameObject child0 = txChild0.gameObject;
                    Renderer rend = child0.GetComponent<Renderer>();
                    if (rend != null) bounds = rend.bounds.size;
                }

            }

            return bounds;
        }

        
        private void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            for (int ci = 0; ci < go.transform.childCount; ci++)
            {
                GameObject goChild = go.transform.GetChild(ci).gameObject;
                SetLayerRecursive(goChild, layer);
            }
        }

        /// <summary> Calculate and return the size of the (vehicle) prefab, x = width, y = height, z = length </summary>
        /// <param name="prefab"> The vehicle prefab</param>
        /// <returns> The size [  x = width, y = height, z = length ] </returns>
        private Vector3 RotatedScaledBounds(GameObject prefab)
        {
            // The prefab may be made up of several meshes, in different materials
            Bounds combinedBounds = new Bounds (prefab.transform.position, Vector3.zero);
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer> ();
            foreach (Renderer renderer in renderers)
            {
                combinedBounds.Encapsulate (renderer.bounds);
            }

            // Vector3 size = combinedBounds.size;
            // Reporter.Report("Prefab %s W %.2f x H %.2f x L %.2f", prefab.name, size.x, size.y, size.z);

            return combinedBounds.size;
            
        }
        private void CreateDrvTypesRigid(GameObject[] prefabs, Purpose purpose)
        {
            foreach (GameObject prefab in prefabs)
            {
                Vector3 size = RotatedScaledBounds(prefab);
                
                IType kt = null;
                {
                    TypeSpecCar carSpec = prefab.GetComponent<TypeSpecCar>();
                    if (carSpec != null)
                    {
                        kt = VRoad.NewVehicleType(prefab.name, purpose, carSpec.abundance, carSpec.GetMotorType(),
                            size.z, size.x, size.y, carSpec.wheelBaseMetres, null, 0);
                    }
                }
                
                if (kt == null) // maybe it is a fixed-body truck
                {
                    TypeSpecTruck truckSpec = prefab.GetComponent<TypeSpecTruck>();
                    if (truckSpec != null)
                    {
                        kt = VRoad.NewVehicleType(prefab.name, purpose, truckSpec.abundance, truckSpec.GetMotorType(),
                            size.z, size.x, size.y, truckSpec.wheelBaseMetres, null, 0);
                    }
                }

                if (kt == null) // maybe it is a coach
                {
                    TypeSpecBus busSpec = prefab.GetComponent<TypeSpecBus>();
                    if (busSpec != null)
                    {
                        kt = VRoad.NewVehicleType(prefab.name, purpose, busSpec.abundance, busSpec.GetMotorType(),
                            size.z, size.x, size.y, busSpec.wheelBaseMetres, null, 0);
                    }
                }

                if (kt != null) typeToPrefab.Put(kt, prefab);
            }
        }
        private void CreateDrvTypesTractor(GameObject[] prefabs, Purpose purpose, IType[] trailers)
        {
            foreach (GameObject prefab in prefabs)
            {
                TypeSpecTruck spec = prefab.GetComponent<TypeSpecTruck>();
                if (spec == null) continue;
                Vector3 size = RotatedScaledBounds(prefab);
                
                foreach (IType trailer in trailers)
                {
                    IType kt = VRoad.NewVehicleType(prefab.name, purpose, spec.abundance, spec.GetMotorType(),
                        size.z, size.x, size.y, spec.wheelBaseMetres, trailer, spec.trailerOffset);
                    
                    typeToPrefab.Put(kt, prefab);
                }
            }
        }
        private IType[] CreateDrvTypesTrailer(GameObject[] prefabs, Purpose purpose)
        {
            int np = prefabs.Length;
            IType[] trailers = new IType[np];

            for (int pi = 0; pi < np; pi++)
            {
                GameObject prefab = prefabs[pi];
                
                TypeSpecTrailer spec = prefab.GetComponent<TypeSpecTrailer>();
                if (spec == null) continue;

                Vector3 size = RotatedScaledBounds(prefab);
                
                trailers[pi] = VRoad.NewTrailerType(prefab.name,  size.z, size.x, size.y, spec.trailerOffset);
                
                typeToPrefab.Put(trailers[pi], prefab);
            }

            return trailers;
        }

        private void CreateBusTypes(GameObject[] prefabs)
        {
            foreach (GameObject prefab in prefabs)
            {
                TypeSpecBus spec = prefab.GetComponent<TypeSpecBus>();
                if (spec == null) continue;

                Vector3 size = RotatedScaledBounds(prefab);
                
                IType bt = VRoad.NewBusType(prefab.name, spec.abundance, spec.GetMotorType(), 
                    size.z, size.x, size.y, spec.wheelBaseMetres);

                typeToPrefab.Put(bt, prefab);
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////
        
        
        
        
        
        
        
        // This 'brake' object controls the master simulation thread, holding it back
        // once a simulation step has been completed until the UBots (game objects)
        // have been created, moved or destroyed

        protected class SimBrake : LSimTimeStep
        {
            private readonly object simMaster;
            private bool stepResultsWaiting;

            public SimBrake(App app)
            {
                app.AddEventConsumer(this);

                simMaster = app.Sim().MasterThread();
            }

            public bool DeregisterFireMapChange() { return true; }

            public void TimeStep()
            {
                stepResultsWaiting = true;

                lock (simMaster)
                {
                    KMonitor.Wait(simMaster);
                }
            }

            public bool StepResultsWaiting()
            {
                return stepResultsWaiting;
            }

            public void RenderingCompleted()
            {
                stepResultsWaiting = false;

                lock (simMaster)
                {
                    KMonitor.Pulse(simMaster);
                }

            }
        }
        
    }

}