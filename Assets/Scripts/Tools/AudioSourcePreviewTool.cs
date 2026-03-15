using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class AudioSourcePreviewTool : EditorWindow
    {
        private const string PreviewObjectName = "AudioSource Preview (Editor)";

        [SerializeField]
        private AudioSource _audioSource;

        private Editor _audioSourceEditor;
        private Vector2 _scrollPosition;
        private double _lastRepaintTime;

        [MenuItem("Tools/Audio Source Preview")]
        public static void ShowWindow()
        {
            GetWindow<AudioSourcePreviewTool>("Audio Source Preview");
        }

        private void OnEnable()
        {
            EnsureAudioSource();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            CleanupEditor();
            CleanupAudioSource();
        }

        private void OnGUI()
        {
            EnsureAudioSource();

            EditorGUILayout.LabelField("Audio Source Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Drop a clip into the AudioSource below, adjust its settings, and press Play.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Play"))
                {
                    Play();
                }

                if (GUILayout.Button("Pause"))
                {
                    Pause();
                }

                if (GUILayout.Button("Stop"))
                {
                    Stop();
                }

                if (GUILayout.Button("Restart"))
                {
                    Restart();
                }

                if (GUILayout.Button("Reset Settings"))
                {
                    ResetSettings();
                }
            }

            EditorGUILayout.Space();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawAudioSourceInspector();
            EditorGUILayout.EndScrollView();
        }

        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup - _lastRepaintTime < 0.2f)
            {
                return;
            }

            _lastRepaintTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void DrawAudioSourceInspector()
        {
            if (_audioSource == null)
            {
                EditorGUILayout.HelpBox("AudioSource is missing. Reopen the window to recreate it.", MessageType.Warning);
                return;
            }

            if (_audioSourceEditor == null || _audioSourceEditor.target != _audioSource)
            {
                CleanupEditor();
                _audioSourceEditor = Editor.CreateEditor(_audioSource);
            }

            _audioSourceEditor.OnInspectorGUI();
        }

        private void EnsureAudioSource()
        {
            if (_audioSource != null)
            {
                return;
            }

            foreach (var source in Resources.FindObjectsOfTypeAll<AudioSource>())
            {
                if (source != null && source.gameObject.name == PreviewObjectName)
                {
                    _audioSource = source;
                    break;
                }
            }

            if (_audioSource == null)
            {
                var go = new GameObject(PreviewObjectName);
                go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                _audioSource = go.AddComponent<AudioSource>();
            }
        }

        private void Play()
        {
            if (_audioSource == null || _audioSource.clip == null)
            {
                EditorUtility.DisplayDialog("Audio Source Preview", "Assign an AudioClip first.", "OK");
                return;
            }

            _audioSource.Play();
        }

        private void Pause()
        {
            if (_audioSource == null)
            {
                return;
            }

            _audioSource.Pause();
        }

        private void Stop()
        {
            if (_audioSource == null)
            {
                return;
            }

            _audioSource.Stop();
        }

        private void Restart()
        {
            if (_audioSource == null)
            {
                return;
            }

            _audioSource.Stop();
            _audioSource.Play();
        }

        private void CleanupEditor()
        {
            if (_audioSourceEditor == null)
            {
                return;
            }

            DestroyImmediate(_audioSourceEditor);
            _audioSourceEditor = null;
        }

        private void CleanupAudioSource()
        {
            if (_audioSource == null)
            {
                return;
            }

            DestroyImmediate(_audioSource.gameObject);
            _audioSource = null;
        }

        private void ResetSettings()
        {
            if (_audioSource == null)
            {
                return;
            }

            var clip = _audioSource.clip;
            var mixerGroup = _audioSource.outputAudioMixerGroup;

            var temp = new GameObject("AudioSource Defaults");
            temp.hideFlags = HideFlags.HideAndDontSave;
            var defaultSource = temp.AddComponent<AudioSource>();

            EditorUtility.CopySerialized(defaultSource, _audioSource);
            _audioSource.clip = clip;
            _audioSource.outputAudioMixerGroup = mixerGroup;

            DestroyImmediate(temp);
            EditorUtility.SetDirty(_audioSource);
        }
    }
}
