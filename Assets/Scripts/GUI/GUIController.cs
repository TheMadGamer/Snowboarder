using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {
	public GUIText textDisplay;
	public GameObject snowboarder;
	public GamePlayController gamePlayController;

	void Start ()
	{
		textDisplay.material.color = Color.black;
	}
	
	void OnGUI ()
	{	
		if (snowboarder != null) {
			textDisplay.text = ((int)snowboarder.transform.position.z).ToString () + " feet";
		} else {
			textDisplay.text = "Snowboarder not connected to GUI.";
		}
		
		if (gamePlayController.isGameOver ()) {
			float width = 100;
			float height = 30;
			if (GUI.Button (new Rect (512 - width / 2, 100, width, height), "Restart")) {
				Application.LoadLevel (Application.loadedLevel);
			}
			GUIStyle style = new GUIStyle ();
			style.normal.textColor = Color.black;
			GUI.Label (new Rect (512 - width / 2, 768 / 2 - height, width, height), "GAME OVER!", style);
		}
	}
}
