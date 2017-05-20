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
                    var obj = ScriptableObject.CreateInstance<Settings> ();
                    MonoScript s = MonoScript.FromScriptableObject (obj);
                    var configGuiPath = AssetDatabase.GetAssetPath( s );
                    UnityEngine.Object.DestroyImmediate (obj);

                    var fileInfo = new FileInfo(configGuiPath);
                    var baseDir = fileInfo.Directory;

                    Assert.AreEqual ("AssetBundleManager", baseDir.Name);

                    string baseDirPath = baseDir.ToString ();

                    int index = baseDirPath.LastIndexOf (ASSETS_PATH);
                    Assert.IsTrue ( index >= 0 );

                    baseDirPath = baseDirPath.Substring (index);

                    return baseDirPath;
                }
            }

            public const string ASSETS_PATH = "Assets/";

            public static string ResourcesPath          { get { return BasePath + "/Resources/"; } }
            public static string SettingsFilePath       { get { return ResourcesPath + "/" + SettingsFileName + ".asset"; } }

            public static string GUIResourceBasePath    { get { return BasePath + "/Editor/Graphics/"; } }
            #endif
        }
//		public class GUI {
//            public static string ClearCache               { get { return Path.GUIResourceBasePath + "NodeStyle.guiskin"; } }
//            public static string LocalServer    { get { return Path.GUIResourceBasePath + "ConnectionPoint.png"; } }
//            public static string InputBG            { get { return Path.GUIResourceBasePath + "InputBG.png"; } }
//            public static string OutputBG           { get { return Path.GUIResourceBasePath + "OutputBG.png"; } }
//		}

        [Serializable]
        public class ServerSetting {
            static string s_localServerURL;

            [SerializeField] private string m_name;
            [SerializeField] private string m_serverURL;
            [SerializeField] private string m_localAssetBundleDirectory;
            [SerializeField] private bool m_isLocalServer;

            public ServerSetting(string name, bool isLocalServer) {
                m_name = name;
                m_isLocalServer = isLocalServer;
                m_localAssetBundleDirectory = string.Empty;
                m_serverURL = string.Empty;
            }

            public string Name {
                get {
                    return m_name;
                }

                #if UNITY_EDITOR
                set {
                    m_name = value;
                    Settings.SetSettingsDirty ();
                }
                #endif
            }

            public string ServerURL {
                get {
                    if (m_isLocalServer) {
                        return GetLocalServerURL ();
                    } else {
                        return m_serverURL;
                    }
                }
                #if UNITY_EDITOR
                set {
                    m_serverURL = value;
                    Settings.SetSettingsDirty ();
                }
                #endif
            }

            public string AssetBundleDirectory {
                get {
                    return m_localAssetBundleDirectory;
                }
                #if UNITY_EDITOR
                set {
                    m_localAssetBundleDirectory = value;
                    Settings.SetSettingsDirty ();
                }
                #endif
            }

            public bool IsLocalServer {
                get {
                    return m_isLocalServer;
                }
            }

            private static string GetLocalServerURL() {
                if (s_localServerURL == null) {
                    IPHostEntry host;
                    string localIP = "";
                    string hostName = Dns.GetHostName ();
                    try {
                        host = Dns.GetHostEntry(hostName);
                    } catch (SocketException ) {
                        host = Dns.GetHostEntry("127.0.0.1");
                    }

                    foreach (IPAddress ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIP = ip.ToString();
                            break;
                        }
                    }
                    s_localServerURL = "http://"+localIP+":7888/";
                }
                return s_localServerURL;
            }
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
        [SerializeField] AssetBundleManagerMode m_mode;
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

        public static IEnumerable<Settings.ServerSetting> ServerSettings {
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

        #if UNITY_EDITOR
        public static ServerSetting CreateServerSetting(string name, bool isLocalServer) {
            var newSetting = new ServerSetting (name, isLocalServer);
            var s = GetSettings ();
            s.m_settings.Add (newSetting);
            s.m_currentSetting = newSetting;
            EditorUtility.SetDirty (s);

            return newSetting;
        }

        public static void RemoveServerSetting(ServerSetting removingSetting) {
            var s = GetSettings();
            s.m_settings.Remove(removingSetting);
            if(s.m_currentSetting == removingSetting) {
                s.m_currentSetting = null;
            }
        }

        public static void SetSettingsDirty() {
            EditorUtility.SetDirty(s_settings);
        }
        #endif
	}
}
