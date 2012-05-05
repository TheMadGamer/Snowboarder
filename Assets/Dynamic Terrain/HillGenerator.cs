using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ImageTools.Core;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

/*

This script generates mesh for a portion of an infinitely large hill.  The hill is
several layers of noise functions overlayed on a cone.  The hill detail is a generated
by a 2D perlin noise function.  The noise is sloped using an exponential curve and
the number of octaves and scale and sloping for each is user definable.

The mesh is layed out in concentric 'rings' about the center of the mesh regin.
The innermost ring has the highest detail with meshDetail*128 triangles.  For each
outer ring the number of triangles is meshDetail*128/(n*2).  Note that meshDetailLevels
must not be larger than meshDetail/2

The mesh is udpated automatically if the region is set using SetRegion(rect)

*/

public class HillGenerator : MonoBehaviour 
{
	// The target transform - the mesh is recentered around this object every Updatge
	public Transform target;
	
	// The mesh component - either a MeshFilter or MeshCollider
	public Component meshComponent;
	
	// The terrain is slooed along the z-axis
	public float slope = 0.2f;
	
	// Grid size for the render mesh and collision mesh
	public int renderMeshSize = 10;
	public int collisionMeshSize = 10;
	public float meshScale = 1.0f;
	
	// Noise parameters.  This is similar to the notion of octaves used by many noise generators.  In this case
	// though the scale and intensity of the octave is manually assigned, rather than procedural.
	[System.Serializable]
	public class NoiseData
	{
		public float scale;
		public float height;
		public Vector2 position;
		public AnimationCurve profile;

		public float inverseScale;
		public void Setup() { inverseScale = 1.0f / scale; }

		public NoiseData(float scale, float height, Vector2 position, AnimationCurve profile)
		{
			this.scale = scale;
			this.height = height;
			this.position = position;
			this.profile = profile;
		}		
	}
	public NoiseData[] noiseData = {	new NoiseData(5, 3, Vector2.zero, AnimationCurve.EaseInOut(0,0,1,1)),
										new NoiseData(13, 8, Vector2.zero, AnimationCurve.EaseInOut(0,0,1,1)) 	};
	
	// Detail mesh data - holds configuration info for detail mesh instancing.  On start, DetailMeshData.count
	// instances of the game object are instantiated.  The scale, elevation, and spacing are chosen randomly
	// to be within the scaleRange, elevationRange, and spacingRange respectively.
	[System.Serializable]
	public class DetailMeshData
	{
		public GameObject gameObject;
		
		public float scale = 1.0f;
		public float scaleRange = 0.0f;
		
		public float elevation = 0.0f;
		public float elevationRange = 0.0f;
		
		public int count = 1;
		public float spacing = 20.0f;
		public float spacingRange = 10.0f;
	}
	public DetailMeshData[] detailMeshData;
	
	// Instance information for a detail mesh.  Detail instances are  tiled in the x-z plane and positioned 
	// vertically at the height of the terrain.  The position of the detail meshes is updated by calling
	// UpdateDetails().  On start the detail mesh instances are created using the DetailMeshData above.
	private class DetailMeshInstance
	{
		public GameObject gameObject;
		public Vector2 spacing;
		public Vector2 offset;
		public float scale;
		public float scaleRange;
		
		public Vector2 gridPosition;
				
		private Vector3 initialScale;
		
		public DetailMeshInstance(GameObject gameObject, Vector2 spacing, Vector2 offset, float scale, float scaleRange)
		{
			this.gameObject = gameObject;
			this.spacing = spacing;
			this.offset = offset;
			this.scale = scale;
			this.scaleRange = scaleRange;

			initialScale = gameObject.transform.localScale;
		}
		
		public void SetScale(float scale)
		{
			gameObject.transform.localScale = initialScale * scale;
		}
	}
	private List<DetailMeshInstance> detailMeshInstances = new List<DetailMeshInstance>();
	
	// Integer based size 
	[System.Serializable]
	public class ISize
	{
		public int width;
		public int height;
		public int area { get { return width*height; } }
		public ISize(int width, int height) { this.width = width; this.height = height; }
	}
	
	public class IPoint
	{
		public int x;
		public int y;
		public IPoint(int x, int y) { this.x = x; this.y = y; }
	}
	
	// Instance of the perlin noise generator	
	private PerlinNoise perlinNoise;
	
	// Wrapper for the terrain grid mesh.  Provides access to vertices, some convenience
	// methods for updating a Mesh object, and triangle index creation
	private class GridMesh
	{	
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector2[] uv;
		public int[] triangles;
		
