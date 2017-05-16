using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace AssetBundles {
    public class AssetBundleManagerControlPanel : EditorWindow {

        private static AssetBundleManagerControlPanel s_window;

//        private Texture2D m_localServerButtonIcon;
//        private Texture2D m_clearCacheOnPlayButtonIcon;
        private int m_loadFrom;

        internal class Config {
            internal static readonly string LOCAL_SERVER_BUTTON_ICON = "Assets/AssetBundleManager/Editor/Graphics/localserver.psd";
            internal static readonly string CLEAR_CACHE_BUTTON_ICON = "Assets/AssetBundleManager/Editor/Graphics/clearcache.psd";
            internal static readonly string[] LOADFROM = new string[] {
            "Regular Bundle Path",
            "GraphTool Output"
            };
        }

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
            this.titleContent = new GUIContent("ABMgr Control");
//            m_localServerButtonIcon = 
//                LoadTextureFromFile (Config.LOCAL_SERVER_BUTTON_ICON);
//
//            m_clearCacheOnPlayButtonIcon = 
//                LoadTextureFromFile (Config.CLEAR_CACHE_BUTTON_ICON);

            m_loadFrom = (AssetBundleManager.UseGraphToolBundle) ? 1 : 0;
        }


        public void OnEnable () {
            Init();
        }

        public void OnFocus() {
        }

        public void OnDisable() {
        }

        public void OnGUI () {

            using (new EditorGUILayout.VerticalScope()) {

                bool newClearCache = GUILayout.Toggle(AssetBundleManager.CleanCacheOnPlay, 
                    "Clear Cache On Play");
                bool newLocalServer = GUILayout.Toggle(LaunchAssetBundleServer.IsRunning(), 
                    "Local Server");

                bool newSimulationMode = GUILayout.Toggle(AssetBundleManager.SimulateAssetBundleInEditor, 
                    "Simulation mode");
                
                if (newClearCache != AssetBundleManager.CleanCacheOnPlay) {
                    AssetBundleManager.CleanCacheOnPlay = newClearCache;
                }
                if (newSimulationMode != AssetBundleManager.SimulateAssetBundleInEditor) {
                    AssetBundleManager.SimulateAssetBundleInEditor = newSimulationMode;
                }
                if (newLocalServer != LaunchAssetBundleServer.IsRunning()) {
                    LaunchAssetBundleServer.ToggleLocalAssetBundleServer ();
                }

                GUILayout.Space(12f);

                int idx = EditorGUILayout.Popup (m_loadFrom, Config.LOADFROM);
                if (idx != m_loadFrom) {
                    m_loadFrom = idx;
                    if (LaunchAssetBundleServer.IsRunning ()) {
                        LaunchAssetBundleServer.ToggleLocalAssetBundleServer ();
                    }
                    AssetBundleManager.UseGraphToolBundle = (idx == 1);
                    m_loadFrom = idx;
                }

                var url = EditorGUILayout.TextField("Server URL", AssetBundleManager.BaseDownloadingURL);
                if (url != AssetBundleManager.BaseDownloadingURL) {
                    AssetBundleManager.BaseDownloadingURL = url;
                }
                GUILayout.Space(8f);

                EditorGUILayout.HelpBox (LaunchAssetBundleServer.GetServerArgs(), MessageType.Info);

                GUILayout.Space(8f);
            }
        }
    }
}
