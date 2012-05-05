using UnityEngine;
using System;

using SnowboardStates;

using DeltaCommon.Component;

public class SnowboardController : MonoBehaviour
{
	StateMachine<SnowboardStates.CharacterEvents> mStateMachine = new StateMachine<SnowboardStates.CharacterEvents>();
	
	public Material mDebugMaterial = null;	
	public Transform MeshTransform = null;
	
	[System.Serializable]
	public class IdleControl
	{
		// less than this, we go to idle 
		public float MinSkiSlope = -0.075f;
		public float MinSkiVelocity = 0.2f;
		
		public float IdleSpeed = 1.0f;
	}
	public IdleControl mIdleControl = new IdleControl();
	
	// mass and height
	[System.Serializable]
	public class CharacterPhysicsControl
	{
		public float CharacterHeight = 0.75f;
		public float CharacterMass = 20.0f;
		public float LandingHeight = 1.5f;
		public float Gravity = -10.0f;
		
		public float VelocityDamping = 0.99f;
		public float TerminalSpeed = 20;
		
		public float MinHeight = 0.5f;
	
		public float BoardFriction = 0.2f;	
	}
	public CharacterPhysicsControl mCharacterPhysics = new CharacterPhysicsControl();
	
	[System.Serializable]
	public class SteeringControl
	{
		public float Lean = 0;
		public float LeanAdjustmentRate = 1.0f; // how fast the character turns when the key is pressed
		public float LeanForceFactor = 50.0f;	// factor applied to the turning force
		public float LeanDecayRate = 0.3f;		// how fast the character snaps back to neutral
		public float MaxLean = 1.0f;
		public float LeanVisualAngleFactor = 80.0f;
		public float MaxLeanAngle = 50.0f;

		public float PivotRate = 20.0f;
		
		public float TurnRate = 50.0f;
		
		// how fast we blend in the dip to downhill
		public float DownhillBlendRate = 0.1f;
	}

	public SteeringControl mSteeringControl = new SteeringControl();
	
	public float JumpSpeed = 2.5f;
	
	// Used during the frame in which the character landed.  Prevents bouncing due to velocity corrections
	public bool JustLanded = false;	

	public float JumpUpwardImpulse = 5.0f;
	
	// integrated velocity
	protected Vector3 mVelocity = new Vector3();
	public Vector3 Velocity{ get{ return mVelocity;} set{ mVelocity = value;}}
	
	// Actual velocity - calculated by change in position
	protected Vector3 mActualVelocity;
	public Vector3 ActualVelocity{ get{ return mActualVelocity; } }
	
	protected Vector3 mLastVelocity;
	public Vector3 LastVelocity{ get{ return mLastVelocity; } set { mLastVelocity = value; } }
	
	protected Vector3 mLastPosition;	
	public Vector3 LastPosition{ get{ return mLastPosition; } set { mLastPosition = value; } }
	
	public Vector3 SideVelocity = new Vector3();
	
	
	// Surface description
	protected Vector3 mDownhill = new Vector3();
	public Vector3 Downhill{ get{ return mDownhill;} }
	
	protected Vector3 mBinormal = new Vector3();
	public Vector3 Binormal{ get{ return mBinormal; } }
	
	protected Vector3 mSurfaceNormal;
	public Vector3 SurfaceNormal{ get { return mSurfaceNormal;}}
	
	protected Vector3 mGround = new Vector3();
	public Vector3 Ground { get{ return mGround; } } 
		
	protected bool mOnGround = true;
	public bool OnGround{ get{ return mOnGround;}}
	
	protected bool mNearGround = true;
	public bool NearGround{ get{ return mNearGround;}}
	
	// Debug  integration control
	public bool Integrating = true;
	public bool StepIntegration = false;

	protected Vector3 startPosition;
	
	public Transform SnowBurstPrefab = null;

	public string DebugString = "";
	
	private int mLayerMask = 0;
	public int LayerMask { get { return mLayerMask;}}
	
