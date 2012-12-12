using UnityEngine;
using System.Collections;

public class SnowboardLocalController : SnowboardController 
{
	//private GroundTrail groundTrails;
	private GameObject[] snowParticles;

	public void Update()
	{
		UpdateSteering();
		UpdatePosition();	
	}
	
	float LowPassKernelWidthInSeconds = 1.0f;
	// The greater the value of LowPassKernelWidthInSeconds, 
	// the slower the filtered value will converge towards current input sample (and vice versa). 
	// You should be able to use LowPassFilter() function instead of avgSamples(). 
	
	private float AccelerometerUpdateInterval = 1.0f / 60.0f;
		
	private Vector3 lowPassValue = Vector3.zero; // should be initialized with 1st sample
	
	private Vector3 IphoneAcc;
	private Vector3 IphoneDeltaAcc;
	
	///////////////////////
	Vector3 LowPassFilter(Vector3 newSample ) 
	{
		float LowPassFilterFactor = AccelerometerUpdateInterval / LowPassKernelWidthInSeconds; 

		lowPassValue = Vector3.Lerp(lowPassValue, newSample, LowPassFilterFactor);
		return lowPassValue;
	}
	
	// TODO - move the ski blending into SKI state
	public void UpdateSteering()
	{
		// Update lean and pivot values in response to player inputs
		//Debug.Log("Update Steering " + Input.GetKey(KeyCode.RightArrow).ToString());
		Vector3 dir = Vector3.zero;
		float fakeR = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
		float fakeL = Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
        dir.x = -Input.acceleration.y + fakeL + fakeR;
        dir.z = Input.acceleration.x;	
		//Debug.Log("Steer " + fakeR.ToString() + " " + fakeL.ToString());
        dir = LowPassFilter(dir);
        
        if (dir.sqrMagnitude > 1)
        {
            dir.Normalize();
        }
        
        Debug.DrawLine(transform.position, transform.position+ dir*10, Color.blue);

       
        
        //transform.Translate(dir * speed);
		
		float acc = dir.x * Time.deltaTime;
		
		//Debug.Log("Accel " + acc.ToString());
		
		
		if(dir.x < -0.2f)
		{
			mSteeringControl.Lean = -dir.x;
			mSteeringControl.Lean = Mathf.Min(mSteeringControl.Lean, 1.0f);
		}
		else if(dir.x > 0.2f)
		{
			
			mSteeringControl.Lean = -dir.x; //-= 1.0f * mSteeringControl.LeanAdjustmentRate;
			mSteeringControl.Lean = Mathf.Max(mSteeringControl.Lean, -1.0f);
		}
		else
		{
			mSteeringControl.Lean *= 1.0f - Mathf.Clamp(mSteeringControl.LeanDecayRate, 0.0f, 1.0f);
		}
	}
	
}
