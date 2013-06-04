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
		
		for (int r = -2; r <= 2; r++) {
			for (int c = -2; c <= 2; c++) {
				inds.Add (new TileInds (row + r, col + c));
			}
		}
		return inds;
	}
	
	
	
	// Centers terrain at object.
	/*void CenterTerrain ()
	{
		terrain.transform.position = new Vector3 (targetObject.position.x - terrain.terrainData.size.x / 2.0f, 
			0,
			targetObject.position.z - terrain.terrainData.size.z / 2.0f);
	}
	
	void UpdateTerrainHeight ()
	{		
		float positionOffsetX = targetObject.transform.position.x;
		float positionOffsetY = targetObject.transform.position.z;
		TerrainData terrainData = terrain.terrainData;
		float[,] HeightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
		
		for (int x = 0; x < HeightMap.GetLength(0); x++) {
			for (int y = 0; y < HeightMap.GetLength(1); y++) {
				HeightMap [x, y] = Mathf.Cos (0.01f * (positionOffsetY + x)) * 0.5f + 0.5f;
			}
		}
		
		terrainData.SetHeights (0, 0, HeightMap);
	}*/
	
	void Update ()
	{
		UpdateTiles ();
	}
}
