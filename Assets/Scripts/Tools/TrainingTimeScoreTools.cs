using System.Linq;
using Configs.Impl;
using UnityEditor;

namespace Tools
{
    public static class TrainingTimeScoreTools
    {
        [MenuItem("Tools/Clear Training Saves")]
        private static void ClearTrainingSaves()
        {
            var shouldClear = EditorUtility.DisplayDialog(
                "Clear Training Saves",
                "Clear best total time and segment times for all TrainingTimeScoreParameters assets?",
                "Clear",
                "Cancel");

            if (!shouldClear)
                return;

            var assetGuids = AssetDatabase.FindAssets("t:TrainingTimeScoreParameters");
            foreach (var guid in assetGuids.Distinct())
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<TrainingTimeScoreParameters>(assetPath);
                if (asset == null)
                    continue;

                asset.ClearTimeValues();
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
