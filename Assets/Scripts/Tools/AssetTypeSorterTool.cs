#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class AssetTypeSorterTool : EditorWindow
    {
        private const string MenuPath = "Tools/JCD/Asset Type Sorter";
        private const string DefaultOutputFolderName = "_SortedByType";

        private static readonly Dictionary<string, string> FolderByExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".prefab", "Prefabs" },
            { ".unity", "Scenes" },
            { ".mat", "Materials" },
            { ".physicmaterial", "Materials" },
            { ".physicsmaterial2d", "Materials" },
            { ".shader", "Shaders" },
            { ".shadergraph", "Shaders" },
            { ".compute", "Shaders" },
            { ".cs", "Scripts" },
            { ".asmdef", "Scripts" },
            { ".asmref", "Scripts" },
            { ".asset", "ScriptableObjects" },
            { ".controller", "Animations" },
            { ".overridecontroller", "Animations" },
            { ".anim", "Animations" },
            { ".mask", "Animations" },
            { ".png", "Textures" },
            { ".jpg", "Textures" },
            { ".jpeg", "Textures" },
            { ".tga", "Textures" },
            { ".psd", "Textures" },
            { ".tif", "Textures" },
            { ".tiff", "Textures" },
            { ".bmp", "Textures" },
            { ".gif", "Textures" },
            { ".exr", "Textures" },
            { ".hdr", "Textures" },
            { ".fbx", "Models" },
            { ".obj", "Models" },
            { ".blend", "Models" },
            { ".dae", "Models" },
            { ".3ds", "Models" },
            { ".wav", "Audio" },
            { ".mp3", "Audio" },
            { ".ogg", "Audio" },
            { ".aiff", "Audio" },
            { ".mixer", "Audio" },
            { ".ttf", "Fonts" },
            { ".otf", "Fonts" },
            { ".fontsettings", "Fonts" },
            { ".uxml", "UI" },
            { ".uss", "UI" },
            { ".json", "Data" },
            { ".xml", "Data" },
            { ".csv", "Data" },
            { ".txt", "Text" },
            { ".md", "Text" }
        };

        private static readonly HashSet<string> IgnoredGroupTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a",
            "albedo",
            "alpha",
            "alphas",
            "ao",
            "b",
            "basecolor",
            "bc",
            "bump",
            "baked",
            "color",
            "d",
            "diff",
            "diffuse",
            "er",
            "height",
            "m",
            "mask",
            "masks",
            "metallic",
            "n",
            "nm",
            "normal",
            "normalmap",
            "normalmaps",
            "normals",
            "ogl",
            "opacity",
            "pf",
            "ph",
            "roughness",
            "single",
            "sm",
            "smoothness",
            "spec",
            "specular",
            "unity",
            "x"
        };

        private static readonly HashSet<string> IgnoredSingleGroupTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "asset",
            "baked",
            "material",
            "mat",
            "mesh",
            "pf",
            "ph",
            "prefab",
            "single",
            "sm",
            "texture",
            "tex"
        };

        private DefaultAsset _sourceFolder;
        private string _outputFolderName = DefaultOutputFolderName;
        private bool _includeSubfolders = true;
        private bool _groupRepeatedNames = true;
        private bool _dryRun = true;
        private Vector2 _scroll;
        private List<SortItem> _previewItems = new List<SortItem>();
        private string _lastMessage = string.Empty;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<AssetTypeSorterTool>("Asset Type Sorter");
        }

        [MenuItem(MenuPath + " From Selection")]
        private static void SortFromSelection()
        {
            var window = GetWindow<AssetTypeSorterTool>("Asset Type Sorter");
            window.UseSelectedFolder();
            window.BuildPreview();
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("JCD Asset Type Sorter", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Выбери папку в Project window. Тулза создаст внутри нее новую папку и разложит файлы по типам. " +
                "Серии от двух файлов с похожими токенами имени уйдут в подпапку внутри своего типа.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                _sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Source folder", _sourceFolder, typeof(DefaultAsset), false);

                if (GUILayout.Button("Use Selection", GUILayout.Width(110)))
                {
                    UseSelectedFolder();
                }
            }

            _outputFolderName = EditorGUILayout.TextField("Output folder", _outputFolderName);
            _includeSubfolders = EditorGUILayout.Toggle("Include subfolders", _includeSubfolders);
            _groupRepeatedNames = EditorGUILayout.Toggle(
                new GUIContent("Group repeated names", "Создавать подпапки для серий от двух файлов с общим именем."),
                _groupRepeatedNames);
            _dryRun = EditorGUILayout.Toggle("Dry run", _dryRun);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview", GUILayout.Height(28)))
                {
                    BuildPreview();
                }

                using (new EditorGUI.DisabledScope(_dryRun))
                {
                    if (GUILayout.Button("Create Folders And Move", GUILayout.Height(28)))
                    {
                        SortAssets();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_lastMessage))
            {
                EditorGUILayout.HelpBox(_lastMessage, MessageType.None);
            }

            DrawPreview();
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"Preview ({_previewItems.Count})", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (var item in _previewItems)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(item.SourcePath);
                    EditorGUILayout.LabelField(item.TargetPath);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void UseSelectedFolder()
        {
            var selectedFolder = Selection.objects
                .OfType<DefaultAsset>()
                .FirstOrDefault(asset =>
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    return AssetDatabase.IsValidFolder(path) && IsAssetsPath(path);
                });

            if (selectedFolder == null)
            {
                _lastMessage = "Выдели папку внутри Assets/ в Project window.";
                return;
            }

            _sourceFolder = selectedFolder;
            _lastMessage = string.Empty;
        }

        private void BuildPreview()
        {
            _previewItems = CreateSortItems();
            _lastMessage = _previewItems.Count == 0
                ? "Файлы для сортировки не найдены."
                : $"Найдено файлов для сортировки: {_previewItems.Count}.";
        }

        private void SortAssets()
        {
            var items = CreateSortItems();
            if (items.Count == 0)
            {
                _previewItems = items;
                _lastMessage = "Файлы для сортировки не найдены.";
                return;
            }

            var sourcePath = GetSourcePath();
            var outputPath = EnsureOutputFolder(sourcePath);
            var movedCount = 0;
            var errors = new List<string>();

            foreach (var targetFolderPath in items.Select(item => item.TargetFolderPath).Distinct())
            {
                EnsureAssetFolderPath(outputPath, targetFolderPath);
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var item in items)
                {
                    var targetPath = AssetDatabase.GenerateUniqueAssetPath(item.TargetPath);
                    var error = AssetDatabase.MoveAsset(item.SourcePath, targetPath);
                    if (string.IsNullOrEmpty(error))
                    {
                        movedCount++;
                    }
                    else
                    {
                        errors.Add($"{item.SourcePath}: {error}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            _previewItems = CreateSortItems();
            _lastMessage = errors.Count == 0
                ? $"Готово. Перемещено файлов: {movedCount}."
                : $"Перемещено файлов: {movedCount}. Ошибок: {errors.Count}. Первая ошибка: {errors[0]}";
        }

        private List<SortItem> CreateSortItems()
        {
            var sourcePath = GetSourcePath();
            if (string.IsNullOrEmpty(sourcePath))
            {
                return new List<SortItem>();
            }

            var sourceFullPath = Path.GetFullPath(sourcePath);
            var outputPath = Path.Combine(sourcePath, GetSafeOutputFolderName()).Replace("\\", "/");
            var outputFullPath = Path.GetFullPath(outputPath);
            var searchOption = _includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var assetPaths = Directory.GetFiles(sourceFullPath, "*", searchOption)
                .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .Where(path => !IsInsidePath(path, outputFullPath))
                .Select(path => ToAssetPath(path))
                .Where(path => !string.IsNullOrEmpty(path))
                .Where(path => !AssetDatabase.IsValidFolder(path))
                .ToList();

            var groupByPath = _groupRepeatedNames
                ? BuildGroupByPath(assetPaths)
                : new Dictionary<string, string>();

            return assetPaths
                .Select(path => CreateSortItem(path, outputPath, groupByPath))
                .OrderBy(item => item.TypeFolder)
                .ThenBy(item => item.TargetFolderPath)
                .ThenBy(item => item.SourcePath)
                .ToList();
        }

        private SortItem CreateSortItem(string sourcePath, string outputPath, IReadOnlyDictionary<string, string> groupByPath)
        {
            var extension = Path.GetExtension(sourcePath);
            var typeFolder = FolderByExtension.TryGetValue(extension, out var folder)
                ? folder
                : "Other";
            var targetFolderPath = $"{outputPath}/{typeFolder}";

            if (groupByPath.TryGetValue(sourcePath, out var groupFolder))
            {
                targetFolderPath = $"{targetFolderPath}/{groupFolder}";
            }

            var targetPath = $"{targetFolderPath}/{Path.GetFileName(sourcePath)}";

            return new SortItem(sourcePath, targetPath, targetFolderPath, typeFolder);
        }

        private string GetSourcePath()
        {
            if (_sourceFolder == null)
            {
                return string.Empty;
            }

            var path = AssetDatabase.GetAssetPath(_sourceFolder);
            return AssetDatabase.IsValidFolder(path) && IsAssetsPath(path)
                ? path
                : string.Empty;
        }

        private string EnsureOutputFolder(string sourcePath)
        {
            var folderName = GetSafeOutputFolderName();
            var outputPath = $"{sourcePath}/{folderName}";

            if (!AssetDatabase.IsValidFolder(outputPath))
            {
                AssetDatabase.CreateFolder(sourcePath, folderName);
            }

            return outputPath;
        }

        private static void EnsureAssetFolder(string parentPath, string folderName)
        {
            var path = $"{parentPath}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static void EnsureAssetFolderPath(string rootPath, string targetFolderPath)
        {
            if (!targetFolderPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relativePath = targetFolderPath.Substring(rootPath.Length).Trim('/');
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var currentPath = rootPath;
            foreach (var folderName in relativePath.Split('/'))
            {
                EnsureAssetFolder(currentPath, folderName);
                currentPath = $"{currentPath}/{folderName}";
            }
        }

        private static Dictionary<string, string> BuildGroupByPath(IReadOnlyList<string> assetPaths)
        {
            var candidates = assetPaths
                .SelectMany(CreateGroupCandidates)
                .GroupBy(candidate => $"{candidate.TypeFolder}|{candidate.Key}", StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Select(candidate => candidate.Path).Distinct().Count() >= 2)
                .Select(group =>
                {
                    var firstCandidate = group.First();
                    return new GroupCandidateSet(
                        firstCandidate.FolderName,
                        firstCandidate.Priority,
                        firstCandidate.StartIndex,
                        group.Select(candidate => candidate.Path).Distinct().ToList());
                })
                .OrderBy(group => group.StartIndex)
                .ThenByDescending(group => group.Priority)
                .ThenByDescending(group => group.Paths.Count)
                .ThenBy(group => group.FolderName)
                .ToList();

            var result = new Dictionary<string, string>();
            foreach (var candidateSet in candidates)
            {
                var unassignedPaths = candidateSet.Paths
                    .Where(path => !result.ContainsKey(path))
                    .ToList();

                if (unassignedPaths.Count < 2)
                {
                    continue;
                }

                foreach (var path in unassignedPaths)
                {
                    result.Add(path, candidateSet.FolderName);
                }
            }

            return result;
        }

        private static IEnumerable<GroupCandidate> CreateGroupCandidates(string path)
        {
            var extension = Path.GetExtension(path);
            var typeFolder = FolderByExtension.TryGetValue(extension, out var folder)
                ? folder
                : "Other";
            var tokens = NormalizeAssetTokens(Path.GetFileNameWithoutExtension(path));
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var length = tokens.Length; length >= 1; length--)
            {
                for (var startIndex = 0; startIndex <= tokens.Length - length; startIndex++)
                {
                    var groupTokens = tokens.Skip(startIndex).Take(length).ToArray();
                    if (length == 1 && IsBadSingleGroupToken(groupTokens[0]))
                    {
                        continue;
                    }

                    var key = string.Join(" ", groupTokens).ToLowerInvariant();
                    if (!seenKeys.Add(key))
                    {
                        continue;
                    }

                    var folderName = CreateFolderName(groupTokens);
                    if (string.IsNullOrEmpty(folderName))
                    {
                        continue;
                    }

                    yield return new GroupCandidate(path, typeFolder, key, folderName, length, startIndex);
                }
            }
        }

        private static bool IsBadSingleGroupToken(string token)
        {
            return token.Length <= 1 || IgnoredSingleGroupTokens.Contains(token);
        }

        private static string CreateFolderName(IReadOnlyList<string> tokens)
        {
            var folderName = string.Join(" ", tokens.Select(UppercaseFirstLetter));
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), string.Empty);
            }

            return folderName.Trim();
        }

        private static string UppercaseFirstLetter(string value)
        {
            return string.IsNullOrEmpty(value)
                ? value
                : char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string[] NormalizeAssetTokens(string name)
        {
            var characters = new List<char>(name.Length);
            var previousWasSeparator = false;

            foreach (var character in name)
            {
                if (char.IsDigit(character))
                {
                    continue;
                }

                if (char.IsLetter(character))
                {
                    characters.Add(character);
                    previousWasSeparator = false;
                    continue;
                }

                if (!previousWasSeparator)
                {
                    characters.Add(' ');
                    previousWasSeparator = true;
                }
            }

            return new string(characters.ToArray())
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(token => !IgnoredGroupTokens.Contains(token))
                .ToArray();
        }

        private string GetSafeOutputFolderName()
        {
            var folderName = string.IsNullOrWhiteSpace(_outputFolderName)
                ? DefaultOutputFolderName
                : _outputFolderName.Trim();

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), string.Empty);
            }

            return string.IsNullOrWhiteSpace(folderName)
                ? DefaultOutputFolderName
                : folderName;
        }

        private static bool IsInsidePath(string path, string parentPath)
        {
            var fullPath = Path.GetFullPath(path);
            var fullParentPath = Path.GetFullPath(parentPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return fullPath.StartsWith(fullParentPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || fullPath.StartsWith(fullParentPath + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAssetsPath(string path)
        {
            return string.Equals(path, "Assets", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        private static string ToAssetPath(string fullPath)
        {
            var projectPath = Path.GetFullPath(Application.dataPath + "/..").Replace("\\", "/");
            var normalizedPath = Path.GetFullPath(fullPath).Replace("\\", "/");

            if (!normalizedPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return normalizedPath.Substring(projectPath.Length + 1);
        }

        private readonly struct SortItem
        {
            public readonly string SourcePath;
            public readonly string TargetPath;
            public readonly string TargetFolderPath;
            public readonly string TypeFolder;

            public SortItem(string sourcePath, string targetPath, string targetFolderPath, string typeFolder)
            {
                SourcePath = sourcePath;
                TargetPath = targetPath;
                TargetFolderPath = targetFolderPath;
                TypeFolder = typeFolder;
            }
        }

        private readonly struct GroupCandidate
        {
            public readonly string Path;
            public readonly string TypeFolder;
            public readonly string Key;
            public readonly string FolderName;
            public readonly int Priority;
            public readonly int StartIndex;

            public GroupCandidate(string path, string typeFolder, string key, string folderName, int priority, int startIndex)
            {
                Path = path;
                TypeFolder = typeFolder;
                Key = key;
                FolderName = folderName;
                Priority = priority;
                StartIndex = startIndex;
            }
        }

        private readonly struct GroupCandidateSet
        {
            public readonly string FolderName;
            public readonly int Priority;
            public readonly int StartIndex;
            public readonly IReadOnlyList<string> Paths;

            public GroupCandidateSet(string folderName, int priority, int startIndex, IReadOnlyList<string> paths)
            {
                FolderName = folderName;
                Priority = priority;
                StartIndex = startIndex;
                Paths = paths;
            }
        }
    }
}
#endif
