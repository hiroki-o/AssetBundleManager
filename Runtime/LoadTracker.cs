using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles.Manager;
using UnityEngine;

public class LoadTracker : MonoBehaviour
{
    [SerializeField] private List<string> m_loadedBundleName;

    public static LoadTracker GetOrCreateLoadTracker(string name)
    {
        var trackerObject = GameObject.Find(name);
        if (trackerObject == null)
        {
            trackerObject = new GameObject(name);
            trackerObject.AddComponent<LoadTracker>();
            DontDestroyOnLoad(trackerObject);
        }

        return trackerObject.GetComponent<LoadTracker>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_loadedBundleName = new List<string>();
        var manager = AssetBundleManager.GetManager();
        manager.onBundleLoaded += OnBundleLoaded;
    }

    private void OnBundleLoaded(string bundleName)
    {
        m_loadedBundleName.Add(bundleName);
    }

    private void OnDestroy()
    {
        var manager = AssetBundleManager.GetManager();
        manager.onBundleLoaded -= OnBundleLoaded;
    }

    public void UnloadAllTrackedBundles()
    {
        var manager = AssetBundleManager.GetManager();

        if (m_loadedBundleName != null)
        {
            foreach (var bundleName in m_loadedBundleName)
            {
                manager.UnloadAssetBundle(bundleName);
            }
            
            m_loadedBundleName.Clear();
        }
    }
}
