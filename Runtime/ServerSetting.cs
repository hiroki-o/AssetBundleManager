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
        
        public string Name => name;

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
        }

        public string AssetBundleDirectory => m_localAssetBundleDirectory;

        public string ManifestFileName => m_withPlatformSubDir ? Utility.GetPlatformName () : m_manifestFileName;

        public ServerSettingType ServerType => m_serverType;

        public bool UsePlatformSubDir => m_withPlatformSubDir;

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
            return $"file://{Application.streamingAssetsPath}/{str}";
        }
    }
}
