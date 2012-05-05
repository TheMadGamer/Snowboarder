using UnityEngine;

using System;
using System.Collections.Generic;
using DeltaCommon.Component;

namespace SnowboardStates
{
	
	
	class FallDownState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
//			Rigidbody[] rigidBodyComponents = mController.MeshTransform.GetComponentsInChildren<Rigidbody>();
//			foreach (Rigidbody body in rigidBodyComponents)
//				body.isKinematic = true;
			mController.ActivateRagdoll();

			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = Color.black;
			}
		}
		
		public override void Update(float dt)
		{
			// let ragdoll drive?
		}
	}
				
	
	
	class GetUpState : SnowboarderState
	{
		public override void Activate(object eventData)
		{			
			base.Activate(eventData);
			
//			Rigidbody[] rigidBodyComponents = mController.MeshTransform.GetComponentsInChildren<Rigidbody>();
//			foreach (Rigidbody body in rigidBodyComponents)
//				body.isKinematic = true;
			mController.DeactivateRagdoll();

			if(mDebugMaterial != null)
			{
				mDebugMaterial.color = Color.black;
			}
		}
		
		public override void Update(float dt)
		{
			
		}
	}
			
	
}