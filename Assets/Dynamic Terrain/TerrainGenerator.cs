using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is the native terrain height generator.
public class TerrainGenerator : MonoBehaviour {
	//public Terrain terrain;
	public Transform targetObject;
	public Terrain terrainPrefab;
	
	public Dictionary<TileInds, Terrain> TileMap = new Dictionary<TileInds, Terrain> ();
	
	public class TileInds
	{
		private int x;
		private int y;

		public int X { get { return x; } }

		public int Y { get { return y; } }

		public TileInds (int x, int y)
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
	
	void Start ()
	{
		UpdateTiles ();
	}
	
	void UpdateTiles ()
	{
		List<TileInds> inds = 
			GetNeededTiles (targetObject.position.x, targetObject.position.z);
		
		foreach (TileInds tileInds in inds) {
			if (!TileMap.ContainsKey (tileInds)) {
				Debug.Log("Adding tile " + tileInds.ToString());
				Terrain tile = GenerateQuad (tileInds);	
				TileMap.Add (tileInds, tile);
				//addObstacles (tileInds);		
			}
		}	
	}
	
	Terrain GenerateQuad (TileInds tile)
	{
		float tileSizeX = terrainPrefab.terrainData.size.x;
		float tileSizeZ = terrainPrefab.terrainData.size.z;
		
		Terrain newQuad = (Terrain)GameObject.Instantiate (terrainPrefab);
		Vector3 position = new Vector3 (tile.X * tileSizeX, 0, tile.Y * tileSizeZ);		
		GenerateHeights (newQuad,
			tile.X * terrainPrefab.terrainData.heightmapWidth, 
			tile.Y * terrainPrefab.terrainData.heightmapHeight, 
			10, 
			10);

		newQuad.transform.position = position;
		return newQuad;
	}
	
	List<TileInds> GetNeededTiles (float x, float z)
	{		
		float tileSizeX = terrainPrefab.terrainData.size.x;
		float tileSizeZ = terrainPrefab.terrainData.size.z;
		
		List<TileInds> inds = new List<TileInds> ();
		int row = (int)Mathf.Round (x / tileSizeX) - 1;
		int col = (int)Mathf.Round (z / tileSizeZ) - 1;
		
		for (int r = -1; r <= 1; r++) {
			for (int c = -1; c <= 1; c++) {
				inds.Add (new TileInds (row + r, col + c));
			}
		}
		return inds;
	}
	
	public void GenerateHeights (Terrain terrain, float xOffset, float yOffset, float tileXSize, float tileYSize)
	{
		terrain.terrainData = CreateTerrain();
		
		Debug.Log ("Gen heights " + xOffset.ToString () + " " + yOffset.ToString ());
		Debug.Log ("to X " + terrain.terrainData.heightmapWidth.ToString ());
		float[,] heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
 
		for (int i = 0; i < terrain.terrainData.heightmapWidth; i++) {
			for (int k = 0; k < terrain.terrainData.heightmapHeight; k++) {
				float xCoord = ((((float)i) + xOffset) / terrain.terrainData.heightmapWidth) * tileXSize;
				float yCoord = ((((float)k) + yOffset) / terrain.terrainData.heightmapHeight) * tileYSize;
				if (i == 0 && k == 0) {
					//Debug.Log ("    i " + i.ToString () + " k " + k.ToString ());
					Debug.Log ("Perlin for " + xCoord.ToString () + " " + yCoord.ToString ());
				}
				//yCoord / 100.0f; //
				heights [i, k] = Mathf.PerlinNoise (xCoord, yCoord) / 10.0f;
			}
		}
 
		terrain.terrainData.SetHeights (0, 0, heights);
	}
	
	private TerrainData CreateTerrain ()
	{
		TerrainData terrainData = new TerrainData ();
		terrainData.size = terrainPrefab.terrainData.size;	 
		terrainData.heightmapResolution = terrainPrefab.terrainData.heightmapHeight;
		terrainData.baseMapResolution = terrainPrefab.terrainData.baseMapResolution;
		terrainData.SetDetailResolution (terrainPrefab.terrainData.detailResolution, 1);
		return terrainData;
	}

	void Update ()
	{
		UpdateTiles ();
	}
}
