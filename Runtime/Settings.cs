using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System.Net;
using System.Net.Sockets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetBundles.Manager {
    public class Settings : ScriptableObject {

        public class Path {

            public static string SettingsFileName       { get { return "AssetBundleManagerSettings"; } }

            #if UNITY_EDITOR
            public static string BasePath {
                get {
                    string baseDirPath = BaseFullPath;

                    int index = baseDirPath.LastIndexOf (ASSETS_PATH);
                    Assert.IsTrue ( index >= 0 );

                    baseDirPath = baseDirPath.Substring (index);

                    return baseDirPath;
                }
            }

            public static string BaseFullPath {
                get {
                    var obj = ScriptableObject.CreateInstance<Settings> ();
                    MonoScript s = MonoScript.FromScriptableObject (obj);
                    var configGuiPath = AssetDatabase.GetAssetPath( s );
                    UnityEngine.Object.DestroyImmediate (obj);

                    var fileInfo = new FileInfo(configGuiPath);
                    var baseDir = fileInfo.Directory.Parent;

                    Assert.AreEqual ("AssetBundleManager", baseDir.Name);

                    return baseDir.ToString ();
                }
            }

            public const string ASSETS_PATH = "Assets/";

            public static string ResourcesPath          { get { return ASSETS_PATH + "Resources/"; } }
            public static string SettingsFilePath       { get { return ResourcesPath + SettingsFileName + ".asset"; } }
            #endif
        }

        public enum AssetBundleManagerMode : int {
            SimulationMode,
            SimulationModeGraphTool,
            Server
        }

        [SerializeField] List<ServerSetting> m_settings;
        [SerializeField] ServerSetting m_currentSetting;
        [SerializeField] ServerSetting m_devBuildSetting;
        [SerializeField] ServerSetting m_releaseBuildSetting;
        [SerializeField] ServerSetting m_streamingAssetSetting;
        [SerializeField] AssetBundleManagerMode m_mode;
        [SerializeField] AssetMap m_assetMap;
        [SerializeField] private bool m_clearCacheOnPlay;
        #if UNITY_EDITOR
        [SerializeField] private int m_version;
        private const int VERSION = 1;
        #endif

        private static Settings s_settings;

        private static Settings GetSettings() {
            if(s_settings == null) {
                if(!Load()) {
                    // Create vanilla db
                    s_settings = ScriptableObject.CreateInstance<Settings>();
                    s_settings.m_settings = new List<ServerSetting>();
                    #if UNITY_EDITOR
                    s_settings.m_version = VERSION;

                    var DBDir = Path.ResourcesPath;

                    if (!Directory.Exists(DBDir)) {
                        Directory.CreateDirectory(DBDir);
                    }

                    AssetDatabase.CreateAsset(s_settings, Path.SettingsFilePath);
                    AssetDatabase.SaveAssets();
                    #endif
                }
            }

            return s_settings;
        }

        private static bool Load() {
            bool loaded = false;

            #if UNITY_EDITOR
            try {
                var dbPath = Path.SettingsFilePath;

                if(File.Exists(dbPath)) 
                {
                    Settings m = AssetDatabase.LoadAssetAtPath<Settings>(dbPath);

                    if(m != null && m.m_version == VERSION) {
                        s_settings = m;
                        loaded = true;
                    }
                }
            } catch(Exception e) {
                Debug.LogException (e);
            }
            #else
            Settings s = Resources.Load(Path.SettingsFileName) as Settings;
            s_settings = s;
            loaded = true;
            #endif

            return loaded;
        }

        public static ServerSetting CurrentSetting {
            get {
                #if UNITY_EDITOR
                return GetSettings ().m_currentSetting;
                #elif DEBUG
                return DevelopmentBuildSetting;
                #else
                return ReleaseBuildSetting;
                #endif
            }
            #if UNITY_EDITOR
            set {
                var s = GetSettings();
                s.m_currentSetting = value;
                EditorUtility.SetDirty(s);
            }
            #endif
        }

        #if UNITY_EDITOR
        public static bool ClearCacheOnPlay {
            get {
               return GetSettings ().m_clearCacheOnPlay;
            }
            set {
                var s = GetSettings();
                s.m_clearCacheOnPlay = value;
                EditorUtility.SetDirty(s);
            }
        }

        public static List<ServerSetting> ServerSettings {
            get{ 
                return GetSettings ().m_settings;
            }
        }
        #endif

        public static ServerSetting DevelopmentBuildSetting {
            get {
                return GetSettings ().m_devBuildSetting;
            }
            #if UNITY_EDITOR
            set {
                var s = GetSettings();
                s.m_devBuildSetting = value;
                EditorUtility.SetDirty(s);
            }
            #endif
        }

        public static ServerSetting ReleaseBuildSetting {
            get {
                return GetSettings ().m_releaseBuildSetting;
            }
            #if UNITY_EDITOR
            set {
                var s = GetSettings();
                s.m_releaseBuildSetting = value;
                EditorUtility.SetDirty(s);
            }
            #endif
        }

        public static ServerSetting StreamingAssetsSetting {
            get {
                return GetSettings ().m_streamingAssetSetting;
            }
            #if UNITY_EDITOR
            set {
                var s = GetSettings();
                s.m_streamingAssetSetting = value;
                EditorUtility.SetDirty(s);
            }
            #endif
        }

        public static AssetBundleManagerMode Mode {
            get{
                return GetSettings ().m_mode;   
            }
            #if UNITY_EDITOR
            set {
                var s = GetSettings ();
                s.m_mode = value;
                EditorUtility.SetDirty (s);
            }
            #endif
        }

        public static AssetMap Map {
            get {
                return GetSettings().m_assetMap;
            }
            #if UNITY_EDITOR
            set {
                var s = GetSettings ();
                s.m_assetMap = value;
                EditorUtility.SetDirty (s);
            }
            #endif
        }

        #if UNITY_EDITOR
            public static ServerSetting CreateServerSetting(string name, ServerSettingType t) {
            var newSetting = ServerSetting.CreateServerSetting (name, t);

            AssetDatabase.AddObjectToAsset (newSetting, Path.SettingsFilePath);

            var s = GetSettings ();
            s.m_settings.Add (newSetting);
            EditorUtility.SetDirty (s);

            return newSetting;
        }

        public static void RemoveServerSetting(ServerSetting removingSetting) {
            var s = GetSettings();
            s.m_settings.Remove(removingSetting);
            if(s.m_currentSetting == removingSetting) {
                s.m_currentSetting = null;
            }
            if(s.m_devBuildSetting == removingSetting) {
                s.m_devBuildSetting = null;
            }
            if(s.m_releaseBuildSetting == removingSetting) {
                s.m_releaseBuildSetting = null;
            }
            if(s.m_streamingAssetSetting== removingSetting) {
                s.m_streamingAssetSetting = null;
            }
            ScriptableObject.DestroyImmediate (removingSetting, true);
            EditorUtility.SetDirty (s);
        }

        public static void SetSettingsDirty() {
            EditorUtility.SetDirty(s_settings);
        }
        #endif
	}
}