	public void Start()
	{
		
		InitializeStateMachine();
	
		startPosition = transform.position;
		
		mLastPosition = transform.position;
		mVelocity = Vector3.zero;

		// Layer mask used to ignore character and details during ray cast
		mLayerMask = SnowboardLayers.Character |  
						SnowboardLayers.Details | 
						SnowboardLayers.FallDownTerrain | 
						SnowboardLayers.CharacterFalldownDetect |
						SnowboardLayers.IgnoreRaycast ;	
		
		// The index of the character, details, falldown detect, falldown terrain layers
		mLayerMask = ~mLayerMask;

	}
	
	
	public void OnGUI()
	{
		GUIStyle debugStyle = new GUIStyle();
		debugStyle.normal.textColor = Color.red;
		
		GUI.Label(new Rect(10,200,100,20), "Lean " + mSteeringControl.Lean.ToString(), debugStyle);
		if( GUI.Button(new Rect(10,250,150,20), "Toggle Integration", debugStyle ))
		{
			Integrating = !Integrating;
		}
		
		GUI.Label(new Rect(10,270,250,20), "State " + mStateMachine.ActiveStateName(), debugStyle);

		// a simple mechanism for letting the state machine display whatever it wants
		GUI.Label(new Rect(10,350,250,20), DebugString, debugStyle);
		DebugString = "";
		
	}
	
	protected void UpdatePosition()
	{

		// Transform the mesh to simulate leaning left / right
		Vector3 meshAngle = MeshTransform.localEulerAngles;
		meshAngle.x = Mathf.Clamp(mSteeringControl.Lean * mSteeringControl.LeanVisualAngleFactor, -mSteeringControl.MaxLeanAngle, mSteeringControl.MaxLeanAngle);
		MeshTransform.localEulerAngles = meshAngle;
		
		// raycast point, middle of character
		Vector3 rayCastPoint = new Vector3(transform.position.x, transform.position.y + mCharacterPhysics.CharacterHeight, transform.position.z);
		
		RaycastHit hit;

		bool didHit = Physics.Raycast(rayCastPoint, Vector3.down, out hit, Mathf.Infinity, mLayerMask);

		// CALEB: check for tunnelling.  Snap to the surface in case we've tunnelled.  Reset position entirely if all is lost.
		if (!didHit || hit.point.y > transform.position.y + mCharacterPhysics.CharacterHeight)
		{
			Debug.LogWarning("Tunnelled. DidHit: " + didHit);
				
			// Try raycast from a large height
			didHit = Physics.Raycast(rayCastPoint + Vector3.up * 100000.0f, Vector3.down, out hit);
			if (!didHit)
			{
				// Fell off the world
				transform.position = startPosition;
				mActualVelocity = Velocity;
				transform.eulerAngles = Vector3.zero;
			}
			else
			{
				transform.position = hit.point + Vector3.up * mCharacterPhysics.CharacterHeight;
				mLastPosition = transform.position;
				mActualVelocity = Velocity;
			}				
		}
		
		
		mGround = hit.point;
		
		Debug.DrawLine(hit.point+Vector3.up*0.1f, hit.point-Vector3.up*0.1f, Color.red);
		Debug.DrawLine(hit.point+Vector3.right*0.1f, hit.point-Vector3.right*0.1f, Color.red);
		Debug.DrawLine(hit.point+Vector3.forward*0.1f, hit.point-Vector3.forward*0.1f, Color.red);
		
		Vector3 dist = transform.position- mGround;
		
		// for computing jump
		mOnGround = (dist.magnitude <= mCharacterPhysics.CharacterHeight + 0.1f);
		
		// for landing
		mNearGround = (dist.magnitude <= mCharacterPhysics.LandingHeight + 0.1f);
		
		// store surface normal
		mSurfaceNormal = hit.normal;
		
		// this provides a coord frame each step
		ComputeTangentAndBinormal(ref mSurfaceNormal, out mDownhill, out mBinormal);

		// update state machine
		mStateMachine.Update(Time.deltaTime);	
		
		// post statemachine update
		if (JustLanded)
		{
			JustLanded = false;
			
			// re-initialize integration params
			mActualVelocity = mVelocity;
			mLastPosition = transform.position;
		}
		else		
		{
			// CALEB: calculate the actual character velocity (based on change in position) during the last frame 
			mActualVelocity = (transform.position - mLastPosition) / Time.deltaTime;
			mLastPosition = transform.position;
	
			float speed = mVelocity.magnitude;
			
			// Note that this is a modified integration scheme
			mVelocity = mActualVelocity.normalized * speed;
		}
	}
	
