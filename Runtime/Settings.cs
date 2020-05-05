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
        
        public ServerSetting CurrentSetting {
            get {
                #if UNITY_EDITOR
                return m_currentSetting;
                #elif DEBUG
                return DevelopmentBuildSetting;
                #else
                return ReleaseBuildSetting;
                #endif
            }
            #if UNITY_EDITOR
            set {
                m_currentSetting = value;
                EditorUtility.SetDirty(this);
            }
            #endif
        }

        #if UNITY_EDITOR
        public bool ClearCacheOnPlay
        {
            get { return m_clearCacheOnPlay; }
            set
            {
                m_clearCacheOnPlay = value;
                EditorUtility.SetDirty(this);
            }
        }

        public List<ServerSetting> ServerSettings
        {
            get => m_settings;
            set
            {
                m_settings = value;                
                EditorUtility.SetDirty(this);
            } 
        }
        #endif

        public ServerSetting DevelopmentBuildSetting {
            get {
                return m_devBuildSetting;
            }
            #if UNITY_EDITOR
            set {
                m_devBuildSetting = value;
                EditorUtility.SetDirty(this);
            }
            #endif
        }

        public ServerSetting ReleaseBuildSetting {
            get {
                return m_releaseBuildSetting;
            }
            #if UNITY_EDITOR
            set {
                m_releaseBuildSetting = value;
                EditorUtility.SetDirty(this);
            }
            #endif
        }

        public ServerSetting StreamingAssetsSetting {
            get {
                return m_streamingAssetSetting;
            }
            #if UNITY_EDITOR
            set {
                m_streamingAssetSetting = value;
                EditorUtility.SetDirty(this);
            }
            #endif
        }

        public AssetBundleManagerMode Mode {
            get{
                return m_mode;   
            }
            #if UNITY_EDITOR
            set {
                m_mode = value;
                EditorUtility.SetDirty (this);
            }
            #endif
        }

        public AssetMap Map {
            get {
                return m_assetMap;
            }
            #if UNITY_EDITOR
            set {
                m_assetMap = value;
                EditorUtility.SetDirty (this);
            }
            #endif
        }
    }
}
