using UnityEngine;
using System.Collections;

// This is the native terrain height generator.
public class TerrainGenerator : MonoBehaviour {
	public Terrain terrain;
	public Transform targetObject;
	float[,] HeightMap;
	
	void Start ()
	{
		// Initialize height map buffer.
		TerrainData terrainData = terrain.terrainData;
		HeightMap = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
		CenterTerrain ();
		UpdateTerrainHeight ();
	}
	
	// Centers terrain at object.
	void CenterTerrain ()
	{
		terrain.transform.position = new Vector3 (targetObject.position.x - terrain.terrainData.size.x / 2.0f, 
			0,
			targetObject.position.z - terrain.terrainData.size.z / 2.0f);
	}
	
	void UpdateTerrainHeight ()
	{		
		float positionOffsetX = targetObject.transform.position.x;
		float positionOffsetY = targetObject.transform.position.z;
		
		for (int x = 0; x < HeightMap.GetLength(0); x++) {
			for (int y = 0; y < HeightMap.GetLength(1); y++) {
				HeightMap [x, y] = Mathf.Cos ( 0.01f * (positionOffsetX + x)) * 0.5f + 0.5f;
				if (x == 0 && y == 0) {
					Debug.Log("Height " + HeightMap[x, y].ToString());
				}
			}
		}
		
		TerrainData terrainData = terrain.terrainData;
		terrainData.SetHeights (0, 0, HeightMap);
	}
	
	// Update is called once per frame
	void Update ()
	{
		CenterTerrain ();
		UpdateTerrainHeight ();
	}
}
