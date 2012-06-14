using UnityEngine;
using System.Collections;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class TerrainManager : MonoBehaviour {
	
	public GameObject tempMesh;
	public GameObject rock;
	public GameObject tree;
	
	// The target transform - the mesh is recentered around this object every Update
	public GameObject target;
	
	private float lastRockTime;
	
	// Use this for initialization
	void Start () {
		lastRockTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
		lastRockTime += Time.deltaTime;
				
		if (lastRockTime > 3) {
			lastRockTime = 0;	
			AddRock();
		}
	}
	
	void AddRock() {
		Debug.Log(target.name);
		Transform targetTransform = target.transform;
		Vector3 forward = target.transform.forward;
		Debug.Log("Target transform " + targetTransform.position.ToString());
		Vector3 raycastPoint = targetTransform.position + forward * 20 + Vector3.up * 10.0f;
		RaycastHit hit;
		bool didHit = Physics.Raycast(raycastPoint, Vector3.down, out hit);
		if (didHit) {
			// Instantiate a point at hit.point
			GameObject newRock = (GameObject) GameObject.Instantiate(rock);
			newRock.transform.position = hit.point;
			Debug.Log("Hit Point" + hit.point);
		}
	}
}
