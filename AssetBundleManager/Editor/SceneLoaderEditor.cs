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
        [SerializeField] private SerializedProperty m_disableAfterLoad;

        public override bool RequiresConstantRepaint() {
            return true;
        }

        public override void OnInspectorGUI () {

            m_sceneNamePath = serializedObject.FindProperty("sceneName");
            m_disableAfterLoad = serializedObject.FindProperty("disableAfterLoad");

            var map = Settings.Map;
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

            EditorGUILayout.PropertyField (m_disableAfterLoad);

            serializedObject.ApplyModifiedProperties ();
        }
    }
}
