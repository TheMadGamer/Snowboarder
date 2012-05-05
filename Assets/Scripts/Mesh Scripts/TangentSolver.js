/*
Derived from
Lengyel, Eric. ?Computing Tangent Space Basis Vectors for an Arbitrary Mesh?. Terathon Software 3D Graphics Library, 2001.
http://www.terathon.com/code/tangent.html
*/

class TangentSolver
{
	function TangentSolver(theMesh : Mesh)
	{
		var vertexCount : int = theMesh.vertexCount;
		var vertices : Vector3[] = theMesh.vertices;
		var normals : Vector3[] = theMesh.normals;
		var texcoords : Vector2[] = theMesh.uv;
		var triangles = theMesh.triangles;
		var triangleCount = triangles.length/3;
		var tangents = new Vector4[vertexCount];
		var tan1 = new Vector3[vertexCount];
		var tan2 = new Vector3[vertexCount];
		var tri = 0;
		for (var i : int = 0; i < (triangleCount); i++)
		{
			var i1 = triangles[tri];
			var i2 = triangles[tri+1];
			var i3 = triangles[tri+2];
			
			var v1 = vertices[i1];
			var v2 = vertices[i2];
			var v3 = vertices[i3];
			
			var w1 = texcoords[i1];
			var w2 = texcoords[i2];
			var w3 = texcoords[i3];
			
			var x1 = v2.x - v1.x;
			var x2 = v3.x - v1.x;
			var y1 = v2.y - v1.y;
			var y2 = v3.y - v1.y;
			var z1 = v2.z - v1.z;
			var z2 = v3.z - v1.z;
			
			var s1 = w2.x - w1.x;
			var s2 = w3.x - w1.x;
			var t1 = w2.y - w1.y;
			var t2 = w3.y - w1.y;
			
			var r = 1.0 / (s1 * t2 - s2 * t1);
			var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
			
			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;
			
			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
			
			tri += 3;
		}
		
		for (i = 0; i < (vertexCount); i++)
		{
			var n = normals[i];
			var t = tan1[i];
			
			// Gram-Schmidt orthogonalize
			Vector3.OrthoNormalize( n, t );
			
			tangents[i].x  = t.x;
			tangents[i].y  = t.y;
			tangents[i].z  = t.z;
		
			// Calculate handedness
			tangents[i].w = ( Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0 ) ? -1.0 : 1.0;
		}		
		theMesh.tangents = tangents;
	}
}