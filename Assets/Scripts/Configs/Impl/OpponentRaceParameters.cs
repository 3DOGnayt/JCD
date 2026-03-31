using System.Collections.Generic;
using Data.Struct;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(OpponentRaceParameters), fileName = nameof(OpponentRaceParameters), order = 5)]
    public class OpponentRaceParameters : ScriptableObject
    {
        [SerializeField] private List<OpponentBehaviorEntry> _behaviors = new();

        public IReadOnlyList<OpponentBehaviorEntry> Behaviors => _behaviors;
    }
}
