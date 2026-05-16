#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// JCD Asset Usage Scanner
///
/// Put this file here:
/// Assets/Editor/JCDAssetUsageScanner.cs
///
/// Open:
/// Tools/JCD/Asset Usage Scanner
///
/// Main ideas:
/// 1) Select one or more folders in Project window -> Add Selected Folders.
/// 2) Scan Candidate Folders -> checks every asset inside selected folders and shows USED / UNUSED.
/// 3) Scan Whole Project -> checks everything in Assets/ and shows what is referenced by something else.
/// 4) You can delete/move-to-trash only UNUSED assets from selected candidate folders.
///
/// Notes:
/// - Uses AssetDatabase.GetDependencies, so it finds normal Unity references:
///   scenes -> prefabs -> materials -> textures, terrain -> terrain layers -> textures, etc.
/// - It may not catch string-based loading like Resources.Load("path"), Addressables keys, AssetBundles, or custom file paths.
/// - Always use Git/backups before deleting.
/// </summary>
public class JCDAssetUsageScanner : EditorWindow
{
    private enum ScanMode
    {
        CandidateFolders,
        WholeProject
    }

    private enum FilterMode
    {
        AllAssetTypes,
        CommonArtAssetsOnly
    }

    [Serializable]
    private class UsageItem
    {
        public string path;
        public bool used;
        public bool selectedForDelete;
        public List<string> usedBy = new List<string>();
    }

    private readonly List<DefaultAsset> candidateFolders = new List<DefaultAsset>();
    private DefaultAsset scanRootFolder;

    private ScanMode scanMode = ScanMode.CandidateFolders;
    private FilterMode filterMode = FilterMode.AllAssetTypes;

    private bool scanWholeAssets = true;
    private bool includeScenesInSearch = true;
    private bool includePackagesAsSearchArea = false;

    private bool showUsed = true;
    private bool showUnused = true;
    private bool showOnlyDirectFolderAssets = false;
    private bool protectScriptsFromDelete = true;
    private bool protectMetaAndFolders = true;

    private Vector2 foldersScroll;
    private Vector2 resultsScroll;
    private string searchText = string.Empty;
    
    private Vector2 topScroll;

    private bool foldQuickHelp = true;
    private bool foldCandidateFolders = true;
    private bool foldSearchArea = true;
    private bool foldOptions = true;
    private bool foldActions = true;

    private readonly List<UsageItem> results = new List<UsageItem>();
    private readonly Dictionary<string, UsageItem> resultByPath = new Dictionary<string, UsageItem>();

    [MenuItem("Tools/JCD/Asset Usage Scanner")]
    public static void Open()
    {
        GetWindow<JCDAssetUsageScanner>("Asset Usage Scanner");
    }

