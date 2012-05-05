using UnityEngine;
using System.Collections;
using DebugStates;

// Note that MonoBehaviour should not be namespaced, but stay global
// This class is a mechanism for Unity's Animation Events to message the state machine engine
public class DebugEventDispatch : MonoBehaviour {
	
	// Use this for initialization
	public void DispatchToParent(int animId)
	{
		Debug.Log("Event dispatch " + animId.ToString());
				
		switch( animId)
		{
			case 0:
				SendMessageUpwards("HandleEvent", DebugStates.CharacterEvents.To_InAir);
				break;
			
			case 1:
				SendMessageUpwards("HandleEvent", DebugStates.CharacterEvents.To_Ski);
				break;
		}
		
	}
	
}