		private int _width, _depth;
		private float _halfWidth, _halfDepth;
		public int width { get { return _width; } }
		public int depth { get { return _depth; } }
		public float halfWidth { get { return _halfWidth; } }
		public float halfDepth { get { return _halfDepth; } }
		public Mesh mesh;
		
		public bool recreateMesh;
		
		public delegate Vector3 UpdateFunction(int x, int y, GridMesh mesh);
		public UpdateFunction update;
		public UpdateFunction updateNormal;
		
		public GridMesh(int width, int depth, Mesh mesh)
		{
			this._width = width;
			this._depth = depth;
			this._halfWidth = width/2.0f;
			this._halfDepth = depth/2.0f;
			
			this.mesh = mesh;
						
			int vertexCount = (width) * (depth);
			
			this.vertices = new Vector3[vertexCount];
			this.normals = new Vector3[vertexCount];
			this.uv = new Vector2[vertexCount];
			
			// Create the triangle indeces for a grid
			this.triangles = new int[vertexCount*3*2];
			int triangleIndex = 0;
			for (int x = 0; x < width-1; x++)
			{
				for (int z = 0; z < depth-1; z++)
				{
					this.triangles[triangleIndex++] = (x+0)*depth+(z+0);
					this.triangles[triangleIndex++] = (x+0)*depth+(z+1);
					this.triangles[triangleIndex++] = (x+1)*depth+(z+1);
					
					this.triangles[triangleIndex++] = (x+0)*depth+(z+0);
					this.triangles[triangleIndex++] = (x+1)*depth+(z+1);
					this.triangles[triangleIndex++] = (x+1)*depth+(z+0);
				}
			}
			
			print("Created mesh: " + vertices.Length);
		}
		
		public void Setup()
		{
			for (int x = 0; x < _width; x++)
			{
				for (int z = 0; z < _depth; z++)
				{
					int i1 = x*_depth+z;
					vertices[i1] = update(x, z, this);
				}
			}
		}
		
		public void Shift(IPoint dp)
		{
			if (dp.x < 0)
				ShiftRight(-dp.x);
			else if (dp.x > 0)
				ShiftLeft(dp.x);

			if (dp.y < 0)
				ShiftUp(-dp.y);
			else if (dp.y > 0)
				ShiftDown(dp.y);
		}
		
		public void ShiftLeft(int dx)
		{
			print("Shift left: " + dx);
			for (int y = 0; y < _depth; y++)
			{
				for (int x = 0; x < _width-dx; x++)
				{
					int i0 = (x+1)*_depth + y;
					int i1 = x*_depth + y;
					//print("Shift Left " + i1 + " (" + x + "," + y + ") <-- " + i0 + " (" + (x-1) + "," + y + ") V: " + vertices[i1]);
					vertices[i1].y = vertices[i0].y;
					normals[i1] = normals[i0];
					uv[i1] = uv[i0];
				}
				for (int x = _width-dx; x < _width; x++)
				{
					int i1 = x*_depth+y;
					vertices[i1] = update(x, y, this);
					if (updateNormal != null)
						normals[i1] = updateNormal(x, y, this);
					//print("Update " + i1 + " (" + x + "," + y + ") V: " + vertices[i1]);
				}
			}
		}

		public void ShiftRight(int dx)
		{
			print("Shift right: " + dx);
			for (int y = 0; y < _depth; y++)
			{
				for (int x = _width-1; x >= dx; x--)
				{
					int i0 = (x-1)*_depth+y;
					int i1 = x*_depth+y;
					vertices[i1].y = vertices[i0].y;
					normals[i1] = normals[i0];
					uv[i1] = uv[i0];
				}			
				for (int x = dx-1; x >= 0; x--)
				{
					int i1 = x*_depth+y;
					vertices[i1] = update(x, y, this);
					if (updateNormal != null)
						normals[i1] = updateNormal(x, y, this);
				}
			}
		}
		
		public void ShiftUp(int dy)
		{
			print("Shift up: " + dy);
			for (int x = 0; x < _width; x++)
			{
				for (int y = _depth-1; y >= dy; y--)
				{
					int i0 = x*_depth+(y-1);
					int i1 = x*_depth+y;
					vertices[i1].y = vertices[i0].y;
					normals[i1] = normals[i0];
					uv[i1] = uv[i0];
				}
				for (int y = dy-1; y >= 0; y--)
				{
					int i1 = x*_depth+y;
					vertices[i1] = update(x, y, this);
					if (updateNormal != null)
						normals[i1] = updateNormal(x, y, this);
				}
			}
		}

