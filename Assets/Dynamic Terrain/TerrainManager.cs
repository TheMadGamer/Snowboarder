using UnityEngine;
using System.Collections;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class TerrainManager : MonoBehaviour {
	
	public GameObject rock;
	public GameObject tree;
	
	// The target transform - the mesh is recentered around this object every Update
	public GameObject target;
	
	public Material snowMaterial;
	
  	public Vector3[] mVertices;
    public Vector2[] mUVs;
    public int[] mTriangles;
	
	const int kNumHorizontalVertices = 11;
	const int kNumVerticalVertices = 11;
	
	void Start() {
    	GenerateVertices();
		
		Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = mVertices;
        mesh.uv = mUVs;
        mesh.triangles = mTriangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		renderer.material = snowMaterial;
		
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		// Create a new mesh for the mesh collider and a grid mesh to reference it
		meshCollider = GetComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		
    }
	// Update is called once per frame
	void Update () {

	}
	
	void GenerateVertices() {
		mVertices = new Vector3[kNumHorizontalVertices * kNumVerticalVertices];
		mUVs = new Vector2[kNumHorizontalVertices * kNumVerticalVertices];
		mTriangles = new int[(kNumHorizontalVertices - 1) * (kNumVerticalVertices - 1) * 6];
		
		for (int i = 0; i < kNumVerticalVertices; i++) {
			for (int j = 0; j < kNumHorizontalVertices; j++) {
				mVertices[i * kNumVerticalVertices + j] = VertexAtIndex(i, j);
			}
		}
		
		for (int i = 0; i < kNumVerticalVertices; i++) {
			for (int j = 0; j < kNumHorizontalVertices; j++) {
				mUVs[i * kNumVerticalVertices + j] = UVAtIndex(i, j);
			}
		}
		
		for (int i = 0; i < kNumVerticalVertices - 1; i++) {
			TriangulateRow(i);
		}
	}
	
	void TriangulateRow(int triangleRowIndex) {	
		for (int j = 0; j < kNumHorizontalVertices - 1; j++) {
			int baseVertexIndex = triangleRowIndex * kNumHorizontalVertices + j; 
			int baseTriangleIndex = (triangleRowIndex * (kNumHorizontalVertices - 1) + j) * 6;
			Debug.Log( "BaseTriangleIndex " + baseTriangleIndex.ToString());
			mTriangles[baseTriangleIndex] = baseVertexIndex;
			mTriangles[baseTriangleIndex + 1] = baseVertexIndex + 1;
			mTriangles[baseTriangleIndex + 2] = baseVertexIndex + kNumHorizontalVertices;	

			mTriangles[baseTriangleIndex + 3] = baseVertexIndex + kNumHorizontalVertices;
			mTriangles[baseTriangleIndex + 4] = baseVertexIndex + 1;
			mTriangles[baseTriangleIndex + 5] = baseVertexIndex + kNumHorizontalVertices + 1;		
		}
	}
	
	Vector3 VertexAtIndex(int i, int j) {
		// TODO randomize the y component to get a displacement map
		return new Vector3((i -  Mathf.Floor(kNumVerticalVertices / 2)) * 10.0f, 0, (j - Mathf.Floor(kNumHorizontalVertices / 2)) * 10.0f);
	}
	
	Vector2 UVAtIndex(int i, int j) {
		return new Vector2( ((float)i)/((float)kNumVerticalVertices), ((float)j)/((float)kNumHorizontalVertices) );	
	}
	
	void ShiftVerticesDown() {
		Vector3[] oldVertices = mVertices;
		Vector2[] oldUVs = mUVs;
		mVertices = new Vector3[oldVertices.Length];
		//  TODO
		
	
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
	/*
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
	*/
}
