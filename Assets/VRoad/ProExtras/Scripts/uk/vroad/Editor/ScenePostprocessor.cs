using System.Collections.Generic;
using uk.vroad.apk;
using uk.vroad.ucm;
using uk.vroad.urvr;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace uk.vroad.Editor
{
    public static class ScenePostprocessor
    {
        private static bool checkedProScenesAreInBuild = false;

        [PostProcessSceneAttribute (1)]
        public static void OnPostprocessScene()
        {
            if (!checkedProScenesAreInBuild) CheckScenesAreInBuild();
        }
        
        private static void CheckScenesAreInBuild()
        {
            checkedProScenesAreInBuild = true;
            
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            editorBuildSettingsScenes.AddRange(EditorBuildSettings.scenes);

            string sceneDirPath = MeshTools.VRoadRoot() + KC.SLASH + KC.ENV_PRO_EXTRAS_DIR + EDSF.SCENE_PATH_REL;
            string[] requiredScenes = {SG.INITIAL_SCENE, SG.NAVIGATION_SCENE,};
            bool addedAny = false;
            
            foreach (string requiredSceneName in requiredScenes)
            {
                bool found = false;

                foreach (EditorBuildSettingsScene ebss in editorBuildSettingsScenes)
                {
                    string sceneName = (new KFile(ebss.path)).FilenameWithoutExtension();
                    if (requiredSceneName.Equals(sceneName)) { found = true; break; }
                }

                if (!found)
                {
                    string scenePath = sceneDirPath + requiredSceneName + EDSF.SCENE_SUFFIX;
                    if ((new KFile(scenePath)).Exists())
                    {
                        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                        addedAny = true;
                        
                        Debug.Log("[V-Road] Added scene to build: "+scenePath);
                    }
                }
            }

            if (addedAny) EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
    }
}