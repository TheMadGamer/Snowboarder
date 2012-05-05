using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace DeltaCommon.Component
{

    public class StateMachine<T>
    {

        List<State> mStates = new List<State>();

        Dictionary<State, Dictionary<T, State> > mTransitionTable =
            new Dictionary<State, Dictionary<T, State> >();

        State mActiveState = null;

        // basic event structure
        class StateEvent
        {
            public StateEvent(T id, object data)
            {
                mEventID = id;
                mEventData = data;
            }

            public T mEventID;
            public object mEventData;
        }

        Queue<StateEvent> mEventQueue = new Queue<StateEvent>();


        State mStartState = null;
        public State StartState
        {
            get { return mStartState; }
            set { mStartState = value; }
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void AddState(State state)
        {
            mStates.Add(state);

            mTransitionTable.Add(state, new Dictionary<T, State>());
        }

		// for debug
        public String ActiveStateName()
        {
            return mActiveState.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        public void AddTransition(T eventID, State fromState, State toState)
        {
            mTransitionTable[fromState].Add(eventID, toState);
        }

        
        public void Activate(object eventData)
        {
			Debug.Log("State machine activate");
			
            if (mStartState != null)
            {
                mActiveState = mStartState;
            }
            else if(mStates.Count > 0)
            {
                mActiveState = mStates[0];
            }
            if (mActiveState != null)
            {
                mActiveState.Activate(eventData);
            }
        }

        public void Update(float dt)
        {
            if(mActiveState == null)
            {
                return;
            }

            while (mEventQueue.Count > 0)
            {
                StateEvent stateEvent = mEventQueue.Dequeue();

                // can we transition
                if (mTransitionTable[mActiveState].ContainsKey(stateEvent.mEventID))
                {
					Debug.Log( "[" + Time.time.ToString() + "] Transisiton from " + mActiveState.ToString() + " -> " + mTransitionTable[mActiveState][stateEvent.mEventID].ToString());
					
                    mActiveState.Deactivate();

                    mActiveState = mTransitionTable[mActiveState][stateEvent.mEventID];

                    // dequeue any remaining events, invalid
                    mEventQueue.Clear();

                    mActiveState.Activate(stateEvent.mEventData);

                    // activation may enque new events allowing double transition
                    // do not simply break
                }
                else
                {
                    Debug.LogError("Event not handled: " + stateEvent.mEventID + " .  Active state: " + mActiveState + ".  Event: " + stateEvent.mEventID);
                }
            }


            if (mActiveState != null)
            {
                mActiveState.Update(dt);
            }
        }

        /// <summary>
        /// Something happened, queue for next update
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="eventData"></param>
        public void QueueEvent( T eventID, object eventData)
        {
            mEventQueue.Enqueue(new StateEvent(eventID, eventData));
        }

		public List<T> GetTransitionsForActiveState()
		{
			List<T> transitions = new List<T>();
			if(mActiveState == null)
			{
				Debug.LogError("Active State = null.  Cannot get transitions.");
				return transitions;
			}
			
			Dictionary< T, State> transitionDict = mTransitionTable[mActiveState];
			foreach( T key in transitionDict.Keys)
			{
				transitions.Add(key);
			}
			
			return transitions;
		}
		
    }


    /// <summary>
    /// Methods: Activate, Deactivate, Update
    /// </summary>
    public abstract class State
    {
        public object Parent { get; set; }

        public virtual void Activate(object eventData) { }

        public virtual void Deactivate() { }

        public abstract void Update(float dt);
    }

}
