using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetBundles.Manager {
    [CustomEditor(typeof(AssetLoader))]
    public class AssetLoaderEditor : Editor {

        [SerializeField] private SerializedProperty m_assetPath;

        public override bool RequiresConstantRepaint() {
            return true;
        }

        public override void OnInspectorGUI () {

            m_assetPath = serializedObject.FindProperty("assetPath");

            var map = Settings.Map;
            List<string> assets = null;
            if (map != null) {
                assets = map.GetAllAssets ();
            }

            if (assets == null || assets.Count == 0) {
                EditorGUILayout.HelpBox ("Assets in Asset Bundle Map is empty. Please build asset bundles.", MessageType.Info);
                return;
            }

            var currentSceneName = m_assetPath.stringValue;
            int sceneIndex = 0;
            if (!string.IsNullOrEmpty (currentSceneName)) {
                var found = assets.IndexOf (currentSceneName);
                if (found >= 0) {
                    sceneIndex = found;
                } else {
                    m_assetPath.stringValue = assets [0];
                }
            } else {
                m_assetPath.stringValue = assets [0];
            }

            var newSceneIndex = EditorGUILayout.Popup ("Scene", sceneIndex, assets.ToArray());
            if (newSceneIndex != sceneIndex) {
                m_assetPath.stringValue = assets[newSceneIndex];
            }

            serializedObject.ApplyModifiedProperties ();
        }
    }
}
