using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetBundles.Manager {
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor {

        Vector2 m_scroll;
        
        [SerializeField] private SerializedProperty m_settingsProperty;
        [SerializeField] private SerializedProperty m_currentSettingProperty;
        [SerializeField] private SerializedProperty m_devBuildSettingProperty;
        [SerializeField] private SerializedProperty m_releaseBuildSettingProperty;
        [SerializeField] private SerializedProperty m_streamingAssetSettingProperty;
        [SerializeField] private SerializedProperty m_modeProperty;
        [SerializeField] private SerializedProperty m_assetMapProperty;
        [SerializeField] private SerializedProperty m_clearCacheOnPlayProperty;
        
        public override void OnInspectorGUI () {

            var settings = target as Settings;
            m_settingsProperty = serializedObject.FindProperty("m_settings");
            m_currentSettingProperty = serializedObject.FindProperty("m_currentSetting");
            m_devBuildSettingProperty = serializedObject.FindProperty("m_devBuildSetting");
            m_releaseBuildSettingProperty = serializedObject.FindProperty("m_releaseBuildSetting");
            m_streamingAssetSettingProperty = serializedObject.FindProperty("m_streamingAssetSetting");
            m_modeProperty = serializedObject.FindProperty("m_mode");
            m_assetMapProperty = serializedObject.FindProperty("m_assetMap");
            m_clearCacheOnPlayProperty = serializedObject.FindProperty("m_clearCacheOnPlay");

            serializedObject.Update();
            
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scroll)) {
                m_scroll = scroll.scrollPosition;

                GUILayout.Space(12f);
                GUILayout.Label ("Server Settings", "BoldLabel");

                var removingIndex = -1;
                
                for (var i = 0; i < m_settingsProperty.arraySize; ++ i)
                {
                    var settingPropertyElement = m_settingsProperty.GetArrayElementAtIndex(i);
                    var settingProperty = new SerializedObject(settingPropertyElement.objectReferenceValue);
                    
                    var nameProperty = settingProperty.FindProperty("m_Name");
                    var serverURLProperty = settingProperty.FindProperty("m_serverURL");
                    var localAssetBundleDirectoryProperty = settingProperty.FindProperty("m_localAssetBundleDirectory");
                    var manifestFileNameProperty = settingProperty.FindProperty("m_manifestFileName");
                    var serverTypeProperty = settingProperty.FindProperty("m_serverType");
                    var withPlatformSubDirProperty = settingProperty.FindProperty("m_withPlatformSubDir");
                    
                    using (new EditorGUILayout.VerticalScope ("box"))
                    {
                        EditorGUILayout.PropertyField(nameProperty);
                        EditorGUILayout.PropertyField(serverTypeProperty);

                        switch ((ServerSettingType)serverTypeProperty.enumValueIndex) {
                        case ServerSettingType.Local:
                            {
                                var newFolder = DrawFolderSelector ("AssetBundle Directory", 
                                                    "Select AssetBundle Directory", 
                                                    localAssetBundleDirectoryProperty.stringValue,
                                                    Application.dataPath + "/../");
                                if (newFolder != localAssetBundleDirectoryProperty.stringValue) {

                                    var projectPath = Directory.GetParent(Application.dataPath).ToString();

                                    if(projectPath == newFolder) {
                                        newFolder = string.Empty;
                                    } else {
                                        var index = newFolder.IndexOf(projectPath);
                                        if(index >= 0 ) {
                                            newFolder = newFolder.Substring(projectPath.Length + index);
                                            if(newFolder.IndexOf('/') == 0) {
                                                newFolder = newFolder.Substring(1);
                                            }
                                        }
                                    }

                                    localAssetBundleDirectoryProperty.stringValue = newFolder;
                                }
                            }
                            break;
                        case ServerSettingType.Remote:
                            {
                                EditorGUILayout.PropertyField(serverURLProperty);
                            }
                            break;
                        case ServerSettingType.StreamingAssets:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                        }

                        EditorGUILayout.PropertyField(withPlatformSubDirProperty, new GUIContent("Use Platform Subdirectory") );

                        using (new EditorGUI.DisabledScope (withPlatformSubDirProperty.boolValue)) {
                            if (string.IsNullOrEmpty (manifestFileNameProperty.stringValue)) {
                                if (serverTypeProperty.enumValueIndex == (int)ServerSettingType.Local) {
                                    manifestFileNameProperty.stringValue = Path.GetFileName (localAssetBundleDirectoryProperty.stringValue);
                                }
                            }
                            EditorGUILayout.PropertyField(manifestFileNameProperty, new GUIContent("Manifest File Name") );
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if(GUILayout.Button("-", GUILayout.Width(20f)))
                            {
                                removingIndex = i;
                            }
                        }

                        GUILayout.Space(8f);
                    }

                    settingProperty.ApplyModifiedProperties();
                }

                if (removingIndex >= 0)
                {
                    var serverSetting = settings.ServerSettings[removingIndex];
                    settings.ServerSettings.RemoveAt(removingIndex);
                    AssetDatabase.RemoveObjectFromAsset(serverSetting);
                    DestroyImmediate(serverSetting);
                    EditorUtility.SetDirty(target);
                    serializedObject.Update();
                }

                GUILayout.Space(8f);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button("+", GUILayout.Width(20f)))
                    {
                        var newServerSetting = CreateInstance<ServerSetting>();
                        var newObject = new SerializedObject(newServerSetting);
                        var nameProperty = newObject.FindProperty("m_Name");
                        var serverURLProperty = newObject.FindProperty("m_serverURL");
                        var localAssetBundleDirectoryProperty = newObject.FindProperty("m_localAssetBundleDirectory");
                        var manifestFileNameProperty = newObject.FindProperty("m_manifestFileName");
                        var serverTypeProperty = newObject.FindProperty("m_serverType");
                        var withPlatformSubDirProperty = newObject.FindProperty("m_withPlatformSubDir");

                        nameProperty.stringValue = "New Setting";
                        serverTypeProperty.enumValueIndex = (int) ServerSettingType.Local;
                        withPlatformSubDirProperty.boolValue = false;
                        serverURLProperty.stringValue = string.Empty;
                        localAssetBundleDirectoryProperty.stringValue = string.Empty;
                        manifestFileNameProperty.stringValue = string.Empty;
                        newObject.ApplyModifiedPropertiesWithoutUndo();

                        if (settings.ServerSettings == null)
                        {
                            settings.ServerSettings = new List<ServerSetting>();
                        }
                        settings.ServerSettings.Add(newServerSetting);
                        AssetDatabase.AddObjectToAsset(newServerSetting, AssetDatabase.GetAssetPath(target));
                        
                        EditorUtility.SetDirty(target);
                        serializedObject.Update();
                    }
                }

                GUILayout.Space(12f);
                GUILayout.Label ("Editor Server Setting", "BoldLabel");
                EditorGUILayout.PropertyField(m_clearCacheOnPlayProperty, new GUIContent("Clear Cache On Play"));

                var names = new List<string>();
                var namesWithSim = new List<string>
                {
                    "Simulation Mode",
                    "Simulation Mode(AssetGraph)"
                };

                if (settings.ServerSettings != null && settings.ServerSettings.Count > 0)
                {
                    names = settings.ServerSettings.Select(x => x.Name).ToList();
                    namesWithSim.AddRange(names);
                }

                var namesArray = names.ToArray();
                var namesWithSimArray = namesWithSim.ToArray();

                int editorIndex = 0;
                if (m_modeProperty.enumValueIndex == (int)Settings.AssetBundleManagerMode.SimulationMode) {
                    editorIndex = 0;
                }
                else if (m_modeProperty.enumValueIndex == (int) Settings.AssetBundleManagerMode.SimulationModeGraphTool) {
                    editorIndex = 1;
                }
                else {
                    editorIndex = (m_currentSettingProperty.objectReferenceValue == null) ? -1 : settings.ServerSettings.IndexOf(settings.CurrentSetting) + 2;
                }

                var newEditorIndex = EditorGUILayout.Popup("Editor Setting", editorIndex, namesWithSimArray);
                if (newEditorIndex != editorIndex) {
                    if (newEditorIndex == 0) {
                        m_modeProperty.enumValueIndex = (int)Settings.AssetBundleManagerMode.SimulationMode;
                    } else if (newEditorIndex == 1) {
                        m_modeProperty.enumValueIndex = (int)Settings.AssetBundleManagerMode.SimulationModeGraphTool;
                    } else {
                        m_modeProperty.enumValueIndex = (int)Settings.AssetBundleManagerMode.Server;
                        m_currentSettingProperty.objectReferenceValue = settings.ServerSettings [newEditorIndex - 2];
                    }
                }
                
                var devIndex = (m_devBuildSettingProperty.objectReferenceValue == null) ? -1 : settings.ServerSettings.IndexOf(settings.DevelopmentBuildSetting);
                var releaseIndex = (m_releaseBuildSettingProperty.objectReferenceValue == null) ? -1 : settings.ServerSettings.IndexOf(settings.ReleaseBuildSetting);
                var streamingIndex = (m_streamingAssetSettingProperty.objectReferenceValue == null) ? -1 : settings.ServerSettings.IndexOf(settings.StreamingAssetsSetting);
                
                GUILayout.Space(12f);
                GUILayout.Label ("Player Server Setting", "BoldLabel");

                var newDevIndex = EditorGUILayout.Popup("Development Build", devIndex, namesArray);
                var newReleaseIndex = EditorGUILayout.Popup("Release Build", releaseIndex, namesArray);
                var newStreamingAssetsIndex = EditorGUILayout.Popup("Streaming Assets", streamingIndex, namesArray);

                if (newDevIndex != devIndex) {
                    if (m_settingsProperty.arraySize > newDevIndex) {
                        m_devBuildSettingProperty.objectReferenceValue = settings.ServerSettings [newDevIndex];
                    }
                }
                if (newReleaseIndex != releaseIndex) {
                    if (m_settingsProperty.arraySize > newReleaseIndex) {
                        m_releaseBuildSettingProperty.objectReferenceValue = settings.ServerSettings [newReleaseIndex];
                    }
                }
                if (newStreamingAssetsIndex != streamingIndex) {
                    if (m_settingsProperty.arraySize > newStreamingAssetsIndex) {
                        var newSetting = settings.ServerSettings [newStreamingAssetsIndex];
                        if (newSetting.ServerType == ServerSettingType.StreamingAssets) {
                            m_streamingAssetSettingProperty.objectReferenceValue = newSetting;
                        }
                    }
                }

                GUILayout.Space(12f);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(m_assetMapProperty, new GUIContent("Asset Map"), true);
                }
            }
            
            serializedObject.ApplyModifiedProperties ();
        }
        
        
        private string DrawFolderSelector(string label, 
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
    }
}
