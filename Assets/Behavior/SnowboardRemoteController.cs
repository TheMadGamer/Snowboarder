using UnityEngine;
using System.Collections;

public class SnowboardRemoteController : SnowboardController 
{

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) 
	{
		Vector3 velocity = Vector3.zero,  position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		float lean = 0, pivot = 0;
		
	    if (!stream.isWriting) 
	    {
        	stream.Serialize(ref position);
        	stream.Serialize(ref rotation);
        	stream.Serialize(ref velocity);
        	stream.Serialize(ref pivot);
        	stream.Serialize(ref lean);

			transform.position = position;
			transform.rotation = rotation;
        	mVelocity = velocity;
        	mSteeringControl.Lean = lean;
        	//mPivot = pivot;
	    }    
	}
	
	void Update()
	{
		//UpdatePosition();
	}
	
}
