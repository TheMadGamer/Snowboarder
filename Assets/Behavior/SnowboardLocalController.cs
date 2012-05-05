using UnityEngine;
using System.Collections;

public class SnowboardLocalController : SnowboardController 
{
	//private GroundTrail groundTrails;
	private GameObject[] snowParticles;
	
	new void Start()
	{
		base.Start();
		//groundTrails = GetComponentInChildren<GroundTrail>();
		
//		snowParticles = GetComponentInChildren("SnowParticles");
	}
	
	public void Setup()
	{
	}
		
	void _OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) 
	{
		Vector3 velocity = Vector3.zero,  position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		float lean = 0, pivot = 0;
		
	    if (stream.isWriting) 
	    {
    	    velocity = mVelocity;
    	    position = transform.position;
    	    lean = this.mSteeringControl.Lean;

        	stream.Serialize(ref position);
        	stream.Serialize(ref rotation);
        	stream.Serialize(ref velocity);
        	stream.Serialize(ref pivot);
        	stream.Serialize(ref lean);
	    } 
	    else 
	    {
        	stream.Serialize(ref position);
        	stream.Serialize(ref rotation);
        	stream.Serialize(ref velocity);
        	stream.Serialize(ref pivot);
        	stream.Serialize(ref lean);

			transform.position = position;
        	mVelocity = velocity;
        	mSteeringControl.Lean = lean;
	    }    
	}
	
    void OnNetworkInstantiate(NetworkMessageInfo info) 
    {
    	Debug.Log("OnNetworkInstantiate: " + info);

    	// If this object is the local character controller. Attach the camera to this object and enable the local controls script
    	// TODO: run an init script to config things like the trail length and particle renderers
//    	NetworkView networkView = info.networkView;
		if (networkView.isMine)
		{	
			GameObject camera = GameObject.Find("Orbit Camera");
			
			if (camera != null)
			{
				MouseOrbitCameraController cameraController = camera.GetComponent<MouseOrbitCameraController>();
		    	Debug.Log("Attach: " + cameraController);
		    	Transform root = transform.Find("Ragdoll/Root");
		    	if (root)
					cameraController.target = root;
				else
					cameraController.target = transform;
			}
			else
				Debug.LogError("Couldn't find camera");

			GetComponent<SnowboardLocalController>().enabled = true;
			GetComponent<SnowboardRemoteController>().enabled = false;
		}
		else
		{
			GetComponent<SnowboardLocalController>().enabled = false;
			GetComponent<SnowboardRemoteController>().enabled = true;
		}
    }
    	
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
			
		Vector3 dir = Vector3.zero;
        dir.x = -Input.acceleration.y;
        dir.z = Input.acceleration.x;
         
        dir = LowPassFilter(dir);
        
        if (dir.sqrMagnitude > 1)
        {
            dir.Normalize();
        }
        
        Debug.DrawLine(transform.position, transform.position+ dir*10, Color.blue);

       
        
        //transform.Translate(dir * speed);
		
		float acc = dir.x * Time.deltaTime;
		
		Debug.Log("Accel " + acc.ToString());
		
		
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
