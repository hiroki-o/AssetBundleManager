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
            this.titleContent = new GUIContent("AssetBundle Manager");
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

        private void DrawServerSettingDropDown(Settings.ServerSetting current, string title, bool includeSimulation, bool includeAddServer, Func<Settings.ServerSetting, bool> selector) {

            if (GUILayout.Button(new GUIContent(title, "Select Server"), EditorStyles.popup)) {
                GenericMenu menu = new GenericMenu();

                if (includeSimulation) {
                    menu.AddItem(new GUIContent("Simulation Mode"), false, () => {
                        Settings.Mode = Settings.AssetBundleManagerMode.SimulationMode;
                        ConfigureMenuSelectionName ();
                    });
                    menu.AddItem(new GUIContent("Simulation Mode(GraphTool)"), false, () => {
                        Settings.Mode = Settings.AssetBundleManagerMode.SimulationModeGraphTool;
                        ConfigureMenuSelectionName ();
                    });
                    menu.AddSeparator ("");
                }

                foreach(var s in Settings.ServerSettings) {
                    var curSetting = s;
                    menu.AddItem(new GUIContent(curSetting.Name), false, () => {
                        selector(s);
                        Settings.SetSettingsDirty();
                    });
                }

                if (includeAddServer) {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("New Local Server..."), false, () => {
                        Settings.CreateServerSetting("New Local Server", true);
                        ConfigureMenuSelectionName ();
                    });
                    menu.AddItem(new GUIContent("New Remote Server..."), false, () => {
                        Settings.CreateServerSetting("New Remote Server", false);
                        ConfigureMenuSelectionName ();
                    });
                }

                menu.DropDown(new Rect(4f, 8f, 0f, 0f));
            }            
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

            using (new EditorGUILayout.VerticalScope()) {

                bool newClearCache = EditorGUILayout.ToggleLeft("Clear Cache On Play", AssetBundleManager.CleanCacheOnPlay);
                if (newClearCache != AssetBundleManager.CleanCacheOnPlay) {
                    Settings.ClearCacheOnPlay = newClearCache;
                }

                GUILayout.Space(12f);
                GUILayout.Label ("Editor Server Setting", "BoldLabel");
                DrawServerSettingDropDown (Settings.CurrentSetting, m_menuTitle, true, true, (Settings.ServerSetting s) => {
                    Settings.CurrentSetting = s;
                    Settings.Mode = Settings.AssetBundleManagerMode.Server;
                    ConfigureMenuSelectionName ();
                    return true;
                });

                var curSetting = Settings.CurrentSetting;
                if (curSetting != null) {
                    var newName = EditorGUILayout.TextField("Name", curSetting.Name);
                    if (newName != curSetting.Name) {
                        curSetting.Name = newName;
                        ConfigureMenuSelectionName ();
                    }

                    if (curSetting.IsLocalServer) {
                        var newFolder = DrawFolderSelector ("AssetBundle Directory", 
                            "Select AssetBundle Directory", 
                            curSetting.AssetBundleDirectory,
                            Application.dataPath + "/../");
                        if (newFolder != curSetting.AssetBundleDirectory) {
                            curSetting.AssetBundleDirectory = newFolder;
                        }
                    } else {
                        var url = EditorGUILayout.TextField("Server URL", curSetting.ServerURL);
                        if (url != curSetting.ServerURL) {
                            curSetting.ServerURL = url;
                        }
                    }
                }

                GUILayout.Space(8f);

                GUILayout.Space(4f);
                GUILayout.Label ("Development Server", "BoldLabel");
                string devTitle = (Settings.DevelopmentBuildSetting == null) ? "<select server>" : Settings.DevelopmentBuildSetting.Name;
                DrawServerSettingDropDown (Settings.DevelopmentBuildSetting, devTitle, false, false, (Settings.ServerSetting s) => {
                    Settings.DevelopmentBuildSetting = s;
                    return true;
                });
                GUILayout.Space(4f);
                GUILayout.Label ("Release Server", "BoldLabel");
                string relTitle = (Settings.ReleaseBuildSetting == null) ? "<select server>" : Settings.ReleaseBuildSetting.Name;
                DrawServerSettingDropDown (Settings.ReleaseBuildSetting, relTitle, false, false, (Settings.ServerSetting s) => {
                    Settings.ReleaseBuildSetting = s;
                    return true;
                });

                GUILayout.Space(20f);


                if (newClearCache != AssetBundleManager.CleanCacheOnPlay) {
                    Settings.ClearCacheOnPlay = newClearCache;
                }

                bool isRunning = LaunchAssetBundleServer.IsRunning();
                EditorGUILayout.LabelField ("Local Server Running", isRunning.ToString());
                EditorGUILayout.LabelField ("Server URL", curSetting.ServerURL);

                EditorGUILayout.HelpBox (LaunchAssetBundleServer.GetServerArgs(), MessageType.Info);

                using (new EditorGUI.DisabledScope(Settings.Mode != Settings.AssetBundleManagerMode.Server || curSetting == null || !curSetting.IsLocalServer)) {
                    if (GUILayout.Button (isRunning ? "Stop Local Server": "Start Local Server")) {
                        LaunchAssetBundleServer.ToggleLocalAssetBundleServer ();
                    }
                }
            }
        }
    }
}