	/// <summary>
	/// Initialize controlling state machine
	/// </summary>
	private void InitializeStateMachine()
	{
		Debug.Log("Init state machine");
		
		// States
		IdleState idleState = new IdleState();
		idleState.Parent = this;
		mStateMachine.AddState(idleState);

		SkiState skiState = new SkiState();
		skiState.Parent = this;
		mStateMachine.AddState(skiState);
		
		CrouchState crouchState = new CrouchState();
		crouchState.Parent = this;
		mStateMachine.AddState(crouchState);

		JumpState jumpState = new JumpState();
		jumpState.Parent = this;
		mStateMachine.AddState(jumpState);
	
		InAirState inAirState = new InAirState();
		inAirState.Parent = this;
		mStateMachine.AddState(inAirState);

		LandingState landState = new LandingState();
		landState.Parent = this;
		mStateMachine.AddState(landState);

		FallDownState fallDownState = new FallDownState();
		fallDownState.Parent = this;
		mStateMachine.AddState(fallDownState);
		// Transitions ///////////////////

		// Grounded states
		mStateMachine.AddTransition( CharacterEvents.To_Ski, idleState, skiState);
		mStateMachine.AddTransition( CharacterEvents.To_Idle, skiState, idleState);

		// Jump / Land states
		mStateMachine.AddTransition( CharacterEvents.To_Crouch, skiState, crouchState);
		mStateMachine.AddTransition( CharacterEvents.To_Jump, crouchState, jumpState);
		mStateMachine.AddTransition( CharacterEvents.To_InAir, jumpState, inAirState);
		mStateMachine.AddTransition( CharacterEvents.To_Landing, inAirState, landState);
		mStateMachine.AddTransition( CharacterEvents.To_Ski, landState, skiState);

		// Ramp states
		mStateMachine.AddTransition( CharacterEvents.To_InAir, skiState, inAirState);

		
		// for initialization
		mStateMachine.AddTransition( CharacterEvents.To_InAir, idleState, inAirState);

		// for fall down
		mStateMachine.AddTransition( CharacterEvents.To_FallDown, idleState, fallDownState);
		mStateMachine.AddTransition( CharacterEvents.To_FallDown, skiState, fallDownState);
		mStateMachine.AddTransition( CharacterEvents.To_FallDown, crouchState, fallDownState);
		
		// character starts in a spawn in state
		mStateMachine.StartState = idleState;
		
		mStateMachine.Activate(null);
		
		DeactivateRagdoll();
	}
	
	public void DeactivateRagdoll()
	{
		// TODO: Need to adjust the ragdoll and parent transform here so that the ragdoll position is correct relative to parent transform
		
		Debug.Log("DeactivateRagdoll");
		Rigidbody[] rigidBodies  = gameObject.GetComponentsInChildren<Rigidbody>();
		MeshTransform.animation.enabled = true;
		foreach (Rigidbody body in rigidBodies) {
			body.isKinematic= true;
//			body.useGravity = false;
			
		}
	}

	public void ActivateRagdoll()
	{
		Debug.Log("ActivateRagdoll");
		Rigidbody[] rigidBodies  = gameObject.GetComponentsInChildren<Rigidbody>();
		MeshTransform.animation.enabled = false;
		foreach (Rigidbody body in rigidBodies) {
			body.isKinematic= false;
			body.velocity = mActualVelocity;
		}
	}
	
