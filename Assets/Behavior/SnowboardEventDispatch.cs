using UnityEngine;
using System.Collections;
using SnowboardStates;


// This class is a mechanism for Unity's Animation Events to message the state machine engine
public class SnowboardEventDispatch : MonoBehaviour {
	
	// Use this for initialization
	public void DispatchToParent(int animId)
	{
		Debug.Log("Event dispatch " + animId.ToString());
				
		switch( animId)
		{
			case 0:
				SendMessageUpwards("HandleEvent", SnowboardStates.CharacterEvents.To_InAir);
				break;
			
			case 1:
				SendMessageUpwards("HandleEvent", SnowboardStates.CharacterEvents.To_Ski);
				break;
		}
		
	}
	
}
