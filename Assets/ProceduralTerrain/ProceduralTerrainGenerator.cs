using UnityEngine;
using System.Collections;

public class ProceduralTerrainGenerator : MonoBehaviour {

	public Transform CharacterRoot;
	
	public GameObject TerrainPiece;
	
	// terrain bounds
	Transform mTerrains0;
	Transform mTerrains1;
	int mTerrainIndex;
	
	// Use this for initialization
	void Start () {
		//mTerrains = new GameObject[2];
		mTerrains0 = Instantiate(TerrainPiece) as Transform;
		
		mTerrainIndex = 0;
		
		
	}
	
	Vector3 GetNextTilePosition()
	{
		Vector3 position = mTerrains0.position;
		mTerrainIndex = mTerrainIndex % 2;
		
		position += new Vector3(0, -Mathf.Sqrt(2.0f)*10.0f, Mathf.Sqrt(2.0f)*10.0f);
		
		return position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		if(mTerrains1 == null)
		{
			Vector3 nextPosition = GetNextTilePosition();
			mTerrains1 = Instantiate(TerrainPiece) as Transform;
			mTerrains1.position = nextPosition;
		}
	}
}