		public void ShiftDown(int dy)
		{
			print("Shift down: " + dy);
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _depth-dy; y++)
				{
					int i0 = x*_depth+(y+1);
					int i1 = x*_depth+y;
					vertices[i1].y = vertices[i0].y;
					normals[i1] = normals[i0];
					uv[i1] = uv[i0];
				}
				for (int y = _depth-dy; y < _depth; y++)
				{
					int i1 = x*_depth+y;
					vertices[i1] = update(x, y, this);
					if (updateNormal != null)
						normals[i1] = updateNormal(x, y, this);
				}
			}
		}
		
		public void BeginUpdate()
		{
		}
		
		public void FinishedUpdate()
		{
			if (recreateMesh)
				mesh = new Mesh();
			else
				mesh.Clear();
				
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.triangles = triangles;
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}
		
	}
	
	private GridMesh renderMesh;
	private GridMesh collisionMesh;

	// The position of the transform at the last update
	public Vector3 oldPosition;
	public Vector2 oldGridPosition;
	
	
	
	void PrintVerts()
	{
		return;
		
		for (int z = 0; z < renderMesh.depth; z++)
		{
			string str = "";
			for (int x = 0; x < renderMesh.width; x++)
			{
				str += "(" + renderMesh.vertices[x*renderMesh.depth+z] + ") \t";
			}
			print(z + ": " + str);
		}
	}

	void Start()
	{
		// Setup the noise generators
		perlinNoise = new PerlinNoise((int)Time.time);	

		// Create the terrain mesh
		CreateMeshes();
		
		// Instantiate detail objects
		CreateDetails();
	}
	
	void CreateMeshes()
	{
		// Create a new mesh for the mesh filter and a grid mesh to reference it
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.sharedMesh = new Mesh();
		renderMesh = new GridMesh(renderMeshSize, renderMeshSize, meshFilter.sharedMesh);
		//renderMesh.recreateMesh = true;
		renderMesh.update = new GridMesh.UpdateFunction(GridCallback);
		//renderMesh.updateNormal = new GridMesh.UpdateFunction(GridNormalCallback);
		renderMesh.Setup();
		
		// Create a new mesh for the mesh collider and a grid mesh to reference it
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		meshCollider.sharedMesh = new Mesh();
		collisionMesh = new GridMesh(collisionMeshSize, collisionMeshSize, meshCollider.sharedMesh);
		//collisionMesh.recreateMesh = true;
		collisionMesh.update = new GridMesh.UpdateFunction(GridCallback);
		collisionMesh.Setup();
		
		print ("Initialized: ");
		PrintVerts();
	}
	
	void OnDrawGizmosSelected()
	{
		return;
		
		Gizmos.color = Color.green;
		if (renderMesh == null)
			return;
		
		for (int i = 0; i < renderMesh.vertices.Length; i++)
		{
			Gizmos.DrawSphere(renderMesh.vertices[i] + transform.position, 0.1f);
			Gizmos.DrawRay(renderMesh.vertices[i] + transform.position, renderMesh.normals[i] * 2.0f);
		}
	}
	
	void UpdateTerrain()
	{
		// Update the transform position so that it tracks the target position - this seems to fix the
		// dissapearance issue (probably due to Unity thinking the mesh is outside the frustum) 
		Vector3 position = new Vector3(Mathf.Round(target.position.x), 0.0f, Mathf.Round(target.position.z));
		transform.position = position;

		// Generates vertices for the render mesh
		renderMesh.BeginUpdate();
		UpdateTerrainMesh(renderMesh);
		renderMesh.FinishedUpdate();
		//MeshFilter meshFilter = GetComponent<MeshFilter>();
		//meshFilter.sharedMesh = renderMesh.mesh;

		// Generates vertices for the collision mesh
		collisionMesh.BeginUpdate();
		UpdateTerrainMesh(collisionMesh);
		collisionMesh.FinishedUpdate();
		
		MeshCollider meshCollider =  transform.GetComponent<MeshCollider>();
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = collisionMesh.mesh;
		
		// Repositions the detail meshes if necessary 
		UpdateDetails();

		oldPosition = transform.position;
	}

	// TODO: this script should be caching as much data as possible - we can add and remove a single row
	// from the mesh at a time.  We can probably do something with the meshes to make them triangle
	// strips from the get-go, rather than relying on Unity to do it.  One option is to use multiple game objects
	// with this script attached.
	void UpdateTerrainMesh(GridMesh gridMesh)
	{
		// Calculate the number of terrain units the grid has shifted along the x-z plane
		IPoint dp = new IPoint((int)(transform.position.x - oldPosition.x), (int)(transform.position.z - oldPosition.z));
		gridMesh.Shift(dp);
	}
	
	Vector3 GridCallback(int x, int y, GridMesh mesh)
	{
		float wx = (x - mesh.halfWidth) * meshScale;
		float wy = (y - mesh.halfDepth) * meshScale;

		return new Vector3(wx, HillFunction(wx + transform.position.x, wy + transform.position.z), wy);
	}
	
	float HillFunction(float x, float y)
	{		
		float h = -y*slope;
		
		foreach (NoiseData n in noiseData)
		{
			float noise = perlinNoise.Noise((x + n.position.x)*n.inverseScale, (y + n.position.y)*n.inverseScale, 0.0f);
			h += n.height * n.profile.Evaluate(noise);
		}
		
		return h;
	}
	
	Vector3 GridNormalCallback(int x, int z, GridMesh mesh)
	{
		float wx = (x - mesh.halfWidth) * meshScale;
		float wz = (z - mesh.halfDepth) * meshScale;

		return HillNormal(wx + transform.position.x, wz + transform.position.z);
	}
	
	Vector3 HillNormal(float x, float z)
	{		
		// For a surface s = [x, f(x,z), z] the normal is n = cross( dS/dx , dS/dz )
		// So, dS/dx = [x+1, HillFunction(x+1,z), z) - [x-1, HillFunction(x-1,z), z]
		// So, dS/dz = [x, HillFunction(x,z+1), z+1) - [x, HillFunction(x,z-1), z-1]

		Vector3 dSdx = new Vector3( 2.0f, 
									HillFunction(x+1,z) - HillFunction(x-1,z),
									0.0f );
		Vector3 dSdz = new Vector3( 0.0f, 
									HillFunction(x,z+1) - HillFunction(x,z-1),
									2.0f );
									
		Vector3 n = Vector3.Cross(dSdz, dSdx);
		return n.normalized;
	}
	
	
