using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]

public class MeshFromImagePixels : MonoBehaviour {

	public Texture2D image;
	public float minAlphaComponent = 0.1f;
	public bool calculateNormals;
	
	void Start()
	{
		
		GetComponent<MeshFilter>().mesh = ImageToMesh.CreateMesh(image, minAlphaComponent);
		
	}
}


