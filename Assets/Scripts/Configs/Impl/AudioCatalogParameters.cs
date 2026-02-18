using System.Collections.Generic;
using Data.Struct;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(AudioCatalogParameters), fileName = nameof(AudioCatalogParameters))]
    public class AudioCatalogParameters : ScriptableObject
    {
        [SerializeField] private List<AudioSettingsSetup> _setups = new();

        public List<AudioSettingsSetup> AudioSetups => _setups;
    }
}