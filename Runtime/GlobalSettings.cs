using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetBundles.Manager {
    public class GlobalSettings : ScriptableObject
    {
        public class Path
        {
            public static string SettingsFileName
            {
                get { return "AssetBundleManagerGlobalSettings"; }
            }

#if UNITY_EDITOR
            public static string BasePath
            {
                get
                {
                    string baseDirPath = BaseFullPath;

                    int index = baseDirPath.LastIndexOf(ASSETS_PATH);
                    Assert.IsTrue(index >= 0);

                    baseDirPath = baseDirPath.Substring(index);

                    return baseDirPath;
                }
            }

            public static string BaseFullPath
            {
                get
                {
                    var obj = ScriptableObject.CreateInstance<Settings>();
                    MonoScript s = MonoScript.FromScriptableObject(obj);
                    var configGuiPath = AssetDatabase.GetAssetPath(s);
                    UnityEngine.Object.DestroyImmediate(obj);

                    var fileInfo = new FileInfo(configGuiPath);
                    var baseDir = fileInfo.Directory.Parent;

                    Assert.AreEqual("AssetBundleManager", baseDir.Name);

                    return baseDir.ToString();
                }
            }

            public const string ASSETS_PATH = "Assets/";

            public static string ResourcesPath
            {
                get { return ASSETS_PATH + "Resources/"; }
            }

            public static string SettingsFilePath
            {
                get { return ResourcesPath + SettingsFileName + ".asset"; }
            }
#endif
        }

        [SerializeField] List<Settings> m_settings;
        [SerializeField] private int m_activeSettingIndex;
        
        private static GlobalSettings s_gs;

#if UNITY_EDITOR
        public List<Settings> Settings
        {
            get => m_settings;
            set => m_settings = value;
        }

        public int ActiveSettingIndex
        {
            get => m_activeSettingIndex;
            set => m_activeSettingIndex = value;
        }
#endif
        
        public static GlobalSettings GetGlobalSettings()
        {
            if (s_gs == null)
            {
                if (!Load())
                {
                    // Create vanilla db
                    s_gs = CreateInstance<GlobalSettings>();
                    s_gs.m_settings = new List<Settings>();
                    s_gs.m_activeSettingIndex = -1;
#if UNITY_EDITOR

                    var DBDir = Path.ResourcesPath;

                    if (!Directory.Exists(DBDir))
                    {
                        Directory.CreateDirectory(DBDir);
                    }

                    AssetDatabase.CreateAsset(s_gs, Path.SettingsFilePath);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
            return s_gs;
        }

        private static bool Load()
        {
            var loaded = false;

#if UNITY_EDITOR
            try
            {
                var dbPath = Path.SettingsFilePath;

                if (File.Exists(dbPath))
                {
                    var m = AssetDatabase.LoadAssetAtPath<GlobalSettings>(dbPath);
                    if (m != null)
                    {
                        s_gs = m;
                        loaded = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            GlobalSettings s = Resources.Load(Path.SettingsFileName) as GlobalSettings;
            s_gs = s;
            loaded = true;
#endif
            return loaded;
        }

        private Settings GetActiveSettingsInternal()
        {
            // if (m_activeSetting == null)
            // {
            //     if (m_activeSettingIndex >= 0 && m_activeSettingIndex < m_settingPath.Count)
            //     {
            //         return Resources.Load(m_settingPath[m_activeSettingIndex]) as Settings;
            //     }
            //
            //     return null;
            // }
            //
            // return m_activeSetting;

            if (m_settings.Count == 0)
            {
                return null;
            }

            if (m_activeSettingIndex >= m_settings.Count)
            {
                return null;
            }

            return m_settings[m_activeSettingIndex];
        }
        
        private bool SetActiveSettingsByNameInternal(string name)
        {
            var found = m_settings.FindIndex(s => s.name == name);
            if (found >= 0)
            {
                m_activeSettingIndex = found;
            }

            return found >= 0;
        }

        public static Settings GetActiveSettings()
        {
            return GetGlobalSettings().GetActiveSettingsInternal();
        }
        
        public static bool SetActiveSettingsByName(string name)
        {
            return GetGlobalSettings().SetActiveSettingsByNameInternal(name);
        }
    }
}
