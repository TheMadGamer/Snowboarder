using UnityEngine;

using System;
using System.Collections.Generic;
using DeltaCommon.Component;

namespace SnowboardStates
{
	
	abstract partial class SnowboarderState : State
	{
		
		protected void AirIntegration(float dt)
		{
			// Damp forward velocity
			//mController.Velocity *= mController.VelocityDamping;
			
			// Add in gravitational acceleration
			mController.Velocity += new Vector3(0, mController.mCharacterPhysics.Gravity * dt,0);		
			mController.LastVelocity = mController.Velocity;

			// Integration step
			Vector3 position = mParent.transform.position + mController.Velocity * dt;	

			// Clamp the position to the surface height to prevent tunnelling.
			// TODO: raycast down so that we detect colliders as .  Dont use terrain sample
			RaycastHit hit;
			bool didHit = Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, mController.LayerMask);
			
			if (!didHit)
			{
				Debug.LogWarning("Jumped off the world!");
			}

			// Update the position
			mParent.transform.position = position;
		}
	}
	
	class CrouchState : SnowboarderState
	{
		// pretend to go this far in the anim
		float mTimeout = 1.0f;
		bool mToNextState = false;
		public override void Activate(object eventData)
		{	
			mToNextState = false;
			
			base.Activate(eventData);
			
			mAnimation.CrossFade("crouch");
			
			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = Color.green;
			}
		}
		
		public override void Update(float dt)
		{
			if(mTimeout > 0)
			{
				mTimeout -= dt;
			}
			
			if((!mToNextState) && mController.JumpButtonReleased())
			{
				mToNextState = true;
			}
			
			// the timeout forces the to_jump to be delayed by a timeout
			if(mTimeout <= 0 && mToNextState)
			{
				mController.HandleEvent(  CharacterEvents.To_Jump, "Jump timed out + button released");
			}
			
			// Integration step
			SkiIntegration(dt);
		}
		
	}
	
	
	class JumpState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
			mParent.BroadcastMessage("DidJump",SendMessageOptions.DontRequireReceiver);
			
			mAnimation.CrossFade("jump");
			
			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = new Color(0.25f, 0.75f,0);
			}
			
			mController.Velocity = new Vector3(mController.Velocity.x, mController.Velocity.y + mController.JumpUpwardImpulse, mController.Velocity.z);
			
		}
		
		public override void Update(float dt)
		{
			// Integration step
			AirIntegration(dt);
			
		}
	}
	
	class InAirState : SnowboarderState
	{
		public override void Activate(object eventData)
		{				
			base.Activate(eventData);
			
			mParent.BroadcastMessage("DidJump",SendMessageOptions.DontRequireReceiver);
			
			mAnimation.CrossFade("in_air");
			
			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = new Color(0.75f, 0.25f,0);
			}
		}
		
		public override void Update(float dt)
		{
			if(mController.NearGround)
			{
				mController.HandleEvent(  CharacterEvents.To_Landing, "Air to landing");
			}
			
			AirIntegration(dt);
			
		}
	}
	
	
	class LandingState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
			mAnimation.CrossFade("landing");
			
			mParent.BroadcastMessage("DidLand", SendMessageOptions.DontRequireReceiver);
			
			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = Color.yellow;
			}
		}
		
		public override void Update(float dt)
		{
			if(mController.OnGround)
			{
				mController.HandleEvent(  CharacterEvents.To_Ski, "Landing to ski");
								
				Vector3 groundForward;
				mController.ProjectOntoGroundPlane(mController.SurfaceNormal, 
												   mController.Velocity.normalized, 
												   out groundForward);
				
				mController.Velocity = Vector3.Project(mController.Velocity, groundForward);
				mController.LastVelocity = mController.Velocity;
				
				mController.JustLanded = true;

				Vector3 position = mParent.transform.position;

				// Constrain character to ground					
				RaycastHit hit;

				bool didHit = Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, this.mController.LayerMask);

				float terrainHeight = hit.point.y + mController.mCharacterPhysics.CharacterHeight;
				
				position.y = terrainHeight;
				mParent.transform.position = position;
				mController.LastPosition = position;
				
				SkiIntegration(dt);
			}
			else
			{
				AirIntegration(dt);
			}
		}
	}
}