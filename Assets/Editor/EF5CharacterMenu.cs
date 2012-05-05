// C# example:
using UnityEditor;
using UnityEngine;

class  EF5CharacterMenu : MonoBehaviour {
	
	[MenuItem("EF5/Move Selection To Top Of Hill")]
	static void MoveCharacterToTopOfHill()
	{
		MoveSelectionTo("TopOfHill");
	}
	
	[MenuItem("EF5/Move Selection To Bottom Of Hill")]
	static void MoveCharacterToBottomOfHill()
	{
		MoveSelectionTo("BottomOfHill");
	}
	
	static void MoveSelectionTo(string location)
	{
		Transform toTransform = GameObject.Find(location).transform;
		if(location == null)
		{
			Debug.Log(location + " not found.");
			return;
		}
		
		Selection.activeTransform.position = toTransform.position;
	}
	
}