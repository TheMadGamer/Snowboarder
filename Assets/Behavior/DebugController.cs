using UnityEngine;
using System;
using System.Collections.Generic;
using DebugStates;

// debug controller, cycles through anims
using DeltaCommon.Component;

public class DebugController : MonoBehaviour
{
	StateMachine<DebugStates.CharacterEvents> mStateMachine = new StateMachine<DebugStates.CharacterEvents>();
	
	public Transform MeshTransform = null;

	public float mLean = 0.0f;
	
	public void Start()
	{
		InitializeStateMachine();

	}
	
	
	public void OnGUI()
	{
		
		List<DebugStates.CharacterEvents> transitions = mStateMachine.GetTransitionsForActiveState();
		GUIStyle debugStyle = new GUIStyle();
		debugStyle.normal.textColor = Color.red;
		
		int ht = 10;

		// leans: L, R
		if(GUI.Button(new Rect(10, ht, 200, 30), "Lean L" ))
		{
			mLean -= 0.1f;
		}
		ht += 40;
		
		if(GUI.Button(new Rect(10, ht, 200, 30), "Lean R" ))
		{
			mLean += 0.1f;
		}
		ht += 40;
		
		GUI.Label(new Rect(10, ht, 200, 30), "Lean " + mLean, debugStyle);
		ht += 40;
		
		
		foreach( DebugStates.CharacterEvents transition in transitions)
		{
			if(GUI.Button(new Rect(10, ht, 200, 30), "Transition " +transition))
			{
				mStateMachine.QueueEvent(transition, "Transition");
			}
			ht += 40;
		}
		
	}
	
	/// <summary>
	/// Initialize controlling state machine
	/// </summary>
	private void InitializeStateMachine()
	{
		Debug.Log("Init state machine");
		
		// States
		IdleState idleState = new DebugStates.IdleState();
		idleState.Parent = this;
		mStateMachine.AddState(idleState);

		SkiState skiState = new DebugStates.SkiState();
		skiState.Parent = this;
		mStateMachine.AddState(skiState);
		
		CrouchState crouchState = new DebugStates.CrouchState();
		crouchState.Parent = this;
		mStateMachine.AddState(crouchState);

		JumpState jumpState = new DebugStates.JumpState();
		jumpState.Parent = this;
		mStateMachine.AddState(jumpState);
	
		InAirState inAirState = new DebugStates.InAirState();
		inAirState.Parent = this;
		mStateMachine.AddState(inAirState);

		LandingState landState = new DebugStates.LandingState();
		landState.Parent = this;
		mStateMachine.AddState(landState);


		// Transitions

		// Grounded states
		mStateMachine.AddTransition(CharacterEvents.To_Ski, idleState, skiState);
		mStateMachine.AddTransition(CharacterEvents.To_Idle, skiState, idleState);

		// Jump / Land states
		mStateMachine.AddTransition(CharacterEvents.To_Crouch, skiState, crouchState);
		mStateMachine.AddTransition(CharacterEvents.To_Jump, crouchState, jumpState);
		mStateMachine.AddTransition(CharacterEvents.To_InAir, jumpState, inAirState);
		mStateMachine.AddTransition(CharacterEvents.To_Landing, inAirState, landState);
		mStateMachine.AddTransition(CharacterEvents.To_Ski, landState, skiState);

		// Ramp states
		mStateMachine.AddTransition(CharacterEvents.To_InAir, skiState, inAirState);

		// character starts in a spawn in state
		mStateMachine.StartState = idleState;
		
		mStateMachine.Activate(null);

	}

	
	public void Update()
	{
		
		// update state machine
		mStateMachine.Update(Time.deltaTime);	
		
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

	// needed so that unity can send message to this object
	public void HandleEvent(DebugStates.CharacterEvents eventId)
	{
		HandleEvent(eventId, null);
	}
	
	public void HandleEvent(DebugStates.CharacterEvents eventId, object eventData)
	{
		if(eventData != null)
		{
			Debug.Log("Handle Event: " + eventId.ToString() + ". Data: " + eventData);
		}
		else
		{
			Debug.Log("Handle Event: " + eventId.ToString() );
		}

		mStateMachine.QueueEvent(eventId, eventData);
	}
	
}
	
