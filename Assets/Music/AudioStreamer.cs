using UnityEngine;
using System.Collections;

public class AudioStreamer : MonoBehaviour {

	// for testing in editor
	public string localURL = "http://localhost/Powder/Music/TakeTheLead.ogg";
	
	// when up on the site
	public string prodURL =  "http://www.ef5.us/Powder/Music/TakeTheLead.ogg";
	
	WWW www;
	
	void Start() 
	{
		if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			www = new WWW(localURL);
		}
		else
		{
			www = new WWW(prodURL);
		}
		
		audio.clip = www.audioClip;
		
	}
	
/*	void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.red;
		GUI.Label(new Rect(20, 20, 400, 30), "WWW.error " + www.error + " " + (www.isDone ? "XXX - Done " :  "Not Done"), style) ;
	}*/
		
	void Update() {

		
		if ( !audio.isPlaying && audio.clip != null && audio.clip.isReadyToPlay)
		{
			audio.Play();
		}
		
	}
	
}
