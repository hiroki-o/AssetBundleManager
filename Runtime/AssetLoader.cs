using UnityEngine;
using System.Collections;

namespace AssetBundles.Manager {
    public class AssetLoader : MonoBehaviour
    {
        public string assetPath;

    	// Use this for initialization
    	IEnumerator Start ()
    	{
    		yield return StartCoroutine(Initialize() );
    		
    		// Load asset.
            yield return StartCoroutine(InstantiateGameObjectAsync (assetPath) );
    	}

    	// Initialize the downloading url and AssetBundleManifest object.
    	protected IEnumerator Initialize()
    	{
    		// Don't destroy this gameObject as we depend on it to run the loading script.
    		DontDestroyOnLoad(gameObject);

    		// Initialize AssetBundleManifest which loads the AssetBundleManifest object.
    		var request = AssetBundleManager.Initialize();
    		if (request != null)
    			yield return StartCoroutine(request);
    	}

    	protected IEnumerator InstantiateGameObjectAsync (string assetPath)
    	{
            var assetName = System.IO.Path.GetFileNameWithoutExtension (assetPath).ToLower();
            var assetBundleName = Settings.Map.GetAssetBundleName (assetPath);

    		// This is simply to get the elapsed time for this phase of AssetLoading.
    		float startTime = Time.realtimeSinceStartup;

    		// Load asset from assetBundle.
    		AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject) );
    		if (request == null)
    			yield break;
    		yield return StartCoroutine(request);

    		// Get the asset.
    		GameObject prefab = request.GetAsset<GameObject> ();

    		if (prefab != null)
    			GameObject.Instantiate(prefab);
    		
    		// Calculate and display the elapsed time.
    		float elapsedTime = Time.realtimeSinceStartup - startTime;
    		Debug.Log(assetName + (prefab == null ? " was not" : " was")+ " loaded successfully in " + elapsedTime + " seconds" );
    	}
    }
}