    private void OnGUI()
    {
        float topHeight = Mathf.Clamp(position.height * 0.42f, 240f, 430f);

        topScroll = EditorGUILayout.BeginScrollView(topScroll, GUILayout.Height(topHeight));
        DrawHeader();
        DrawQuickHelp();
        DrawScanMode();
        DrawCandidateFolders();
        DrawSearchArea();
        DrawOptions();
        DrawActions();
        DrawSummary();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);
        DrawResults();
    }
    
    private void DrawQuickHelp()
    {
        EditorGUILayout.Space(6);

        foldQuickHelp = EditorGUILayout.BeginFoldoutHeaderGroup(foldQuickHelp, "Краткая инструкция");
        if (foldQuickHelp)
        {
            EditorGUILayout.HelpBox(
                "1. Выдели одну или несколько папок в Project window и нажми Add Selected Folders.\n" +
                "2. Нажми Scan.\n" +
                "3. USED = ассет где-то используется.\n" +
                "4. UNUSED = ассет нигде не найден по обычным Unity-ссылкам.\n" +
                "5. Move Selected UNUSED To Trash = отправить выбранные неиспользуемые ассеты в корзину Windows.\n\n" +
                "Важно: строковые загрузки типа Resources.Load / Addressables / AssetBundles могут не определяться.",
                MessageType.Info
            );
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("JCD Asset Usage Scanner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Выбери одну или несколько папок в Project window, добавь их в список и нажми Scan. " +
            "Тулза покажет, какие ассеты из этих папок реально где-то используются, а какие нет.",
            MessageType.Info
        );
    }

    private void DrawScanMode()
    {
        EditorGUILayout.Space(6);
        scanMode = (ScanMode)EditorGUILayout.EnumPopup("Scan mode", scanMode);

        if (scanMode == ScanMode.CandidateFolders)
        {
            EditorGUILayout.HelpBox(
                "Candidate Folders: проверяем только ассеты внутри выбранных папок. " +
                "Например Assets/EasyRoads3D — внутри будут проверены текстуры, материалы, модели, префабы и т.д.",
                MessageType.None
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Whole Project: проверяем весь Assets/ и показываем, что используется где-либо в проекте, а что нет. " +
                "Удалять из этого режима можно, но осторожно.",
                MessageType.Warning
            );
        }
    }

    private void DrawCandidateFolders()
    {
        EditorGUILayout.Space(6);

        foldCandidateFolders = EditorGUILayout.BeginFoldoutHeaderGroup(foldCandidateFolders, "Candidate folders");
        if (foldCandidateFolders)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Selected Folders", GUILayout.Height(26)))
                {
                    AddSelectedFolders();
                }

                if (GUILayout.Button("Clear", GUILayout.Height(26), GUILayout.Width(80)))
                {
                    candidateFolders.Clear();
                }
            }

            foldersScroll = EditorGUILayout.BeginScrollView(foldersScroll, GUILayout.MinHeight(50), GUILayout.MaxHeight(120));

            for (int i = 0; i < candidateFolders.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    candidateFolders[i] = (DefaultAsset)EditorGUILayout.ObjectField(candidateFolders[i], typeof(DefaultAsset), false);

                    if (GUILayout.Button("Ping", GUILayout.Width(45)))
                    {
                        EditorGUIUtility.PingObject(candidateFolders[i]);
                    }

                    if (GUILayout.Button("X", GUILayout.Width(24)))
                    {
                        candidateFolders.RemoveAt(i);
                        i--;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawSearchArea()
    {
        EditorGUILayout.Space(6);

        foldSearchArea = EditorGUILayout.BeginFoldoutHeaderGroup(foldSearchArea, "Search area");
        if (foldSearchArea)
        {
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190f;

            scanWholeAssets = EditorGUILayout.Toggle(
                new GUIContent("Scan whole Assets", "Искать использования по всему Assets/."),
                scanWholeAssets
            );

            using (new EditorGUI.DisabledScope(scanWholeAssets))
            {
                scanRootFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                    new GUIContent("Scan root folder", "Если Scan whole Assets выключен — искать только внутри этой папки."),
                    scanRootFolder,
                    typeof(DefaultAsset),
                    false
                );
            }

            includeScenesInSearch = EditorGUILayout.Toggle(
                new GUIContent("Include scenes in search", "Учитывать сцены (.unity) как источник ссылок."),
                includeScenesInSearch
            );

            includePackagesAsSearchArea = EditorGUILayout.Toggle(
                new GUIContent("Include Packages in search", "Искать ссылки также внутри Packages/."),
                includePackagesAsSearchArea
            );

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawOptions()
    {
        EditorGUILayout.Space(6);

        foldOptions = EditorGUILayout.BeginFoldoutHeaderGroup(foldOptions, "Options");
        if (foldOptions)
        {
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190f;

            filterMode = (FilterMode)EditorGUILayout.EnumPopup(
                new GUIContent("Candidate filter", "Какие типы файлов брать из выбранных папок в список проверки."),
                filterMode
            );

            showOnlyDirectFolderAssets = EditorGUILayout.Toggle(
                new GUIContent("Only direct folder files", "Проверять только файлы прямо в папке, без подпапок."),
                showOnlyDirectFolderAssets
            );

            protectScriptsFromDelete = EditorGUILayout.Toggle(
                new GUIContent("Protect .cs from delete", "Не давать удалять C#-скрипты."),
                protectScriptsFromDelete
            );

            protectMetaAndFolders = EditorGUILayout.Toggle(
                new GUIContent("Protect folders/meta", "Не давать удалять папки и .meta файлы."),
                protectMetaAndFolders
            );

            EditorGUIUtility.labelWidth = oldLabelWidth;

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                showUsed = EditorGUILayout.ToggleLeft("Show USED", showUsed, GUILayout.Width(100));
                showUnused = EditorGUILayout.ToggleLeft("Show UNUSED", showUnused, GUILayout.Width(120));

                GUILayout.Label("Search", GUILayout.Width(45));
                searchText = EditorGUILayout.TextField(searchText);
            }

            EditorGUILayout.Space(6);

            EditorGUILayout.HelpBox(
                "Опции кратко:\n" +
                "• Candidate filter — какие типы файлов брать на проверку.\n" +
                "• Only direct folder files — без подпапок.\n" +
                "• Protect .cs from delete — защита скриптов.\n" +
                "• Protect folders/meta — защита папок и .meta.\n" +
                "• Show USED / UNUSED — фильтр отображения результатов.",
                MessageType.None
            );
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawActions()
    {
        EditorGUILayout.Space(8);

        foldActions = EditorGUILayout.BeginFoldoutHeaderGroup(foldActions, "Actions");
        if (foldActions)
        {
            if (GUILayout.Button("Scan", GUILayout.Height(34)))
            {
                Scan();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(results.Count == 0))
                {
                    if (GUILayout.Button("Select All UNUSED", GUILayout.Height(26)))
                    {
                        foreach (UsageItem item in results)
                        {
                            item.selectedForDelete = !item.used && CanDelete(item.path);
                        }
                    }

                    if (GUILayout.Button("Deselect All", GUILayout.Height(26)))
                    {
                        foreach (UsageItem item in results)
                        {
                            item.selectedForDelete = false;
                        }
                    }
                }
            }

            using (new EditorGUI.DisabledScope(results.Count == 0))
            {
                if (GUILayout.Button("Move Selected UNUSED To Trash", GUILayout.Height(30)))
                {
                    MoveSelectedUnusedToTrash();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawSummary()
    {
        if (results.Count == 0)
            return;

        int used = results.Count(r => r.used);
        int unused = results.Count - used;
        int selected = results.Count(r => r.selectedForDelete);

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            $"Total checked: {results.Count}\nUSED: {used}\nUNUSED: {unused}\nSelected for delete: {selected}",
            MessageType.Info
        );
    }

    private void DrawResults()
    {
        if (results.Count == 0)
            return;

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

        resultsScroll = EditorGUILayout.BeginScrollView(resultsScroll);

        foreach (UsageItem item in results)
        {
            if (item.used && !showUsed)
                continue;

            if (!item.used && !showUnused)
                continue;

            if (!string.IsNullOrWhiteSpace(searchText) &&
                item.path.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) < 0 &&
                !item.usedBy.Any(p => p.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                continue;
            }

            DrawResultItem(item);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawResultItem(UsageItem item)
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(item.used || !CanDelete(item.path)))
                {
                    item.selectedForDelete = EditorGUILayout.Toggle(item.selectedForDelete, GUILayout.Width(20));
                }

                GUIStyle stateStyle = new GUIStyle(EditorStyles.boldLabel);
                stateStyle.normal.textColor = item.used ? new Color(0.1f, 0.6f, 0.1f) : new Color(0.85f, 0.15f, 0.1f);

                EditorGUILayout.LabelField(item.used ? "USED" : "UNUSED", stateStyle, GUILayout.Width(70));

                if (GUILayout.Button("Ping", GUILayout.Width(48)))
                    Ping(item.path);

                if (GUILayout.Button("Select", GUILayout.Width(56)))
                    SelectAsset(item.path);

                EditorGUILayout.SelectableLabel(item.path, GUILayout.Height(18));
            }

            if (item.used)
            {
                EditorGUILayout.LabelField("Used by:", EditorStyles.miniBoldLabel);

                int shown = 0;
                foreach (string usedByPath in item.usedBy)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(22);

                        if (GUILayout.Button("Ping", GUILayout.Width(48)))
                            Ping(usedByPath);

                        EditorGUILayout.SelectableLabel(usedByPath, GUILayout.Height(18));
                    }

                    shown++;
                    if (shown >= 12)
                    {
                        if (item.usedBy.Count > shown)
                            EditorGUILayout.LabelField($"...and {item.usedBy.Count - shown} more", EditorStyles.miniLabel);
                        break;
                    }
                }
            }
            else if (!CanDelete(item.path))
            {
                EditorGUILayout.HelpBox("Protected from delete by current options.", MessageType.None);
            }
        }
    }

    private void AddSelectedFolders()
    {
        UnityEngine.Object[] selectedObjects = Selection.objects;
        int added = 0;

        foreach (UnityEngine.Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
                continue;

            if (!AssetDatabase.IsValidFolder(path))
                continue;

            DefaultAsset folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

            if (folderAsset == null)
                continue;

            if (!candidateFolders.Contains(folderAsset))
            {
                candidateFolders.Add(folderAsset);
                added++;
            }
        }

        if (added == 0)
        {
            EditorUtility.DisplayDialog("No folders added", "Select one or more folders in Project window first.", "OK");
        }
    }

    private void Scan()
    {
        results.Clear();
        resultByPath.Clear();

        List<string> candidatePaths = CollectCandidatePaths();

        if (candidatePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing to scan", "No candidate assets found.", "OK");
            return;
        }

        foreach (string path in candidatePaths.OrderBy(p => p))
        {
            UsageItem item = new UsageItem
            {
                path = path,
                used = false,
                selectedForDelete = false
            };

            results.Add(item);
            resultByPath[path] = item;
        }

        HashSet<string> candidateSet = new HashSet<string>(candidatePaths);
        List<string> searchPaths = CollectSearchPaths();

        ScanDependencies(candidateSet, searchPaths);

        foreach (UsageItem item in results)
            item.usedBy.Sort(StringComparer.OrdinalIgnoreCase);

        Debug.Log($"JCD Asset Usage Scanner: checked {results.Count} candidates, search roots/assets: {searchPaths.Count}");
    }

    private List<string> CollectCandidatePaths()
    {
        if (scanMode == ScanMode.WholeProject)
        {
            return FindAssetsInFolders(new[] { "Assets" });
        }

        List<string> folders = candidateFolders
            .Where(f => f != null)
            .Select(AssetDatabase.GetAssetPath)
            .Where(path => !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
            .Distinct()
            .ToList();

        if (folders.Count == 0)
        {
            EditorUtility.DisplayDialog("No candidate folders", "Add at least one candidate folder.", "OK");
            return new List<string>();
        }

        return FindAssetsInFolders(folders.ToArray());
    }

    private List<string> CollectSearchPaths()
    {
        List<string> roots = new List<string>();

        if (scanWholeAssets)
        {
            roots.Add("Assets");
        }
        else
        {
            string scanRootPath = scanRootFolder != null ? AssetDatabase.GetAssetPath(scanRootFolder) : null;

            if (!string.IsNullOrEmpty(scanRootPath) && AssetDatabase.IsValidFolder(scanRootPath))
                roots.Add(scanRootPath);
            else
                roots.Add("Assets");
        }

        if (includePackagesAsSearchArea)
            roots.Add("Packages");

        return FindAssetsInFolders(roots.ToArray(), searchArea: true);
    }

    private List<string> FindAssetsInFolders(string[] folders, bool searchArea = false)
    {
        string filter = filterMode == FilterMode.AllAssetTypes ? string.Empty : BuildCommonArtFilter();

        List<string> paths = new List<string>();

        foreach (string folder in folders)
        {
            if (string.IsNullOrEmpty(folder))
                continue;

            if (!AssetDatabase.IsValidFolder(folder) && folder != "Packages")
                continue;

            string[] guids = AssetDatabase.FindAssets(filter, new[] { folder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!IsValidAssetPath(path, searchArea))
                    continue;

                if (!includeScenesInSearch && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (showOnlyDirectFolderAssets && !searchArea)
                {
                    bool isDirect = folders.Any(root =>
                        string.Equals(Path.GetDirectoryName(path)?.Replace('\\', '/'), root.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)
                    );

                    if (!isDirect)
                        continue;
                }

                paths.Add(path);
            }
        }

        return paths.Distinct().OrderBy(p => p).ToList();
    }

    private string BuildCommonArtFilter()
    {
        // Unity FindAssets supports OR-like tokens here.
        // This is not exhaustive, but covers most art/game content.
        return string.Join(" ", new[]
        {
            "t:Prefab",
            "t:Material",
            "t:Texture",
            "t:Texture2D",
            "t:Sprite",
            "t:Mesh",
            "t:Model",
            "t:Shader",
            "t:TerrainLayer",
            "t:AudioClip",
            "t:AnimationClip",
            "t:AnimatorController",
            "t:Avatar",
            "t:PhysicMaterial",
            "t:ScriptableObject",
            "t:Font"
        });
    }

    private void ScanDependencies(HashSet<string> candidateSet, List<string> searchPaths)
    {
        int count = searchPaths.Count;

        try
        {
            for (int i = 0; i < count; i++)
            {
                string sourcePath = searchPaths[i];

                if (string.IsNullOrEmpty(sourcePath))
                    continue;

                // true = recursive dependencies, so scene -> prefab -> material -> texture is detected.
                string[] dependencies;

                try
                {
                    dependencies = AssetDatabase.GetDependencies(sourcePath, true);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not read dependencies for {sourcePath}: {e.Message}");
                    continue;
                }

                foreach (string dependencyPath in dependencies)
                {
                    if (!candidateSet.Contains(dependencyPath))
                        continue;

                    // Do not count asset as used only because GetDependencies returns itself.
                    if (dependencyPath == sourcePath)
                        continue;

                    if (!resultByPath.TryGetValue(dependencyPath, out UsageItem item))
                        continue;

                    item.used = true;

                    if (!item.usedBy.Contains(sourcePath))
                        item.usedBy.Add(sourcePath);
                }

                if (i % 40 == 0)
                {
                    bool cancel = EditorUtility.DisplayCancelableProgressBar(
                        "Scanning dependencies",
                        sourcePath,
                        count <= 0 ? 1f : (float)i / count
                    );

                    if (cancel)
                        break;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private bool IsValidAssetPath(string path, bool searchArea)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (protectMetaAndFolders)
        {
            if (path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (AssetDatabase.IsValidFolder(path))
            return false;

        if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            return false;

        // For delete candidates, do not include package files. Packages are usually read-only or external.
        if (!searchArea && path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private bool CanDelete(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            return false;

        if (AssetDatabase.IsValidFolder(path))
            return false;

        if (protectScriptsFromDelete && path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            return false;

        if (protectMetaAndFolders && path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private void MoveSelectedUnusedToTrash()
    {
        List<UsageItem> selected = results
            .Where(r => !r.used && r.selectedForDelete && CanDelete(r.path))
            .ToList();

        if (selected.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing selected", "Select unused assets first.", "OK");
            return;
        }

        string preview = string.Join("\n", selected.Take(20).Select(s => s.path));
        if (selected.Count > 20)
            preview += $"\n...and {selected.Count - 20} more";

        bool confirm = EditorUtility.DisplayDialog(
            "Move UNUSED assets to Trash?",
            $"Selected: {selected.Count}\n\n{preview}\n\nFiles will be moved to OS Trash, not permanently deleted.",
            "Move to Trash",
            "Cancel"
        );

        if (!confirm)
            return;

        int moved = 0;
        foreach (UsageItem item in selected)
        {
            if (AssetDatabase.MoveAssetToTrash(item.path))
            {
                moved++;
            }
            else
            {
                Debug.LogWarning($"Could not move to trash: {item.path}");
            }
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Moved to Trash: {moved}/{selected.Count}", "OK");
        Scan();
    }

    private static void Ping(string path)
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (asset == null)
            return;

        EditorGUIUtility.PingObject(asset);
    }

    private static void SelectAsset(string path)
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (asset == null)
            return;

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}
#endif
