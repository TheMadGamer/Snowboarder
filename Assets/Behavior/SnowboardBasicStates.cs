using UnityEngine;

using System;
using System.Collections.Generic;
using DeltaCommon.Component;

namespace SnowboardStates
{
	
	public enum CharacterEvents { 
		To_Idle,		
		To_Ski,
		To_Crouch,
		To_Jump,
		To_InAir,
		To_Landing,
		To_FallDown,
		To_GetUp
	}
	
	
	abstract partial class SnowboarderState : State
	{
		protected SnowboardController mController;
		protected GameObject mParent;
		protected CharacterController mCharacterController;
		
		
		protected Material mDebugMaterial = null;
		protected Animation mAnimation = null;		
		
		public override void Activate(object eventData)
		{
			mController = (this.Parent as SnowboardController);
			mParent = mController.gameObject;
			mCharacterController = (CharacterController)mParent.GetComponent(typeof(CharacterController));
			
			if(mController.mDebugMaterial != null)
			{
				mDebugMaterial = mController.mDebugMaterial;
			}
			if (mController.MeshTransform.animation != null)
			{
				mAnimation = mController.MeshTransform.animation;
			}
			
			Rigidbody[] rigidBodyComponents = mController.MeshTransform.GetComponentsInChildren<Rigidbody>();
			foreach (Rigidbody body in rigidBodyComponents)
				body.isKinematic = true;
		}
		
		// moves skiier along slope
		// allows simple turning
		protected void SkiIntegration(float dt)
		{			

			Vector3 velocity = mController.Velocity;
			Vector3 vNorm = velocity.normalized;

			// compute forward  vector along ground
			Vector3 groundForward;
			
			mController.ProjectOntoGroundPlane(mController.SurfaceNormal, 
											mParent.transform.forward, 
											out groundForward);
			
			//get forward dir
			Vector3 characterForward = Vector3.Project(mParent.transform.forward, groundForward);
			
			// force skiier to look forward/down
			// ACL changed this to blend
			mParent.transform.LookAt( mParent.transform.position + 
					mParent.transform.forward * (1.0f - mController.mSteeringControl.DownhillBlendRate * dt) + 
					characterForward * mController.mSteeringControl.DownhillBlendRate * dt );
			
			// project onto gravity force
			// remember that grav is down
			Vector3 accel = Vector3.up*mController.mCharacterPhysics.Gravity;
			accel = Vector3.Project(accel, mParent.transform.forward);
			accel = Vector3.Project(accel, groundForward);
			
			// push character using edge of board
			// drag force
			accel += computeBoardDragForce(velocity, dt);
			
			// scale forward velocity
			velocity += accel  * dt;
			
			// apply a linear drag 
			velocity -= vNorm * mController.mCharacterPhysics.BoardFriction * dt;
			
			// Clamp velocity to terminal speed
			velocity = Vector3.ClampMagnitude(velocity, mController.mCharacterPhysics.TerminalSpeed);

			// Integration step, compute target position
			Vector3 position = mParent.transform.position + velocity * dt;
			
			Vector3 testPoint = position + new Vector3(0,mController.mCharacterPhysics.CharacterHeight,0);
			
			// snap to "terrain height", 
			// first compute hit point with a ray cast
			Vector3 hitPt = new Vector3();
			RaycastHit hit;
			bool didHit = Physics.Raycast(testPoint, Vector3.down, out hit, Mathf.Infinity, mController.LayerMask);
			if(!didHit)
			{
				hitPt = mController.Ground;
			}
			else
			{
				hitPt = hit.point;
			}
			
			float terrainHeight = hitPt.y + mController.mCharacterPhysics.CharacterHeight;
			
			
			// under whatever condition, we transition to in air
			// have not just landed, and either not near ground - off a cliff
			// or above landing height
			// or we've exceeded a vertical jump speed 
			if (!mController.JustLanded && 
				(!mController.NearGround || 
				mParent.transform.position.y > terrainHeight + mController.mCharacterPhysics.LandingHeight || 
				mController.LastVelocity.y > mController.Velocity.y + mController.JumpSpeed))
			{				
				mController.HandleEvent( CharacterEvents.To_InAir, "Ramp Jump");
				mController.Velocity = mController.LastVelocity;
				
				AirIntegration(dt);
			}
			else
			{
				// Clamp the position to the surface height to prevent tunnelling.
				// SNAP and STEP
				position.y = terrainHeight;
				mController.Velocity = velocity;
				mController.LastVelocity = velocity;
				
				if(mController.Integrating )
				{
					mParent.transform.position = position;
					if(mController.StepIntegration)
					{
						mController.Integrating = false;
					}
				}
			}
		}
		
