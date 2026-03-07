using System.Linq;
using Configs.Impl;
using Data.Enums;
using Services.Impl;
using UnityEditor;

namespace Tools
{
    public static class SaveClearTools
    {
        private const string MenuRoot = "Tools/Saves/";

        [MenuItem(MenuRoot + "Clear All Saves")]
        private static void ClearAllSaves()
        {
            if (!Confirm("Clear ALL saves (selection, training, volume)?"))
                return;

            var dataService = new DataService();
            dataService.DeleteGameSelection();
            dataService.DeleteTrainingTimeScores();
            dataService.DeleteAudioVolumes();

            ResetSelectionAssets();
            ResetTrainingAssets();
            ResetAudioSelectionAssets();
            ResetAudioVolumeAssets();
        }

        [MenuItem(MenuRoot + "Clear Selection Save")]
        private static void ClearSelectionSave()
        {
            if (!Confirm("Clear selection save (car/map/mode/opponent/music)?"))
                return;

            var dataService = new DataService();
            dataService.DeleteGameSelection();

            ResetSelectionAssets();
        }

        [MenuItem(MenuRoot + "Clear Selection/Car")]
        private static void ClearCarSave()
        {
            if (!Confirm("Clear saved car selection?"))
                return;

            var dataService = new DataService();
            dataService.ClearSavedCarSelection();
            ResetCarSelectionAssets();
        }

        [MenuItem(MenuRoot + "Clear Selection/Map")]
        private static void ClearMapSave()
        {
            if (!Confirm("Clear saved map selection?"))
                return;

            var dataService = new DataService();
            dataService.ClearSavedMapSelection();
            ResetMapSelectionAssets();
        }

        [MenuItem(MenuRoot + "Clear Selection/Mode")]
        private static void ClearModeSave()
        {
            if (!Confirm("Clear saved game mode selection?"))
                return;

            var dataService = new DataService();
            dataService.ClearSavedGameModeSelection();
            ResetGameModeSelectionAssets();
        }

        [MenuItem(MenuRoot + "Clear Selection/Opponent")]
        private static void ClearOpponentSave()
        {
            if (!Confirm("Clear saved opponent selection?"))
                return;

            var dataService = new DataService();
            dataService.ClearSavedOpponentSelection();
            ResetOpponentSelectionAssets();
        }

        [MenuItem(MenuRoot + "Clear Selection/Music")]
        private static void ClearMusicSave()
        {
            if (!Confirm("Clear saved music selection?"))
                return;

            var dataService = new DataService();
            dataService.ClearSavedMusicSelection();
            ResetAudioSelectionAssets();
        }

        [MenuItem(MenuRoot + "Clear Training Saves")]
        private static void ClearTrainingSaves()
        {
            if (!Confirm("Clear training time saves?"))
                return;

            var dataService = new DataService();
            dataService.DeleteTrainingTimeScores();

            ResetTrainingAssets();
        }

        [MenuItem(MenuRoot + "Clear Volume Saves")]
        private static void ClearVolumeSaves()
        {
            if (!Confirm("Clear saved volume settings?"))
                return;

            var dataService = new DataService();
            dataService.DeleteAudioVolumes();
            ResetAudioVolumeAssets();
        }

        private static bool Confirm(string message)
        {
            return EditorUtility.DisplayDialog(
                "Clear Saves",
                message,
                "Clear",
                "Cancel");
        }

        private static void ResetSelectionAssets()
        {
            ResetCarSelectionAssets();
            ResetMapSelectionAssets();
            ResetGameModeSelectionAssets();
            ResetOpponentSelectionAssets();
            ResetAudioSelectionAssets();
        }

        private static void ResetCarSelectionAssets()
        {
            foreach (var asset in FindAssets<CarSelectionParameters>())
            {
                asset.SetSelectedCar(null, null, -1);
                EditorUtility.SetDirty(asset);
            }
        }

        private static void ResetMapSelectionAssets()
        {
            foreach (var asset in FindAssets<MapSelectionParameters>())
            {
                asset.SetSelectedMap(null, -1, EMap.None, 0, 0);
                EditorUtility.SetDirty(asset);
            }
        }

        private static void ResetGameModeSelectionAssets()
        {
            foreach (var asset in FindAssets<GameModeSelectionParameters>())
            {
                asset.SetSelectedGameMode(EGameMod.None);
                EditorUtility.SetDirty(asset);
            }
        }

        private static void ResetOpponentSelectionAssets()
        {
            foreach (var asset in FindAssets<OpponentSelectionParameters>())
            {
                asset.SetSelectedOpponent(string.Empty, 0f, -1);
                EditorUtility.SetDirty(asset);
            }
        }

        private static void ResetAudioSelectionAssets()
        {
            foreach (var asset in FindAssets<AudioSelectionParameters>())
            {
                asset.SetSelectedMusic(EAudioSubType.None, -1);
                EditorUtility.SetDirty(asset);
            }
        }

        private static void ResetAudioVolumeAssets()
        {
            foreach (var asset in FindAssets<AudioSelectionParameters>())
            {
                asset.SetMasterVolume(0.7f);
                asset.SetMusicVolume(0.7f);
                asset.SetSfxVolume(0.7f);
                asset.SetUiVolume(0.7f);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
        }

        private static void ResetTrainingAssets()
        {
            foreach (var asset in FindAssets<TrainingTimeScoreParameters>())
            {
                asset.ClearTimeValues();
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
        }

        private static T[] FindAssets<T>() where T : UnityEngine.Object
        {
            var assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = assetGuids
                .Distinct()
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<T>(path))
                .Where(asset => asset != null)
                .ToArray();

            return assets;
        }
    }
}