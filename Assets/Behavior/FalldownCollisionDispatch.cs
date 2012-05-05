using UnityEngine;
using System.Collections;

public class FalldownCollisionDispatch : MonoBehaviour {
	
	public GameObject AnimatedRig;
	public GameObject Ragdoll;
	
	void OnTriggerEnter()
	{
		Debug.Log("Triggered" + name);

		SendMessageUpwards("HandleEvent", SnowboardStates.CharacterEvents.To_FallDown);
		Debug.Log("Enable rag doll");
		gameObject.active = false;

	}
	
}
