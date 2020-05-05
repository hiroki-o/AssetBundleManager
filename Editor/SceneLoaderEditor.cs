using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetBundles.Manager {
    [CustomEditor(typeof(SceneLoader))]
    public class SceneLoaderEditor : Editor {

        [SerializeField] private SerializedProperty m_sceneNamePath;
        [SerializeField] private SerializedProperty m_loadEventHandler;
        [SerializeField] private SerializedProperty m_startOnLoad;
        [SerializeField] private SerializedProperty m_isAdditive;

        public override bool RequiresConstantRepaint() {
            return true;
        }

        public override void OnInspectorGUI () {

            m_sceneNamePath = serializedObject.FindProperty("sceneName");
            m_loadEventHandler = serializedObject.FindProperty("loadEventHandler");
            m_startOnLoad = serializedObject.FindProperty("startOnLoad");
            m_isAdditive = serializedObject.FindProperty("isAdditive");

            var settings = GlobalSettings.GetActiveSettings();
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Active ABM Settings not found. Please configure from ABM Server Control Panel.", MessageType.Error);
                return;
            }
            
            var map = settings.Map;
            List<string> scenes = null;
            if (map != null) {
                scenes = map.GetScenes ();
            }

            if (scenes == null || scenes.Count == 0) {
                EditorGUILayout.HelpBox ("Scenes in Asset Bundle Map is empty. Please build asset bundles with scenes first.", MessageType.Info);
                return;
            }

            var currentSceneName = m_sceneNamePath.stringValue;
            int sceneIndex = 0;
            if (!string.IsNullOrEmpty (currentSceneName)) {
                var found = scenes.IndexOf (currentSceneName);
                if (found >= 0) {
                    sceneIndex = found;
                } else {
                    m_sceneNamePath.stringValue = scenes [0];
                }
            } else {
                m_sceneNamePath.stringValue = scenes [0];
            }

            var newSceneIndex = EditorGUILayout.Popup ("Scene", sceneIndex, scenes.ToArray());
            if (newSceneIndex != sceneIndex) {
                m_sceneNamePath.stringValue = scenes[newSceneIndex];
            }

            EditorGUILayout.PropertyField (m_loadEventHandler);
            EditorGUILayout.PropertyField(m_startOnLoad);
            EditorGUILayout.PropertyField(m_isAdditive);

            serializedObject.ApplyModifiedProperties ();
        }
    }
}
