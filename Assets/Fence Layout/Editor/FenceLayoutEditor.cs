using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(FenceLayout))]

public class FenceLayoutEditor : Editor 
{

	public Texture2D test;
	
	public override void OnInspectorGUI() 
	{
		FenceLayout layout = (FenceLayout) target as FenceLayout;
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Type");
		layout.fenceType = (FenceLayout.FenceType)EditorGUILayout.EnumPopup(layout.fenceType);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Preview Color");
		layout.previewColor = EditorGUILayout.ColorField(layout.previewColor);
		EditorGUILayout.EndHorizontal();
		
		layout.generateOnStart = EditorGUILayout.Toggle("Generate On Start", layout.generateOnStart);
		layout.createColliders = EditorGUILayout.Toggle("Create Colliders", layout.createColliders);

		
		EditorGUILayout.Separator();
		
		switch (layout.fenceType)
		{
			case FenceLayout.FenceType.Mesh:
			layout.meshTransform = EditorGUILayout.ObjectField("Mesh", layout.meshTransform, typeof(Material)) as Transform;
			break;
			
			default:
		
			layout.quadMaterial = EditorGUILayout.ObjectField("Material", layout.quadMaterial, typeof(Material)) as Material;
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Positioning");
			layout.quadPositioning = (FenceLayout.QuadPositioning)EditorGUILayout.EnumPopup(layout.quadPositioning);		
			EditorGUILayout.EndHorizontal();
			
			if (layout.quadPositioning == FenceLayout.QuadPositioning.Displace)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Baseline Position");
				layout.quadDisplaceBaseline = (FenceLayout.QuadDisplaceBaseline)EditorGUILayout.EnumPopup(layout.quadDisplaceBaseline);
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				layout.quadSkewAlignVertical = EditorGUILayout.Toggle("Align to Vertical", layout.quadSkewAlignVertical);
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Tiling");
			layout.quadTiling = (FenceLayout.QuadTiling)EditorGUILayout.EnumPopup(layout.quadTiling);		
			EditorGUILayout.EndHorizontal();
			
			layout.quadHeight = EditorGUILayout.FloatField("Height", layout.quadHeight);
			layout.quadWobbleRange = EditorGUILayout.FloatField("Wobble", layout.quadWobbleRange);
			
			layout.createDoubleSidedQuads = EditorGUILayout.Toggle("Double sided", layout.createDoubleSidedQuads);
			
			break;
		}
		
		EditorGUILayout.Separator();
		
		if (GUILayout.Button("Generate Fences"))
		{
			layout.GenerateFences();
		}
	}
	
	
}
