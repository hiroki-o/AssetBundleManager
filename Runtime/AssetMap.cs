using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetBundles.Manager
{   
    [Serializable]
    public class AssetMap {

        [Serializable]
        public class AssetMapEntry
        {
            public string path;
            public string bundleName;
            public string typeFullName;

            public AssetMapEntry(string p, string b, string t) {
                path = p;
                bundleName = b.ToLower();
                typeFullName = t;
            }
        }

        [SerializeField] private List<AssetMapEntry> m_assets;
        [SerializeField] private string m_manifestName;

        public string GetAssetBundleName(string assetPath) {
            if (m_assets == null) {
                return string.Empty;
            }

            foreach (var e in m_assets) {
                if (assetPath == e.path) {
                    return e.bundleName;
                }
            }
            return string.Empty;
        }

        public string ManifestFileName {
            get {
                return m_manifestName;
            }
            #if UNITY_EDITOR
            set {
                m_manifestName = value;
            }
            #endif
        }

        public AssetMap() {
            m_assets = new List<AssetMapEntry> ();
        }

        #if UNITY_EDITOR
        public void AddAsset(string path, string typename, string assetBundleName) {
            m_assets.Add (new AssetMapEntry (path, assetBundleName, typename));
        }

        public List<string> GetAssetPathByType(string typeName) {
            var assets = new List<string> ();
            foreach (var e in m_assets) {
                if (e.typeFullName == typeName) {
                    assets.Add (e.path);
                }
            }

            return assets;
        }

        public List<string> GetScenes() {
            return GetAssetPathByType (typeof(UnityEditor.SceneAsset).FullName);
        }

        public List<string> GetAllAssets() {
            var assets = new List<string> ();
            foreach (var e in m_assets) {
                assets.Add (e.path);
            }

            return assets;
        }
        #endif
    }
}
