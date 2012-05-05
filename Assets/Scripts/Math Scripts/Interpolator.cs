using UnityEngine;
using System.Collections;

 // public static class MyExtensions {
   //  public static void MagicDoStuff(this object o) {
 //           ... do magic here ...;
   //     }
 //   }
    
public class Interpolator {

	enum InterpolationType {
		Linear = 0,
		Quadratic = 1,
		Cubic = 2,
		Sinusoidal = 3,
		Exponential = 4,
	};

	enum InterpolationMode
	{
		In = 0,
		Out = 1,
		InOut = 2,
		OutIn = 3,
	};
	
	// Interpolation functionss
	float interpLin(float e1, float e2, float t)
	{
		return e1 + (e2 - e1) * t;
	}

	float interpSin(float e1, float e2, float t)
	{
		return e1 + (e2 - e1) * Mathf.Sin(Mathf.PI / 2.0f * t);
	}

	float interpQuad(float e1, float e2, float t)
	{
		return e1 + (e2 - e1) * t * t;
	}

	float interpCubic(float e1, float e2, float t)
	{
		return e1 + (e2 - e1) * t * t * t;
	}

	float interpExp(float e1, float e2, float t, float tension)
	{
		return e1 + (e2 - e1) * (1.0f - Mathf.Exp(t * tension)) / (1.0f - Mathf.Exp(tension));
	}

	float interp(float e1, float e2, float t, float tension, InterpolationType type, InterpolationMode mode)
	{
		switch ((int)mode)
		{
			case 0:
				return interpIn(e1, e2, t, tension, type);
			case 1:
				return interpOut(e1, e2, t, tension, type);
			case 2:
				return interpInOut(e1, e2, t, tension, type);
			default:
				return interpOutIn(e1, e2, t, tension, type);
		}
	}

	float interpType(float e1, float e2, float t, float tension, InterpolationType type)
	{
		switch (type)
		{
			case InterpolationType.Quadratic:
				return interpQuad(e1, e2, t);
			case InterpolationType.Cubic:
				return interpCubic(e1, e2, t);
			case InterpolationType.Sinusoidal:
				return interpSin(e1, e2, t);
			case InterpolationType.Exponential:
				return interpExp(e1, e2, t, tension);
			default:
				return interpLin(e1, e2, t);
		}
	}	

	float interpIn(float e1, float e2, float t, float tension, InterpolationType type)
	{
		return interpType(e1, e2, t, tension, type);
	}

	float interpOut(float e1, float e2, float t, float tension, InterpolationType type)
	{
		return interpType(e2, e1, 1.0f - t, tension, type);
	}

	float interpInOut(float e1, float e2, float t, float tension, InterpolationType type)
	{
		float e01, e02, t0;
	
		if (t < 0.5f)
		{
			e01 = e1;
			e02 = (e1 + e2) / 2.0f;
			t0 = t*2.0f;
		}
		else
		{
			e01 = e2;
			e02 = (e1 + e2) / 2.0f;
			t0 = 2.0f - t*2.0f;
		}
	
		return interpType(e01, e02, t0, tension, type);
	}

	float interpOutIn(float e1, float e2, float t, float tension, InterpolationType type)
	{
		float e01, e02, t0;
	
		if (t < 0.5f)
		{
			e01 = e1;
			e02 = (e1 + e2) / 2.0f;
			t0 = t*2.0f;
		}
		else
		{
			e01 = e2;
			e02 = (e1 + e2) / 2.0f;
			t0 = 2.0f - t*2.0f;
		}
	
		return interpType(e01, e02, t0, tension, type);
	}	
}
