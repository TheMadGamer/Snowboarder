using UnityEngine;
using System.Collections;

public class FenceLayout : MonoBehaviour 
{

	public Color previewColor = Color.green;
	public bool generateOnStart = false;

	public enum FenceType{Mesh, Quad};
	public FenceType fenceType = FenceType.Quad;

	public Material quadMaterial;
	public enum QuadPositioning{Displace, Skew};
	public QuadPositioning quadPositioning = QuadPositioning.Skew;
	public enum QuadDisplaceBaseline{Average, Min, Max};
	public QuadDisplaceBaseline quadDisplaceBaseline = QuadDisplaceBaseline.Average;
	public bool quadSkewAlignVertical = true;
	public bool createDoubleSidedQuads = true;
	public float quadWobbleRange = 0.0f;
	
	public enum QuadTiling{Material, Quads, None};
	public QuadTiling quadTiling = QuadTiling.Material;
	public bool tileMaterial = true;
	public float quadHeight = 1.5f;
	public bool createColliders = true;
	public bool createRigidBodies = true;

	public Transform meshTransform;
	public enum MeshPositioning{Displace, pivot};
	public MeshPositioning meshPositioning;
	public enum MeshTiling{Center, Tile}
	public MeshTiling meshTiling;

	void Start()
	{
		if (generateOnStart)
			GenerateFences();
	}

	void OnDrawGizmos () 
	{
    	
    	Gizmos.color = previewColor;

    	// Display the explosion radius when selected
    	Transform tPrevious = null;

    	foreach (Transform t in transform)
    	{
	    	Gizmos.DrawWireSphere (t.position, 0.25f);

	    	if (tPrevious)
	    		Gizmos.DrawLine(t.position, tPrevious.position);

	    	tPrevious = t;
    	}
	}
	
	public void GenerateFences()
	{
    	// Display the explosion radius when selected
		
		GameObject fences = new GameObject();
		MeshFilter meshFilter = (MeshFilter)fences.AddComponent("MeshFilter");
		MeshRenderer meshRenderer = (MeshRenderer)fences.AddComponent("MeshRenderer");
				
		switch (quadPositioning)
		{
			case QuadPositioning.Skew:
				meshFilter.mesh = GenerateTriangleStripFence();
				break;
			default:
				meshFilter.mesh = GenerateOffsetQuadFence();
				break;
		}
		    	
    	meshRenderer.material = quadMaterial;
    
   		if (fenceType == FenceType.Quad)
   		{
   			
   			fences.AddComponent<MeshCollider>();
   			fences.AddComponent<Rigidbody>();
   			
   			fences.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
   			fences.rigidbody.isKinematic = true;
   			fences.layer = gameObject.layer;
   		}

	}
	
	Mesh GenerateOffsetQuadFence()
	{
		int triangleCount = 2 * (transform.childCount-1);
		int vertexCount = 3 * triangleCount;

		if (createDoubleSidedQuads)
			triangleCount *= 2;
		
		Vector3 height = Vector3.up*quadHeight;
		
		Vector3[] vertices = new Vector3[vertexCount];
		Vector2[] uv = new Vector2[vertexCount];
		int[] triangles = new int[vertexCount];
		 
		Vector3 v1, v2;
		int vertexIndex = 0;
		int uvIndex = 0;
		int triangleIndex = 0;
		
      	Transform t2 = null;
	  	foreach (Transform t1 in transform)
    	{
	    	if (t2)
	    	{
	    		v1 = t1.position;
	    		v2 = t2.position;
	    		
	    		// Average
	    		switch (quadDisplaceBaseline)
	    		{
	    			case QuadDisplaceBaseline.Average:
			    		v1.y = v2.y = (v1.y+v2.y)/2.0f;
			    		break;
	    			case QuadDisplaceBaseline.Min:
			    		v1.y = v2.y = Mathf.Min(v1.y, v2.y);
			    		break;
	    			case QuadDisplaceBaseline.Max:
			    		v1.y = v2.y = Mathf.Max(v1.y, v2.y);
			    		break;
	    		}

				// l is used to calculate the repeated ( greater than 1 ) texture coord for tiled texture mapping
	    		float l = (tileMaterial) ? Vector3.Distance(v1, v2) : 1;
	    		
	    		vertices[vertexIndex++] = v1;
	    		vertices[vertexIndex++] = v2;
	    		vertices[vertexIndex++] = v2 + height;

	    		vertices[vertexIndex++] = v1;
	    		vertices[vertexIndex++] = v2 + height;
	    		vertices[vertexIndex++] = v1 + height;
	    		
	    		uv[uvIndex++] = new Vector2(0,0);
	    		uv[uvIndex++] = new Vector2(l,0);
	    		uv[uvIndex++] = new Vector2(l,1);

	    		uv[uvIndex++] = new Vector2(0,0);
	    		uv[uvIndex++] = new Vector2(l,1);
	    		uv[uvIndex++] = new Vector2(0,1);
	    		
	    		triangles[triangleIndex++] = vertexIndex-6;
	    		triangles[triangleIndex++] = vertexIndex-5;
	    		triangles[triangleIndex++] = vertexIndex-4;

	    		triangles[triangleIndex++] = vertexIndex-3;
	    		triangles[triangleIndex++] = vertexIndex-2;
	    		triangles[triangleIndex++] = vertexIndex-1;
	    	}

	    	t2 = t1;
    	}
    	
    	Mesh mesh = new Mesh();
    	mesh.vertices = vertices;	
    	mesh.triangles = triangles;	
    	mesh.uv = uv;
    	
    	mesh.RecalculateNormals();
    	mesh.RecalculateBounds();
    	mesh.Optimize();
    	
    	return mesh;
	}
	
