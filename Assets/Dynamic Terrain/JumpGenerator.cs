using UnityEngine;
using System.Collections;

public class JumpGenerator : MonoBehaviour {

	public Transform JumpPrefab;
	
	public Transform CharacterTransform;
	
	int mLayerMask = SnowboardLayers.Character |  
						SnowboardLayers.Details | 
						SnowboardLayers.FallDownTerrain | 
						SnowboardLayers.CharacterFalldownDetect |
						SnowboardLayers.IgnoreRaycast ;	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
		bool hasTouch = false;
		
		// if mouse down, then put down a jump
		foreach (Touch touch in Input.touches)
		{
            if (touch.phase == TouchPhase.Ended )
            {
         		hasTouch = true;   	
            }
		}
            
		if( Input.GetKeyDown(KeyCode.J) || hasTouch )
		{
			if(JumpPrefab != null)
			{
				Transform t = Instantiate(JumpPrefab) as Transform;
				t.position = GetNextJumpPosition();
				
			}
		}
	}
	
	public Vector3 GetNextJumpPosition()
	{
		
		Vector3 binormal = -Vector3.Cross(CharacterTransform.forward, CharacterTransform.up);
		binormal.Normalize();
		
		Vector3 rayCastPoint = CharacterTransform.position + binormal * 10;
		
		RaycastHit hit;
		
		// Try raycast from a large height
		bool didHit = Physics.Raycast(rayCastPoint + Vector3.up * 1000.0f, Vector3.down, out hit, Mathf.Infinity,  mLayerMask);
		if (!didHit)
		{
			Debug.LogError("Raycast failed for jump");
			return rayCastPoint;
		}
		
		return hit.point;
		
	}
	
}
