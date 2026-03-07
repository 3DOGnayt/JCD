using System;
using Data.Enums;

namespace Data.Struct
{
    [Serializable]
    public class GameSelectionSaveData
    {
        public bool HasData;
        public int CarIndex = -1;
        public int MapIndex = -1;
        public EMap Map = EMap.None;
        public EGameMod GameMode = EGameMod.None;
        public int OpponentIndex = -1;
        public EAudioSubType MusicSubType = EAudioSubType.None;
        public int MusicIndex = -1;
    }
}