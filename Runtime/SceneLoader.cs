using UnityEngine;
using System;
using System.Collections;

namespace AssetBundles.Manager {
    public class SceneLoader : MonoBehaviour
    {
        public static string currentSceneName;

        public string sceneName;
        public bool isAdditive;
        public bool startOnLoad = false;

        public GameObject loadEventHandler;

    	// Use this for initialization
    	IEnumerator Start ()
    	{
	        while (!startOnLoad)
	        {
		        yield return null;
	        }
	        
    		yield return StartCoroutine(Initialize() );
    		
    		// Load level.
            yield return StartCoroutine(InitializeLevelAsync (sceneName, isAdditive) );
    	}

        public void BeginLoading()
        {
	        startOnLoad = true;
        }

    	// Initialize the downloading url and AssetBundleManifest object.
    	protected IEnumerator Initialize()
    	{
            currentSceneName = sceneName;

            // Don't destroy this gameObject as we depend on it to run the loading script.
    		DontDestroyOnLoad(gameObject);

            var manager = AssetBundleManager.GetManager();
            if (!manager.IsInitialized)
            {
	            // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
	            var request = AssetBundleManager.GetManager().Initialize();
    		
	            if (request != null) {
		            yield return StartCoroutine (request);
	            }
            }
    	}

    	protected IEnumerator InitializeLevelAsync (string sceneNamePath, bool isAdditive)
        {
	        var manager = AssetBundleManager.GetManager();
            var levelName = System.IO.Path.GetFileNameWithoutExtension (sceneNamePath);
            var sceneAssetBundle = manager.Settings.Map.GetAssetBundleName (sceneNamePath);

    		// This is simply to get the elapsed time for this phase of AssetLoading.
    		float startTime = Time.realtimeSinceStartup;

    		// Load level from assetBundle.
            AssetBundleLoadOperation request = manager.LoadLevelAsync(sceneAssetBundle, levelName, isAdditive);
            if (request == null) {
                if (loadEventHandler != null) {
                    loadEventHandler.SendMessage ("OnSceneLoadError");
                }
                yield break;
            }
    		yield return StartCoroutine(request);

            if (request.IsError()) {
                if (loadEventHandler != null) {
                    loadEventHandler.SendMessage ("OnSceneLoadError");
                }
                yield break;
            }

            var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName (levelName);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene (loadedScene);

            // Calculate and display the elapsed time.
    		float elapsedTime = Time.realtimeSinceStartup - startTime;
    		Debug.Log("Finished loading scene " + levelName + " in " + elapsedTime + " seconds" );

            if (loadEventHandler != null) {
                loadEventHandler.SendMessage ("OnSceneLoaded");
            }

            Destroy (gameObject);
    	}
    }
}