using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit")]

public class MouseOrbitCameraController : MonoBehaviour {

	public Transform target;
	public float distance;

	public float panSpeed = 10; 

	public float xSpeed = 250;
	public float ySpeed = 120;

	public float yMinLimit = -20;
	public float yMaxLimit = 80;
	
	public float minElevation = 3;

	private Vector2 orbit = Vector2.zero;

	private Quaternion orbitRotation;
	private Vector3 orbitPosition;

	void Start() {
	    orbit.x = transform.eulerAngles.y;
    	orbit.y = transform.eulerAngles.x;
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
			orbit.x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			orbit.y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			orbit.y = ClampAngle(orbit.y, yMinLimit, yMaxLimit);
 		       
			orbitRotation = Quaternion.Euler(orbit.y, orbit.x, 0);
			orbitPosition = orbitRotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
            
            
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
			} 
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
