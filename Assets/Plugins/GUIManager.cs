using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	/*-----------------------------------Public Variables------------------------------------*/
	[HideInInspector]
	public GUICameras ptrCameraMgr;
	[HideInInspector]
	public GUITextMgr ptrTextMgr;
	[HideInInspector]
	public GUITextureMgr ptrTextureMgr;

	public enum CameraMode
	{
		Standard = 0,
		Perspective = 1
	}
	
	public int AllocBlockSize = 10;
	public int ScreenWidth = 800;
	public int ScreenHeight = 600;
	public CameraMode[] Cameras = new CameraMode[10];
	
	/*-----------------------------------Private Variables-----------------------------------*/

	/*----------------------------------------Methods----------------------------------------*/

	/*-------------------------------------Unity Methods-------------------------------------*/
	void Awake ()
	{
		GameObject tmpGameObj;
		
		//Initialize the Camera system
		tmpGameObj = new GameObject();
		tmpGameObj.name = "GUICameras";
		tmpGameObj.AddComponent("GUICameras");
		ptrCameraMgr = (GUICameras) tmpGameObj.GetComponent(typeof(GUICameras));
		ptrCameraMgr.Init();
		
		//Init GUIText system
		tmpGameObj = new GameObject();
		tmpGameObj.name = "GUITextMgr";
		tmpGameObj.AddComponent("GUITextMgr");
		ptrTextMgr = (GUITextMgr) tmpGameObj.GetComponent(typeof(GUITextMgr));
		ptrTextMgr.Init();
		
		//Init GUITexture system
		tmpGameObj = new GameObject();
		tmpGameObj.name = "GUITextureMgr";
		tmpGameObj.AddComponent("GUITextureMgr");
		ptrTextureMgr = (GUITextureMgr) tmpGameObj.GetComponent(typeof(GUITextureMgr));
		ptrTextureMgr.Init();
		
		//Init all quad managers and premade quad objects
		gameObject.BroadcastMessage("InitQuadMgr", null, SendMessageOptions.DontRequireReceiver);
	}
}
