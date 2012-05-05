var download : WWW;
var url = "packed_resource.unity3d";
var resourcePath = "Lerpz";
var guiOffset = 20;
var assetBundle : AssetBundle;
var instanced : Object;

function StartDownload () {
	// Use fancy logic to get the actual path
	if (url.IndexOf ("file://") == 0 || url.IndexOf ("http://") == 0)
		download = new WWW (url);
	else if (Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer)
		download = new WWW (url);
	else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		download = new WWW ("file://" + Application.dataPath + "/../" + url);
	
	yield download;
	
	assetBundle = download.assetBundle;

	if (assetBundle != null)
	{
		// Alternatively you can also load an asset by name (assetBundle.Load("my asset name"))
		var go : Object = assetBundle.mainAsset;
			
		if (go != null)
			instanced = Instantiate(go);
		else
			Debug.Log("Couldnt load resource");	
	}
	else
	{
		Debug.Log("Couldnt load resource");	
	}
}


function OnGUI()
{
	
GUILayout.Space(guiOffset);
	GUILayout.BeginHorizontal();
	if (download == null)
	{
		
		if (GUILayout.Button("Download " + url))
			StartDownload();	
	}
	else
	{
		if (download.error == null)
		{
			var progress = parseInt(download.progress * 100);
			GUILayout.Label(progress + "%");	
			
			if (download.isDone && GUILayout.Button("Unload Resource")	)
			{
				// Destroy the instantiated object
				Destroy(instanced);
				// Dispose the WWW class
				// (This happens automatically from the GC, but you can do it explicitly to make sure it happens early on)
				download.Dispose();
				download = null;

				// Unload the whole asset bundles and any loaded assets
				assetBundle.Unload (true);
				assetBundle = null;
			}
		}
		else
		{
			GUILayout.Label(download.error);		
		}	
	}
	GUILayout.EndHorizontal();
	
}