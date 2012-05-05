using UnityEngine;
using System.Collections;

public class DebugCollisionDispatch : MonoBehaviour {

	public float downVelocity = 1;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float dt = Time.deltaTime;
		transform.position = new Vector3(transform.position.x, transform.position.y - downVelocity*dt, transform.position.z);
	}
	
	void OnTriggerEnter()
	{
		Debug.Log("Triggered" + name);
	}
	
	void OnCollisionEnter(Collision collision) 
	{
		Debug.Log("Collided");
		foreach (ContactPoint contact in collision.contacts) 
		{
			Debug.DrawRay(contact.point, contact.normal, Color.white);
		}
	}
}
