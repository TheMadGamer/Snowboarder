using UnityEngine;

using System;
using System.Collections.Generic;
using DeltaCommon.Component;

namespace DebugStates
{
	
	public enum CharacterEvents { 
		To_Idle,		
		To_Ski,
		To_Crouch,
		To_Jump,
		To_InAir,
		To_Landing
	}
	
	
	abstract class SnowboarderState : State
	{
		protected DebugController mController;
		protected GameObject mParent;
		protected CharacterController mCharacterController;
		protected Animation mAnimation = null;	
		
		public override void Activate(object eventData)
		{
			mController = (this.Parent as DebugController);
			mParent = mController.gameObject;
			mCharacterController = (CharacterController)mParent.GetComponent(typeof(CharacterController));
			if (mController.MeshTransform.animation != null)
			{
				mAnimation = mController.MeshTransform.animation;
			}
		}
		
	}
	
	
	class IdleState : SnowboarderState
	{
		public override void Activate(object eventData)
		{
			base.Activate(eventData);

			// play idle
			mAnimation.CrossFade("idle");
			
		}

		public override void Update(float dt)
		{			
			
		}
	}
	
	
	class SkiState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
			mAnimation.CrossFade("forward");
			
			BlendLeans();
		}
		
		private void BlendLeans()
		{
			if(mController.mLean > 0.2f)
			{
				Debug.Log("Lean Right");
				mAnimation.CrossFade("forward_right");
				//mAnimation.Blend("forward_right", mController.mLean);
				//mAnimation.Blend("forward", 1.0f - mController.mLean);
			}
			else if (mController.mLean < -0.2f)
			{
				Debug.Log("Lean Left");
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
			
			BlendLeans();
		}
	}
	
	
	class CrouchState : SnowboarderState
	{

		public override void Activate(object eventData)
		{	

			
			base.Activate(eventData);
			

			mAnimation.CrossFade("crouch");
		}
		
		public override void Update(float dt)
		{
		}
		
	}
	
	
	class JumpState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
			mAnimation.CrossFade("jump");
			//mController.Velocity = new Vector3(mController.Velocity.x, mController.Velocity.y + 10, mController.Velocity.z);
			
		}
		
		public override void Update(float dt)
		{
			
		}
	}
	
	class InAirState : SnowboarderState
	{
		public override void Activate(object eventData)
		{	
			base.Activate(eventData);			
			
			mAnimation.CrossFade("in_air");
		}
		
		public override void Update(float dt)
		{
		}
	}
	
	
	class LandingState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
			mAnimation.CrossFade("landing");
			
		}
		
		public override void Update(float dt)
		{
			
		}
	}
				
		
	
}