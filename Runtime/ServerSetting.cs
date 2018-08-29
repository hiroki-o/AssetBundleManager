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

    public enum ServerSettingType {
        Local,
        Remote,
        StreamingAssets
    }

    public class ServerSetting : ScriptableObject {
        static string s_localServerURL;

        [SerializeField] private string m_serverURL;
        [SerializeField] private string m_localAssetBundleDirectory;
        [SerializeField] private string m_manifestFileName;
        [SerializeField] private ServerSettingType m_serverType;
        [SerializeField] private bool m_withPlatformSubDir;

        public static ServerSetting CreateServerSetting(string name, ServerSettingType t) {

            var newSetting = ScriptableObject.CreateInstance<ServerSetting> ();

            newSetting.name = name;
            newSetting.m_serverType = t;
            newSetting.m_withPlatformSubDir = false;
            newSetting.m_localAssetBundleDirectory = string.Empty;
            newSetting.m_serverURL = string.Empty;

            return newSetting;
        }


        public string Name {
            get {
                return this.name;
            }

            #if UNITY_EDITOR
            set {
                this.name = value;
                Settings.SetSettingsDirty ();
            }
            #endif
        }

        public string ServerURL {
            get {
                string url = string.Empty;
                switch (m_serverType) {
                case ServerSettingType.Local:
                    url = GetLocalServerURL ();
                    break;
                case ServerSettingType.Remote:
                    url = m_serverURL;
                    break;
                case ServerSettingType.StreamingAssets:
                    url = GetStreamingAssetsURL(m_serverURL);
                    break;
                }
                if (m_withPlatformSubDir) {
                    return string.Format ("{0}{1}/",url, Utility.GetPlatformName());
                } else {
                    return url;
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

        public string ManifestFileName {
            get {
                if (m_withPlatformSubDir) {
                    return Utility.GetPlatformName ();
                } else {
                    return m_manifestFileName;
                }
            }
            #if UNITY_EDITOR
            set {
                m_manifestFileName = value;
                Settings.SetSettingsDirty ();
            }
            #endif
        }

        public ServerSettingType ServerType {
            get {
                return m_serverType;
            }
            set {
                m_serverType = value;
            }
        }

        public bool UsePlatformSubDir {
            get {
                return m_withPlatformSubDir;
            }
            #if UNITY_EDITOR
            set {
                m_withPlatformSubDir = value;
                Settings.SetSettingsDirty ();
            }
            #endif
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

        private static string GetStreamingAssetsURL(string str) {
            return string.Format ("file://{0}/{1}", Application.streamingAssetsPath, str);
        }
    }
}
