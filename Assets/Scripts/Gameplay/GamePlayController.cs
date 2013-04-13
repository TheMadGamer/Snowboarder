using UnityEngine;
using System.Collections;

public class GamePlayController : MonoBehaviour {
	
	public SnowboardController snowboardController;
	
	
	public bool isGameOver() {
		return snowboardController.isRagdoll;
	}
}
