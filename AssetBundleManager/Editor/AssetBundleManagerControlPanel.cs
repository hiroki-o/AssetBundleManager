using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace AssetBundles.Manager {
    public class AssetBundleManagerControlPanel : EditorWindow {

        private static AssetBundleManagerControlPanel s_window;

        private string m_menuTitle;
        List<ServerSetting> m_removingItem;
        Vector2 m_scroll;

        [MenuItem("Window/AssetBundle Manager/Open Control Panel...", false, 4)]
        public static void Open () {
            GetWindow<AssetBundleManagerControlPanel>();
        }

        public static Texture2D LoadTextureFromFile(string path) {
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        private void Init() {
            this.titleContent = new GUIContent("ABM Server");
            m_removingItem = new List<ServerSetting>();

            ConfigureMenuSelectionName ();
        }

        private void ConfigureMenuSelectionName () {
            if(Settings.Mode == Settings.AssetBundleManagerMode.SimulationMode) {
                m_menuTitle = "Simulation Mode";
            }
            else if(Settings.Mode == Settings.AssetBundleManagerMode.SimulationModeGraphTool) {
                m_menuTitle = "Simulation Mode(GraphTool)";
            }
            else {
                var s = Settings.CurrentSetting;
                if(s != null) {
                    m_menuTitle = s.Name;
                } else {
                    m_menuTitle = "<select server>";
                }
            }
        }

        public void OnEnable () {
            Init();
        }

        public void OnFocus() {
        }

        public void OnDisable() {
        }

        public string DrawFolderSelector(string label, 
            string dialogTitle, 
            string currentDirPath, 
            string directoryOpenPath, 
            Func<string, string> onValidFolderSelected = null) 
        {
            string newDirPath;
            using(new EditorGUILayout.HorizontalScope()) {
                if (string.IsNullOrEmpty (label)) {
                    newDirPath = EditorGUILayout.TextField(currentDirPath);
                } else {
                    newDirPath = EditorGUILayout.TextField(label, currentDirPath);
                }

                if(GUILayout.Button("Select", GUILayout.Width(50f))) {
                    var folderSelected = 
                        EditorUtility.OpenFolderPanel(dialogTitle, directoryOpenPath, "");
                    if(!string.IsNullOrEmpty(folderSelected)) {
                        if (onValidFolderSelected != null) {
                            newDirPath = onValidFolderSelected (folderSelected);
                        } else {
                            newDirPath = folderSelected;
                        }
                    }
                }
            }
            return newDirPath;
        }

        public void OnGUI () {

            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scroll)) {
                m_scroll = scroll.scrollPosition;

                bool newClearCache = EditorGUILayout.ToggleLeft("Clear Cache On Play", AssetBundleManager.CleanCacheOnPlay);
                if (newClearCache != AssetBundleManager.CleanCacheOnPlay) {
                    Settings.ClearCacheOnPlay = newClearCache;
                }

                GUILayout.Space(12f);
                GUILayout.Label ("Server Settings", "BoldLabel");
                foreach (var s in Settings.ServerSettings) {
                    using (new EditorGUILayout.VerticalScope ("box")) {
                        var newName = EditorGUILayout.TextField("Name", s.Name);
                        if (newName != s.Name) {
                            s.Name = newName;
                            ConfigureMenuSelectionName ();
                        }

                        var newType = (ServerSettingType)EditorGUILayout.EnumPopup ("Server Type", s.ServerType);
                        if (newType != s.ServerType) {
                            s.ServerType = newType;
                        }

                        switch (s.ServerType) {
                        case ServerSettingType.Local:
                            {
                                var newFolder = DrawFolderSelector ("AssetBundle Directory", 
                                                    "Select AssetBundle Directory", 
                                                    s.AssetBundleDirectory,
                                                    Application.dataPath + "/../");
                                if (newFolder != s.AssetBundleDirectory) {
                                    s.AssetBundleDirectory = newFolder;
                                }
                            }
                            break;
                        case ServerSettingType.Remote:
                            {
                                var url = EditorGUILayout.TextField ("Server URL", s.ServerURL);
                                if (url != s.ServerURL) {
                                    s.ServerURL = url;
                                }
                            }
                            break;
                        }

                        bool newWithPlatformDir = EditorGUILayout.ToggleLeft("Use Platform Subdirectory", s.UsePlatformSubDir);
                        if (newWithPlatformDir != s.UsePlatformSubDir) {
                            s.UsePlatformSubDir = newWithPlatformDir;
                        }

                        using (new EditorGUI.DisabledScope (s.UsePlatformSubDir)) {
                            if (string.IsNullOrEmpty (s.ManifestFileName)) {
                                if (s.ServerType == ServerSettingType.Local) {
                                    s.ManifestFileName = Path.GetFileName (s.AssetBundleDirectory);
                                }
                            }
                            var newManifestName = EditorGUILayout.TextField ("Manifest File Name", s.ManifestFileName);
                            if (newManifestName != s.ManifestFileName) {
                                s.ManifestFileName = newManifestName;
                            }
                        }

                        if(GUILayout.Button("Remove")) {
                            m_removingItem.Add (s);
                        }
                    }
                }

                if (m_removingItem.Count > 0) {
                    foreach (var r in m_removingItem) {
                        Settings.RemoveServerSetting (r);
                    }
                    m_removingItem.Clear ();
                }

                GUILayout.Space(8f);
                if(GUILayout.Button("Add Server Setting")) {
                    Settings.CreateServerSetting ("New Setting", ServerSettingType.Local);
                }


                GUILayout.Space(12f);
                GUILayout.Label ("Editor Server Setting", "BoldLabel");


                var names = Settings.ServerSettings.Select (x => x.Name).ToArray ();
                var namesWithSim = new string[names.Length + 2];
                namesWithSim [0] = "Simulation Mode";
                namesWithSim [1] = "Simulation Mode(GraphTool)";
                Array.Copy (names, 0, namesWithSim, 2, names.Length);

                int editorIndex = 0;
                if (Settings.Mode == Settings.AssetBundleManagerMode.SimulationMode) {
                    editorIndex = 0;
                }
                else if (Settings.Mode == Settings.AssetBundleManagerMode.SimulationModeGraphTool) {
                    editorIndex = 1;
                }
                else {
                    editorIndex = (Settings.CurrentSetting == null) ? -1 : Settings.ServerSettings.IndexOf(Settings.CurrentSetting) + 2;
                }

                int devIndex = (Settings.DevelopmentBuildSetting == null) ? -1 : Settings.ServerSettings.IndexOf(Settings.DevelopmentBuildSetting);
                int releaseIndex = (Settings.ReleaseBuildSetting == null) ? -1 : Settings.ServerSettings.IndexOf(Settings.ReleaseBuildSetting);
                int streamingIndex = (Settings.StreamingAssetsSetting == null) ? -1 : Settings.ServerSettings.IndexOf(Settings.StreamingAssetsSetting);


                var newEditorIndex = EditorGUILayout.Popup("Editor Setting", editorIndex, namesWithSim);

                GUILayout.Space(12f);
                GUILayout.Label ("Player Server Setting", "BoldLabel");

                var newDevIndex = EditorGUILayout.Popup("Development Build", devIndex, names);
                var newReleaseIndex = EditorGUILayout.Popup("Release Build", releaseIndex, names);
                var newStreamingAssetsIndex = EditorGUILayout.Popup("Streaming Assets", streamingIndex, names);

                if (newEditorIndex != editorIndex) {
                    if (newEditorIndex == 0) {
                        Settings.Mode = Settings.AssetBundleManagerMode.SimulationMode;
                    } else if (newEditorIndex == 1) {
                        Settings.Mode = Settings.AssetBundleManagerMode.SimulationModeGraphTool;
                    } else {
                        Settings.Mode = Settings.AssetBundleManagerMode.Server;
                        Settings.CurrentSetting = Settings.ServerSettings [newEditorIndex - 2];
                    }
                }
                if (newDevIndex != devIndex) {
                    if (Settings.ServerSettings.Count > newDevIndex) {
                        Settings.DevelopmentBuildSetting = Settings.ServerSettings [newDevIndex];
                    }
                }
                if (newReleaseIndex != releaseIndex) {
                    if (Settings.ServerSettings.Count > newReleaseIndex) {
                        Settings.ReleaseBuildSetting = Settings.ServerSettings [newReleaseIndex];
                    }
                }
                if (newStreamingAssetsIndex != streamingIndex) {
                    if (Settings.ServerSettings.Count > newStreamingAssetsIndex) {
                        var newSetting = Settings.ServerSettings [newStreamingAssetsIndex];
                        if (newSetting.ServerType == ServerSettingType.StreamingAssets) {
                            Settings.StreamingAssetsSetting = newSetting;
                        }
                    }
                }

                GUILayout.Space(20f);

                var curSetting = Settings.CurrentSetting;
                if (curSetting != null) {
                    bool isRunning = LaunchAssetBundleServer.IsRunning();
                    EditorGUILayout.LabelField ("Local Server Running", isRunning.ToString());
                    EditorGUILayout.LabelField ("Server URL", curSetting.ServerURL);

                    EditorGUILayout.HelpBox (LaunchAssetBundleServer.GetServerArgs(), MessageType.Info);

                    using (new EditorGUI.DisabledScope(Settings.Mode != Settings.AssetBundleManagerMode.Server || curSetting.ServerType != ServerSettingType.Local)) {
                        if (GUILayout.Button (isRunning ? "Stop Local Server": "Start Local Server")) {
                            LaunchAssetBundleServer.ToggleLocalAssetBundleServer ();
                        }
                    }
                    GUILayout.Space(8f);
                }
            }
        }
    }
}