/*
	
	Detail mesh functions - handles the initialization and update for the details objects.
	
*/
		
	void UpdateDetails()
	{
		// Reposition detail meshes as the move off of the grid
		foreach (DetailMeshInstance dmi in detailMeshInstances)
		{
			// Gets the position on a grid 
			Vector2 gridPosition = new Vector2(	Mathf.Floor(transform.position.x / dmi.spacing.x + dmi.offset.x) + 0.5f, 
												Mathf.Floor(transform.position.z / dmi.spacing.y + dmi.offset.y) + 0.5f );
			
			if (!Vector2.Equals(gridPosition, dmi.gridPosition))
			{
				dmi.gridPosition = gridPosition;
				
				// Scales the grid point to world units
				Vector2 scaledGridPosition = new Vector2(	(gridPosition.x-dmi.offset.x)*dmi.spacing.x, 
															(gridPosition.y-dmi.offset.y)*dmi.spacing.y	);

				float elevation = HillFunction(scaledGridPosition.x, scaledGridPosition.y);

				Random.seed = (int)(gridPosition.x * 100.0f + gridPosition.y);
				dmi.SetScale(dmi.scale + Random.value*dmi.scaleRange);

				// Gets the world position for the detail mesh
				Vector3 worldPosition = new Vector3(scaledGridPosition.x, elevation, scaledGridPosition.y);
			
				dmi.gameObject.transform.position = worldPosition;
				dmi.gameObject.transform.eulerAngles = new Vector3(0.0f, Random.value*360.0f, 0.0f);
			}
		}
	}

	void CreateDetails()
	{
		foreach (DetailMeshData dmd in detailMeshData)
		{
			for (int i = 0; i < dmd.count; i++)
			{
				GameObject go = (GameObject)Instantiate(dmd.gameObject, Vector3.zero, Quaternion.identity);
				Vector2 spacing = new Vector2(dmd.spacing*renderMeshSize, dmd.spacing*renderMeshSize) + (Vector2)Random.insideUnitSphere*dmd.spacingRange;
				Vector2 offset = Vector2.Scale(Random.insideUnitCircle, spacing);

				DetailMeshInstance newInstance = new DetailMeshInstance(go, spacing, offset, dmd.scale, dmd.scaleRange);

				detailMeshInstances.Add(newInstance);
			}
		}
	}
	
	void Update()
	{			
		foreach (NoiseData n in noiseData)
			n.Setup();

		if (target)
			UpdateTerrain();
	}
}





