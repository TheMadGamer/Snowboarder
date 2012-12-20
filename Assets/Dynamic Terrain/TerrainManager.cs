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
		
	const int kNumHorizontalVertices = 11;
	const int kNumVerticalVertices = 11;
	int mLastRowIndex = 0;
	public float MinGenerationDistance = 40;
	
	public Hashtable TileMap;
	
	// Each quad has a row, column that define it's location.
	class Tile {
	    public int row;
		public int column;
		public GameObject mesh;
		
		const float width = 100f;
		const float height = 100f;
		
		public static Tile BuildTile(int row, int column) 
		{
			Tile t = new Tile();
			t.row = row;
			t.column = column;
			
			// TODO build a mesh, move it.
			
			return t;
		}
	}
	
	void Start() 
	{
		UpdateTiles();
    }
	
	void Update () 
	{
		UpdateTiles();
	}

	void UpdateTiles() 
	{
		// If target.transform.position is close to where a tile will be, build the tile.
		Vector3 lastRowVertex =  this.transform.TransformPoint(VertexAtIndex(mLastRowIndex, 0));
		lastRowVertex.x = target.transform.position.x;
		Vector3 distance = (lastRowVertex - target.transform.position);

		// For each tile, if not cached, build.
		List<Tuple<int, int>> neededTiles = GetListOfNeededTiles();
		foreach (Tuple<int, int> tileInds in neededTiles) 
		{
			if (! TileMap.contains(tileInds)) 
			{
				Debug.Log("Adding tile " + tileInds.toString());
				Tile tile = GenerateTile(tileInds.first, tileInds.second);
				TileMap.put(inds, tile);
			}		
		}
		
		// TODO: compute row and column indices from transform position.		
		//if (distance.magnitude < MinGenerationDistance) 
		//{
		//	GenerateStrip(mLastRowIndex++, 0);
		//}

	}
	
	List<Tuple<int, int>> GetListOfNeededTiles(Vector3 position) 
	{
		return null;
	}
	
	void GenerateTile(int row, int column) 
	{
		Vector3[] vertices = new Vector3[kNumHorizontalVertices * 2];
    	Vector2[] uvs = new Vector2[kNumHorizontalVertices * 2];
    	int[] triangles = new int[(kNumHorizontalVertices - 1) * 6];
		Vector3[] normals = new Vector3[kNumHorizontalVertices * 2];
		Vector4[] tangents = new Vector4[kNumHorizontalVertices	* 2];
		
		GenerateQuadVertices(rowIndex, columnIndex, vertices, uvs, triangles, normals, tangents);
		
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
		Vector3 position = transform.forward;
		position *= rowIndex * 10;
		
		newQuad.transform.position = position;
		
		// Parent to this.
		newQuad.transform.parent = transform;

	}
	
	void GenerateQuadVertices(int rowTile, int columnTile, Vector3[] vertices, Vector2[] uvs, int[] triangles, Vector3[] normals, Vector4[] tangents) 
	{
		
		
	}


	void GenerateInitialStrip() 
	{
		for (int i = -(kNumVerticalVertices / 2); i < (kNumVerticalVertices / 2); i++) 
		{
			GenerateStrip(i);		
		}
		mLastRowIndex = (kNumVerticalVertices / 2);
	}
	
	// Generates a strip of vertices, then positions the strip, parents to |transform|.
	void GenerateStrip(int rowIndex, int columnIndex) 
	{
		Vector3[] vertices = new Vector3[kNumHorizontalVertices * 2];
    	Vector2[] uvs = new Vector2[kNumHorizontalVertices * 2];
    	int[] triangles = new int[(kNumHorizontalVertices - 1) * 6];
		Vector3[] normals = new Vector3[kNumHorizontalVertices * 2];
		Vector4[] tangents = new Vector4[kNumHorizontalVertices	* 2];
		
		GenerateVertices(rowIndex, columnIndex, vertices, uvs, triangles, normals, tangents);
		
		GameObject newStrip = (GameObject) GameObject.Instantiate(meshPrefab);
		
		Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.RecalculateBounds();
		newStrip.GetComponent<MeshFilter>().mesh = mesh;
		
		newStrip.renderer.material = snowMaterial;
		
		// Create a new mesh for the mesh collider
		newStrip.GetComponent<MeshCollider>().sharedMesh = mesh;
		
		Vector3 position = transform.forward;
		position *= rowIndex * 10;
		
		newStrip.transform.position = position;
		
		// Parent to this.
		newStrip.transform.parent = transform;
	}	
	
	// Generates a strip of vertices at a given location defined by row index.
	void GenerateStripVertices(int rowTile, int columnTile, Vector3[] vertices, Vector2[] uvs, int[] triangles, Vector3[] normals, Vector4[] tangents) 
	{
		
		for (int j = 0; j < kNumHorizontalVertices; j++) 
		{
			vertices[j] = VertexAtIndex(0, j);
			vertices[j + kNumHorizontalVertices] = VertexAtIndex(1, j);
		}
		
		for (int j = 0; j < kNumHorizontalVertices; j++) 
		{
			uvs[j] = UVAtIndex(rowTile, j);
			uvs[j + kNumHorizontalVertices] = new Vector2(uvs[j].x, uvs[j].y + (1.0f/(float)kNumVerticalVertices));
		}
		
		TriangulateRow(triangles);
	
		for (int j = 0; j < kNumHorizontalVertices; j++) 
		{
			normals[j] = new Vector3(0, 1, 0);
			normals[j + kNumHorizontalVertices] = new Vector3(0, 1, 0);	
			
			tangents[j] = new Vector4(-1, 0, 0, 1);		
			tangents[j + kNumHorizontalVertices] = new Vector4(-1, 0, 0, 1);
		}
	}
	
	void TriangulateRow(int[] triangles) 
	{	
		for (int j = 0; j < kNumHorizontalVertices - 1; j++) 
		{
			int baseTriangleIndex = j * 6;
			//Debug.Log( "BaseTriangleIndex " + baseTriangleIndex.ToString());
			triangles[baseTriangleIndex] = j;
			triangles[baseTriangleIndex + 1] = j + kNumHorizontalVertices;
			triangles[baseTriangleIndex + 2] = j + 1;	

			triangles[baseTriangleIndex + 3] = j + kNumHorizontalVertices;
			triangles[baseTriangleIndex + 4] = j + kNumHorizontalVertices + 1;
			triangles[baseTriangleIndex + 5] = j + 1;		
		}
	}
	
	Vector3 VertexAtIndex(int i, int j) 
	{
		// TODO randomize the y component to get a displacement map
		return new Vector3((j - Mathf.Floor(kNumHorizontalVertices / 2)) * 10.0f, 0, i * 10.0f);
	}
	
	Vector2 UVAtIndex(int i, int j) 
	{
		float u = ((float)j)/((float)kNumHorizontalVertices - 1);
		float v =  ((float)i)/((float)kNumVerticalVertices);
		
		v = v - Mathf.Floor(v);
		Vector2 uv = new Vector2(u,v);
		//Debug.Log("UV " + uv.ToString());
		return uv;	
	}
	
	
	bool ShouldLayoutNextRow() 
	{

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
