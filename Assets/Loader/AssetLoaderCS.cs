using UnityEngine;
using System.Collections;



public class AssetLoaderCS: MonoBehaviour 
{
	WWW download;
	public string url = "asset_to_load.unity3d";
	public string resourcePath = "";
	
	AssetBundle assetBundle;
	Object instanced;


/*	WWW StartDownload()
	 {
		// if we set the url to a prefixed path, we download from there
		if (url.IndexOf("file://") == 0 || url.IndexOf("http://") == 0)
		{
			download = new WWW(url);
		}
		
		// if we're in a web player, then do this....
		if(Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer)
		{
			Debug.Log("Web player download");
			download = new WWW(url);
		}
		// else do this...
		else if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			Debug.Log("App Path " + Application.dataPath);
			download = new WWW("file://" + Application.dataPath + "/../" + url);
		}
		
		//yield return download;

		assetBundle = download.assetBundle;
		
		if(assetBundle != null)
		{
			object gameObj = assetBundle.mainAsset;
			
			if(go != null)
			{
				instanced = Instantiate(gameObj);
			}
			else
			{
				Debug.Log("Could not instance");
			}
			
		}
		else
		{
			Debug.Log("Could not download");
		}
	}
	*/
	
	// Use this for initialization
	void Start () {
		if(download == null)
		{
		//	StartDownload();
		}
		
	}
	
}
