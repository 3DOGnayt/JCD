using Components;
using Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _parametersText;
        [SerializeField] private TMP_Text _parametersValue;

        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        private void OnEnable()
        {
            _signalBus.Subscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
        }

        private void Awake()
        {
            _parametersText.text = "Km/h\n" + 
                                   "Back Speed\n" +
                                   "Gearbox\n" + 
                                   "Engine Rpm\n" +
                                   "\n" + 
                                   "BrakeInput\n" + 
                                   "HandbrakeInput\n" + 
                                   "Drift Multiplier" ;
        }

        private void OnCarSetupAspectChanged(ComponentChangeSignal<CarSetupAspect> signal)
        {
            _parametersValue.text = $"{signal.Component.Speed.Value:0} :\n" +
                                    $"{signal.Component.BackSpeed.Value:0} :\n" +
                                    $"{signal.Component.Gear.Value:0} :\n" +
                                    $"{signal.Component.EngineRpm.Value:0} :\n" +
                                    "\n" +
                                    $"{signal.Component.BrakeInput.Value:0} :\n" +
                                    $"{signal.Component.HandbrakeInput.Value:0} :\n" +
                                    $"{signal.Component.DriftMultiplier.Value:0} :";
        }
    }
}