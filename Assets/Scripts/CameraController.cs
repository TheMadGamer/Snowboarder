using UnityEngine;
using System.Collections;


public class CameraController : MonoBehaviour {
	
	public Transform target;
	
	public float rotationSpeed = 2.0f;
	public float maxDistance = 10.0f;
	public float minDistance = 6.0f;
	public float maxElevation = 15.0f;
	public float minElevation = -15.0f;
	public float minGroundElevation = 3.0f;
	public float minAngle = -20.0f;
	public float maxAngle = 20.0f;
	public float positionHoldDuration = 5.0f;
	public float positionMoveDuration = 6.0f;

	private float targetAngle;
	private float targetElevation;
	private float targetDistance;
	private Vector3 targetPosition;
	private Vector3 startPosition;
	private float moveTime;
	private float holdTime;

	// Use this for initialization
	void Start () 
	{		
		SetNewTargetPosition();
	}
	
	void SetNewTargetPosition() {
		targetAngle = Random.Range(minAngle, maxAngle) - target.localEulerAngles.y + 0.0f;
		targetElevation = Random.Range(minElevation, maxElevation);
		targetDistance = Random.Range(minDistance, maxDistance);
		
		startPosition = transform.position;

		moveTime = Time.time;
	}

	
	// Update is called once per frame
	void Update () {
		float t = Time.time - moveTime;
		
		targetPosition = new Vector3(	targetDistance*Mathf.Cos((targetAngle)*Mathf.Deg2Rad), 
										targetElevation, 
										targetDistance*Mathf.Sin((targetAngle)*Mathf.Deg2Rad) ) + target.position;

//		targetPosition.y = Mathf.Max(targetPosition.y, Terrain.activeTerrain.SampleHeight(targetPosition) + minGroundElevation);

		if (t < positionMoveDuration)
		{
			transform.position = Vector3.Slerp(startPosition, targetPosition, t / positionMoveDuration);
			
//			if (groundElevation > transform.position.y)
			//{
			//	transform.position = new Vector3( 	transform.position.x,
			//												groundElevation,
			//												transform.position.z);
			//}

			transform.LookAt(target);
		}
		else if (t > positionMoveDuration + positionHoldDuration)
			SetNewTargetPosition();
		else
			transform.position = targetPosition;
	}
}
