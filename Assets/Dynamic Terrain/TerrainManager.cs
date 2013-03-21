using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

// Terrain generation manager.  Creates rows of terrain as the target (snowboarder)
// gets closer.
// TODO: create an undulation noise pattern.
// TODO: create side panels as needed.
public class TerrainManager : MonoBehaviour {
	
	public GameObject rock;
	public GameObject tree;
	
	// The target transform - the mesh is recentered around this object every Update.
	public GameObject target;
	
	public GameObject meshPrefab;
	
	public Material snowMaterial;
		
	const int kNumQuadSideVertices = 2;
	const float kTileSpan = 100;
		
	public Dictionary<TileInds, GameObject> TileMap = new Dictionary<TileInds, GameObject>();
	
	public class TileInds {
		private int x;
		private int y;
		public int X { get{ return x; } }
		public int Y { get{ return y; } }
		public TileInds(int x, int y) 
		{
			this.x = x;
			this.y = y;
		}
		
		public override int GetHashCode ()
		{
			return x + 100000 + y;
		}
		
		public override bool Equals (object obj)
		{
			TileInds other = obj as TileInds;
			if (other != null) {
				return this.x == other.x && this.y == other.y;
			}
			return false;
		}
	}
	
	void Start() 
	{
		rock.transform.position = new Vector3();
		UpdateTiles();
    }
	
	void Update() 
	{
		UpdateTiles();
	}
	
	void UpdateTiles() 
	{
		List<TileInds> inds = 
			GetNeededTiles(target.transform.position.x, target.transform.position.z);
		
		foreach (TileInds tileInds in inds) 
		{
			if (!TileMap.ContainsKey(tileInds)) 
			{
				Debug.Log("Adding tile " + tileInds.ToString());
				GameObject tile = GenerateQuad(tileInds);	
				TileMap.Add(tileInds, tile);
				
				// Parent to this.
				//tile.transform.parent = transform;
			}
		}	
	}
	
	List<TileInds> GetNeededTiles(float x, float z) 
	{		
		List<TileInds> inds = new List<TileInds>();
		int row = (int) Mathf.Round(x / kTileSpan) - 1;
		int col = (int) Mathf.Round(z / kTileSpan) - 1;
		
		for (int r = -2; r <= 2; r++) 
		{
			for (int c = -2; c <= 2; c++) 
			{
				inds.Add(new TileInds(row + r, col + c));
			}
		}
		return inds;
	}
	
	GameObject GenerateQuad(TileInds tile)  
	{
		Vector3[] vertices = new Vector3[kNumQuadSideVertices * 2];
    	Vector2[] uvs = new Vector2[kNumQuadSideVertices * 2];
    	int[] triangles = new int[(kNumQuadSideVertices - 1) * 6];
		Vector3[] normals = new Vector3[kNumQuadSideVertices * 2];
		Vector4[] tangents = new Vector4[kNumQuadSideVertices	* 2];
		
		GenerateQuadVertices(tile.X, tile.Y, vertices, uvs, triangles, normals, tangents);
		
		GameObject newQuad = (GameObject) GameObject.Instantiate(meshPrefab);
		
		Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.RecalculateBounds();
		newQuad.GetComponent<MeshFilter>().mesh = mesh;
		
		newQuad.renderer.material = snowMaterial;
		
		// Create a new mesh for the mesh collider
		newQuad.GetComponent<MeshCollider>().sharedMesh = mesh;
		
		//TODO: Fix this for quad logic.
		Vector3 position = new Vector3(tile.X * 100, 0, tile.Y * 100);		
		newQuad.transform.position = position;
		
		for (int i = 0; i < Random.Range(1,10); i++) {
			GameObject newRock = (GameObject) GameObject.Instantiate(rock);
			float z = tile.Y * 100 +  Random.Range(0, 100);
			newRock.transform.position = ( new Vector3(tile.X * 100 + Random.Range(0, 100), -z, z));
			//Debug.Log("New rock " + tile.X.ToString() + " " + tile.Y.ToString() + " " + newRock.transform.position.ToString());
		}
		
		addObstacles(newQuad.transform);
		return newQuad;
	}
	
	
	void addObstacles(Transform transform) 
	{
		// Instantiate several rocks	
	}
	
	void GenerateQuadVertices(int quadX, int quadZ, Vector3[] vertices, Vector2[] uvs,
		int[] triangles, Vector3[] normals, Vector4[] tangents) 
	{
		
		vertices[0] = new Vector3(0, -quadZ * 100, 0);
		vertices[1] = new Vector3(100, -quadZ * 100, 0);
		vertices[2] = new Vector3(0, -(quadZ + 1) * 100, 100);
		vertices[3] = new Vector3(100, -(quadZ + 1) * 100, 100);
		
		for (int j = 0; j < kNumQuadSideVertices; j++) 
		{
			uvs[j] = UVAtIndex(quadX, j);
			uvs[j + kNumQuadSideVertices] = new Vector2(uvs[j].x, uvs[j].y + (1.0f/(float)kNumQuadSideVertices));
		}
		
		TriangulateRow(triangles);
	
		for (int j = 0; j < kNumQuadSideVertices; j++) 
		{
			normals[j] = new Vector3(0, 1, 0);
			normals[j + kNumQuadSideVertices] = new Vector3(0, 1, 0);	
			
			tangents[j] = new Vector4(-1, 0, 0, 1);		
			tangents[j + kNumQuadSideVertices] = new Vector4(-1, 0, 0, 1);
		}	
	}
	
	// TODO: give this a slope
	Vector3 GetVertexForXZ(float x, float z)
	{
		return new Vector3(x, 0, z);	
	}	
	
	void TriangulateRow(int[] triangles) 
	{	
		for (int j = 0; j < kNumQuadSideVertices - 1; j++) 
		{
			int baseTriangleIndex = j * 6;
			//Debug.Log( "BaseTriangleIndex " + baseTriangleIndex.ToString());
			triangles[baseTriangleIndex] = j;
			triangles[baseTriangleIndex + 1] = j + kNumQuadSideVertices;
			triangles[baseTriangleIndex + 2] = j + 1;	

			triangles[baseTriangleIndex + 3] = j + kNumQuadSideVertices;
			triangles[baseTriangleIndex + 4] = j + kNumQuadSideVertices + 1;
			triangles[baseTriangleIndex + 5] = j + 1;		
		}
	}
	
	Vector2 UVAtIndex(int i, int j) 
	{
		float u = ((float)j)/((float)kNumQuadSideVertices - 1);
		float v =  ((float)i)/((float)kNumQuadSideVertices);
		
		v = v - Mathf.Floor(v);
		Vector2 uv = new Vector2(u,v);
		//Debug.Log("UV " + uv.ToString());
		return uv;	
	}
}
