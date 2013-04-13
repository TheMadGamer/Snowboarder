using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {
	public GUIText textDisplay;
	public GameObject snowboarder;

	void Start ()
	{
		textDisplay.material.color = Color.black;
	}
	
	void OnGUI()
	{	
		if (snowboarder != null) {
			textDisplay.text = ((int)snowboarder.transform.position.z).ToString() + " feet";
		} else {
			textDisplay.text = "Snowboarder not connected to GUI.";
		}
	}
}