	public float ProjectedGravity()
	{
		return Vector3.Dot(Vector3.down*mCharacterPhysics.Gravity, mDownhill);
	}
	
	// this computes the projection of a forward vector onto a plane defined by a normal
	public void ProjectOntoGroundPlane(Vector3 normal, Vector3 direction, out Vector3 floorVector)
	{
		
		// degenerate case (direction is perpendicular to surface)
		if( Vector3.Dot(normal, direction) > 0.99f)
		{
			floorVector = mDownhill;
			return;	
		}
		
		direction.Normalize();
		
		Vector3 binormal = Vector3.Cross(normal, direction);
		binormal.Normalize();
		
		// project normal onto groundplane
		floorVector = Vector3.Cross(binormal, normal);
		floorVector.Normalize();
		
		// if going uphill, flip
//		if(floorVector.y > 0)
//		{
//			floorVector = -floorVector;
//		}
		
	}

	// this computes the downhill  (tangent) and binormal vectors, given a plane defined by a normal
	public void ComputeTangentAndBinormal(ref Vector3 normal, out Vector3 tangent, out Vector3 binormal)
	{
		// flat degenerate case .. changed to 0.999 from 0.99 .. this wasn't precise enough and caused flipping back & forth
		if( Vector3.Dot(normal, Vector3.up) > 0.999f)
		{
			tangent = new Vector3(1,0,0);
			binormal = new Vector3(0,0,1);
			return;	
		}
		
		// project normal onto xz plane
		Vector3 projection = new Vector3(normal.x, 0, normal.z);
		projection.Normalize();
		
		// check if we're hitting something vertical
//		if( Vector3.Dot(projection, normal) < 0.95f )
//		{
//			Debug.Log("Hit wall");
			
			binormal = Vector3.Cross(normal, projection);
			binormal.Normalize();
			
			tangent = -Vector3.Cross(normal, binormal);
			tangent.Normalize();
//		}
//		else
//		{
//			// not downhill component (in air)
//			tangent = new Vector3(0,-1,0);
			
//			binormal = Vector3.Cross(tangent, normal);
//			binormal.Normalize();
//		}
	}
	
	public bool JumpButtonPushed()
	{
		return Input.GetKeyDown(KeyCode.Space);
	}

	public bool JumpButtonReleased()
	{
		return Input.GetKeyUp(KeyCode.Space);
	}
	
	public bool ForwardButtonPushed()
	{
		return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
	}
	
	public float GetTurnAngle()
	{
		float turnAngle = 0;
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			turnAngle = -1.0f;
		}
		else if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			turnAngle =  1.0f;			
		}
		
		return turnAngle;
	}

	// this is needed so that HandleEvent can receive a Unity Message
	public void HandleEvent(SnowboardStates.CharacterEvents eventId)
	{
		HandleEvent(eventId, null);
	}
	
	public void HandleEvent(SnowboardStates.CharacterEvents eventId, object eventData)
	{
		if(eventData != null)
		{
		//	Debug.Log("Handle Event: " + eventId.ToString() + ". Data: " + eventData + " Time: " + Time.time);
		}
		else
		{
		//	Debug.Log("Handle Event: " + eventId.ToString() + " Time: " + Time.time);
		}
		
		mStateMachine.QueueEvent(eventId, eventData);
	}
	
	void CreateLandingSprayEmitter()
	{
		// TODO - removed sray burst - JS that used terrain
		/*if( SnowBurstPrefab == null)
		{
			Debug.LogError ("SnowBurstPrefab not assigned");
			return;
		}
		Transform snowBurst = (Transform)Instantiate(SnowBurstPrefab, transform.position, Quaternion.identity);
				
		snowBurst.transform.rotation = transform.rotation;
		SnowBurstController snowBurstController = snowBurst.GetComponent<SnowBurstController>(); 
		snowBurstController.SetEmitterVelocity(mVelocity);*/
	}
	
	void DidLand()
	{
		//CreateLandingSprayEmitter();
	}
}
	
