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
		AddBackRow();
		if (ShouldLayoutNextRow()) {
			LayoutNextRow();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (ShouldLayoutNextRow()) {
			LayoutNextRow();
		}
	}
	
	bool ShouldLayoutNextRow() {

		// If target is too close to edge, layout next level.
		return false;
	}
	
	void AddBackRow() {
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.transform.localScale = new Vector3(10,10,10);
		plane.transform.position = transform.position;
		plane.transform.rotation = transform.rotation;
		plane.transform.parent = transform;
	}
	
	void LayoutNextRow () {
		Transform targetTransform = target.transform;

		for (int x = -10 ; x <= 10; x++) {
			for (int y = -10; y <= 10; y++) {	
				
				int numRocks = ((int) Random.value * 2) + 1;
				for (int n = 0 ; n < numRocks; n++) { 
					GameObject newRock = (GameObject) GameObject.Instantiate(rock);
					
					Vector3 forward = tempMesh.transform.forward;
					Vector3 binormal = Vector3.Cross(forward, tempMesh.transform.up);
					Vector3 scale = new Vector3(2000,2000,2000); //tempMesh.transform.lossyScale;
					Debug.Log("sc " + scale.ToString());
					
					Vector3 f =  new Vector3(forward.x * scale.x,
											 forward.y * scale.y,
											 forward.z * scale.z);
					Vector3 b =  new Vector3(binormal.x * scale.x,
											 binormal.y * scale.y,
											 binormal.z * scale.z);
						
					float subX = ((float) x) / 10.0f;
					float subY = ((float) y) / 10.0f;
					Debug.Log( subX.ToString() + " " + subY.ToString());
					
					Vector3 newPt =  tempMesh.transform.position + b * subX + f * subY;	
					Debug.Log( b.ToString());
					Debug.Log( f.ToString());
					
					
					newRock.transform.position = newPt;
				}
			}
		}
	}
}
