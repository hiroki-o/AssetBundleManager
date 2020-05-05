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

        private Vector2 m_scroll;

        [MenuItem("Window/AssetBundle Manager/Open Control Panel...", false, 4)]
        public static void Open () {
            GetWindow<AssetBundleManagerControlPanel>();
        }

        private void Init() {
            this.titleContent = new GUIContent("ABM Server");
        }

        public void OnEnable () {
            Init();
        }

        public void OnFocus() {
        }

        public void OnDisable() {
        }

        public void OnGUI () {

            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scroll)) {
                m_scroll = scroll.scrollPosition;

                var gs = GlobalSettings.GetGlobalSettings();
                
                GUILayout.Label ("ABM Settings", "BoldLabel");
                if (gs.Settings.Count > 0)
                {
                    var displayedOptions = gs.Settings.Select(v => v == null ? "(null)" : v.name).ToArray();
                    var newIndex = EditorGUILayout.Popup("Active Settings", gs.ActiveSettingIndex, displayedOptions);
                    if (gs.ActiveSettingIndex != newIndex && displayedOptions[newIndex] != "(null)")
                    {
                        gs.ActiveSettingIndex = newIndex;
                        EditorUtility.SetDirty(gs);
                    }
                }
                if (gs.Settings.Count == 0)
                {
                    GUILayout.Space(8f);
                    EditorGUILayout.HelpBox("ABM Settings not found.", MessageType.Info);
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(GlobalSettings.GetActiveSettings() == null))
                    {
                        if (GUILayout.Button("Edit Active Settings"))
                        {
                            Selection.activeObject = GlobalSettings.GetActiveSettings();
                        }
                    }
                    
                    if (GUILayout.Button("Edit Global Settings"))
                    {
                        Selection.activeObject = GlobalSettings.GetGlobalSettings();
                    }
                }
                
                if (gs.Settings.Count == 0)
                {
                    return;
                }
                
                GUILayout.Space(12f);
                GUILayout.Label ("Editor Settings", "BoldLabel");

                var activeSetting = GlobalSettings.GetActiveSettings();
                var curSetting = activeSetting.CurrentSetting;

                var bCache = EditorGUILayout.Toggle("Clear Cache On Play", activeSetting.ClearCacheOnPlay);
                if (bCache != activeSetting.ClearCacheOnPlay)
                {
                    activeSetting.ClearCacheOnPlay = bCache;
                }
                
                var namesWithSim = new List<string>
                {
                    "Simulation Mode",
                    "Simulation Mode(AssetGraph)"
                };

                if (activeSetting.ServerSettings != null && activeSetting.ServerSettings.Count > 0)
                {
                    namesWithSim.AddRange(activeSetting.ServerSettings.Select(x => x.Name));
                }

                var namesWithSimArray = namesWithSim.ToArray();

                var editorIndex = 0;
                if (activeSetting.Mode == Settings.AssetBundleManagerMode.SimulationMode) {
                    editorIndex = 0;
                }
                else if (activeSetting.Mode == Settings.AssetBundleManagerMode.SimulationModeGraphTool) {
                    editorIndex = 1;
                }
                else {
                    editorIndex = (activeSetting.CurrentSetting == null) ? -1 : activeSetting.ServerSettings.IndexOf(activeSetting.CurrentSetting) + 2;
                }
                
                var newEditorIndex = EditorGUILayout.Popup("Editor Setting", editorIndex, namesWithSimArray);
                if (newEditorIndex != editorIndex) {
                    if (newEditorIndex == 0) {
                        activeSetting.Mode = Settings.AssetBundleManagerMode.SimulationMode;
                    } else if (newEditorIndex == 1) {
                        activeSetting.Mode = Settings.AssetBundleManagerMode.SimulationModeGraphTool;
                    } else {
                        activeSetting.Mode = Settings.AssetBundleManagerMode.Server;
                        activeSetting.CurrentSetting = activeSetting.ServerSettings[newEditorIndex - 2];
                    }
                }

                if (curSetting != null) {
                    GUILayout.Space(12f);
                    GUILayout.Label ("Editor AssetBundle Server", "BoldLabel");
                    
                    bool isRunning = LaunchAssetBundleServer.IsRunning();
                    EditorGUILayout.LabelField ("Local Server Running", isRunning.ToString());
                    EditorGUILayout.LabelField ("Server URL", curSetting.ServerURL);

                    EditorGUILayout.HelpBox (LaunchAssetBundleServer.GetServerArgs(), MessageType.Info);

                    using (new EditorGUI.DisabledScope(activeSetting.Mode != Settings.AssetBundleManagerMode.Server || curSetting.ServerType != ServerSettingType.Local)) {
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