		// computes the force caused by the board holding the skiier on the slope
		Vector3 computeBoardDragForce(Vector3 velocity, float dt)
		{
			
			// this is the sliding direction
			Vector3 projectedVelocity = Vector3.Project(velocity, mParent.transform.forward);
			
			//this is the drag direction
			Vector3 sideVelocity = velocity - projectedVelocity;
			mController.SideVelocity = sideVelocity;
			
			// rotate about norm
			if(mController.mSteeringControl.Lean != 0)
			{
				// pivot
				mParent.transform.RotateAround(mParent.transform.position, mController.SurfaceNormal, -mController.mSteeringControl.Lean * mController.mSteeringControl.PivotRate * dt);
			}
			
			// form an opposing force 
			// * mController.Lean 
			Vector3 opposingForce = -sideVelocity *  mController.mSteeringControl.LeanForceFactor;
			
			// The opposing force should always be opposite of the velocity
			if (Vector3.Dot(velocity, opposingForce) > 0.0f)
			{
				opposingForce = -opposingForce;
			}
			
			return opposingForce;
		}
		
	}
	
	
	class IdleState : SnowboarderState
	{
		public override void Activate(object eventData)
		{
			base.Activate(eventData);

			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = Color.red;
			}
			Debug.Log("Fade to idle");
			mAnimation.CrossFade("idle");
			
			// zero velocity
			mController.Velocity = new Vector3();
		}

		public override void Update(float dt)
		{			
			SkiIntegration(dt);

			// CALEB: changed to min velocity
			if(mController.Velocity.magnitude > mController.mIdleControl.IdleSpeed )
			{
				mController.HandleEvent(  CharacterEvents.To_Ski, "Idle to ski, exceeded velocity");
			}
			else
			{
				mController.DebugString += "Character Velocity < idle speed\n";
			}
			
		}
	}
	
	
	class SkiState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = Color.blue;
			}
				
			BlendLeans();
			
		}
		
		// At the moment, the lean left fails - probably due to the anim being too far apart from the forward
		private void BlendLeans()
		{
			if(mController.mSteeringControl.Lean > 0.2f)
			{
				//Debug.Log("Lean Right");
				mAnimation.CrossFade("forward_right");
				//mAnimation.Blend("forward_right", mController.mLean);
				//mAnimation.Blend("forward", 1.0f - mController.mLean);
			}
			else if (mController.mSteeringControl.Lean < -0.2f)
			{
				//Debug.Log("Lean Left");
				//mAnimation.Blend("forward_left",mController.mLean);
				//mAnimation.Blend("forward", 1.0f - mController.mLean);
				mAnimation.CrossFade("forward_left");
			}
			else
			{
				mAnimation.CrossFade("forward");
			}
			
		}
		
		public override void Update(float dt)
		{
			// Blend anims
			mController.DebugString += "Ski Update";
			BlendLeans();
			

			
			// TODO -- check on ground
			if(mController.OnGround && mController.JumpButtonPushed())
			{
				Debug.Log("JUMP!!");
				mController.HandleEvent(  CharacterEvents.To_Crouch, "Jump button pushed");
			}
			else if(mController.JumpButtonPushed())
			{
				Debug.Log("WANTS TO JUMP!!");
			}			
			// CALEB: changed to velocity check
			else if (mController.Velocity.magnitude < mController.mIdleControl.IdleSpeed)
			{
				mController.HandleEvent(  CharacterEvents.To_Idle, "Slowed down");
				
				// damp out velocity
				mController.Velocity *= (1.0f - mController.mCharacterPhysics.VelocityDamping);
			}

			SkiIntegration(dt);			
			
		}
	}
	
	
	
}