	Mesh GenerateTriangleStripFence()
	{
		// Create an array of child transforms
		Transform[] children = new Transform[transform.childCount];
		int index = 0;
		foreach (Transform t in transform) children[index++] = t;
		
		int triangleCount = 2 * (transform.childCount-1);
		int vertexCount = 4 * (transform.childCount-1);

		if (createDoubleSidedQuads)
		{
			triangleCount *= 2;
			vertexCount *= 2;
		}
		
		Vector3 height = Vector3.up*quadHeight;
		
		// Variables used to iterate through child transforms
		Vector3[] vertices = new Vector3[vertexCount];
		Vector2[] uv = new Vector2[vertexCount];
		int[] triangles = new int[triangleCount*3];
		
		// Calculate the 'face' normals aligned to the y-axis
		Vector3[] normals = new Vector3[transform.childCount];
		for (int i = 0; i < children.Length-1; i++)
		{
			Vector3 p1 = children[i].position;
			Vector3 p2 = children[i+1].position;
			
			Vector3 d = p1-p2;
			Vector3 c = Vector3.Cross(d, Vector3.up);
			Vector3 n = Vector3.Cross(c, d);
			n = n.normalized;
			
			normals[i] = n;
			if (i > 1 && i < children.Length-2)
			{
				normals[i-1] = (normals[i-1] + normals[i]) / 2.0f;
			}
			if (i == children.Length-2)
				normals[i+1] = n;
		}
		 
		Vector3 v1, v2;
		Vector3 h1 = Vector3.zero, h2 = Vector3.zero;
		int vertexIndex = 0;
		int uvIndex = 0;
		int triangleIndex = 0;
		

		
		Transform t1, t2;
	  	for (int i = 1; i < children.Length; i++)
    	{
    		t1 = children[i];
    		t2 = children[i-1];

	   		v1 = t1.position;
	   		v2 = t2.position;
	    		
	   		float l = Vector3.Distance(v1, v2);
	   		
			if (quadSkewAlignVertical)
			{
				h1 = Vector3.up * quadHeight;
				h2 = Vector3.up * quadHeight;
			}	
		   	else
	   		{
	   			h1 = quadHeight*normals[i];
	    		h2 = quadHeight*normals[i-1];
	   		}
	   		if (quadWobbleRange != 0.0f)
	   		{
	   			Vector3 d1 = v1 - h1;
	   			Vector3 d2 = v2 - h2;
	   			
	   			// Cardinal basis vector ... used to calculate perpendicular vector
	   			Vector3 c1 = (d1.x < d1.y) ? Vector3.right : (d1.y < d1.z) ? Vector3.up : Vector3.forward;
	   			Vector3 c2 = (d2.x < d2.y) ? Vector3.right : (d2.y < d2.z) ? Vector3.up : Vector3.forward;
	   			
	   			Vector3 p1 = Vector3.Cross(d1, c1).normalized;
	   			Vector3 p2 = Vector3.Cross(d2, c2).normalized;
	   			
	   			Random.seed = i + 15;
	   			float r1 = (Random.value - 0.5f)*2.0f;
	   			h1 += p1 * r1;
	   			Random.seed = i - 1 + 15;
	   			float r2 = (Random.value - 0.5f)*2.0f;
	   			h2 += p2 * r2;
	   			Debug.Log("R1: " + r1 + " R2: " + r2);
	   		}

	   		vertices[vertexIndex++] = v1;
	   		vertices[vertexIndex++] = v2;
    		vertices[vertexIndex++] = v1 + h1;
    		vertices[vertexIndex++] = v2 + h2;
	    		
	   		uv[uvIndex++] = new Vector2(0,0);
	   		uv[uvIndex++] = new Vector2(l,0);
	   		uv[uvIndex++] = new Vector2(0,1);
	   		uv[uvIndex++] = new Vector2(l,1);
	    		
	   		triangles[triangleIndex++] = vertexIndex-4;
	   		triangles[triangleIndex++] = vertexIndex-3;
	   		triangles[triangleIndex++] = vertexIndex-1;

    		triangles[triangleIndex++] = vertexIndex-4;
    		triangles[triangleIndex++] = vertexIndex-1;
    		triangles[triangleIndex++] = vertexIndex-2;
    		
    		if (createDoubleSidedQuads)
    		{
    			vertices[vertexIndex++] = v1;
	   			vertices[vertexIndex++] = v2;
    			vertices[vertexIndex++] = v1 + h1;
	    		vertices[vertexIndex++] = v2 + h2;
	    		
	   			uv[uvIndex++] = new Vector2(0,0);
	   			uv[uvIndex++] = new Vector2(l,0);
		   		uv[uvIndex++] = new Vector2(0,1);
		   		uv[uvIndex++] = new Vector2(l,1);
	   		
		   		triangles[triangleIndex++] = vertexIndex-1;
		   		triangles[triangleIndex++] = vertexIndex-3;
	   			triangles[triangleIndex++] = vertexIndex-4;

	    		triangles[triangleIndex++] = vertexIndex-2;
    			triangles[triangleIndex++] = vertexIndex-1;
    			triangles[triangleIndex++] = vertexIndex-4;
    		}
    	}
    	
    	Mesh mesh = new Mesh();
    	mesh.vertices = vertices;	
    	mesh.triangles = triangles;	
    	mesh.uv = uv;
    	
    	mesh.RecalculateNormals();
    	mesh.RecalculateBounds();
    	mesh.Optimize();
    	
    	return mesh;
	}
	
}
