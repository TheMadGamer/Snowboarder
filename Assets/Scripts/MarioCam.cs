using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit")]

public class MarioCam : MonoBehaviour {

	public Transform target;
	public float distance;


	private Vector3 camPosition;

	void Start() {
		
	}
	
	public float GetGroundHeight(Vector3 position)
	{
		RaycastHit hit;
		Physics.Raycast(position + Vector3.up*10000.0f, Vector3.down, out hit);
		return hit.point.y;
	}
		
	void Update () {
	
		//Orbit
    	if (target) {

 		    
			/*
			
           	float y = Mathf.Max(orbitPosition.y, GetGroundHeight(orbitPosition) + minElevation);
	      	transform.position = new Vector3(orbitPosition.x, y, orbitPosition.z);
    	    
    	  //  transform.rotation = Quaternion.LookRotation(target.position, Vector3.up);
    	    //transform.rotation = orbitRotation;
    	    transform.LookAt(target, Vector3.up);
    	    	
			//Pan
			if (Input.GetKey(KeyCode.Space)) { 
				transform.Translate(transform.right * -Input.GetAxis("Mouse X") * panSpeed, Space.World); 
				transform.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World); 
			} 
			
        	//Zoom
			if (Input.GetKey(KeyCode.LeftAlt)) { 
				transform.Translate(transform.forward * -Input.GetAxis("Mouse X") * panSpeed, Space.World); 
			} */
		}
	}

	float ClampAngle (float angle, float min, float max) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
	
	
}
