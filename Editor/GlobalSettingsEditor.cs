using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetBundles.Manager {
    [CustomEditor(typeof(GlobalSettings))]
    public class GlobalSettingsEditor : Editor {

        Vector2 m_scroll;

        // [SerializeField] List<Settings> m_settings;
        // [SerializeField] private int m_activeSettingIndex;
        
        [SerializeField] private SerializedProperty m_settingsProperty;
        [SerializeField] private SerializedProperty m_activeSettingIndexProperty;

        public override void OnInspectorGUI () {

            var gs = target as GlobalSettings;
            m_settingsProperty = serializedObject.FindProperty("m_settings");
            m_activeSettingIndexProperty = serializedObject.FindProperty("m_activeSettingIndex");

            serializedObject.Update();

            using (var scroll = new EditorGUILayout.ScrollViewScope(m_scroll))
            {
                m_scroll = scroll.scrollPosition;
                var removingIndex = -1;

                if (gs.Settings.Count > 0)
                {
                    for (var i = 0; i < gs.Settings.Count; ++i)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(gs.Settings[i].name);
                            if (GUILayout.Button("Edit", GUILayout.Width(40f)))
                            {
                                Selection.activeObject = gs.Settings[i];
                            }
                            if (GUILayout.Button("-", GUILayout.Width(20f)))
                            {
                                removingIndex = i;
                            }
                        }
                    }

                    if (removingIndex >= 0)
                    {
                        gs.Settings.RemoveAt(removingIndex);
                        EditorUtility.SetDirty(gs);
                    }
                }
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(20f)))
                    {
                        var selectedItem = AddNewABMSetting();
                        if (selectedItem != null && !gs.Settings.Contains(selectedItem))
                        {
                            gs.Settings.Add(selectedItem);
                            EditorUtility.SetDirty(gs);
                        }
                    }
                }
            }
            
            GUILayout.Space(12f);

            if (GUILayout.Button("Create New ABM Settings"))
            {
                var newItem = CreateNewABMSetting();
                if (newItem != null)
                {
                    gs.Settings.Add(newItem);
                    EditorUtility.SetDirty(gs);
                }
            }

            GUILayout.Space(12f);

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

            serializedObject.ApplyModifiedProperties ();
        }

        private static Settings AddNewABMSetting()
        {
            var selected = EditorUtility.OpenFilePanelWithFilters("Select ABM Settings", "Assets/Resources", new[] {"ABM Settings", "asset"});
            if (string.IsNullOrEmpty (selected)) {
                return null;
            }

            var subPath = selected.Substring(selected.IndexOf("Assets"));
            var obj = AssetDatabase.LoadAssetAtPath<Settings>(subPath);
            
            return obj;
        }

        private static Settings CreateNewABMSetting()
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            
            var path =
                EditorUtility.SaveFilePanelInProject (
                    "Create New ABM Settings", 
                    "New ABM Settings", "asset", 
                    "Create a new asset graph:",
                    "Assets/Resources");
            if (string.IsNullOrEmpty (path)) {
                return null;
            }
            
            var data = CreateInstance<Settings>();
            AssetDatabase.CreateAsset(data, path);

            return data;
        }
    }
}
