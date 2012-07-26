using UnityEngine;
using System.Collections;

public class ScoreButtonBehavior : MonoBehaviour {
	
	
	// adding player object so i can reference it later
	public PlayerBehavior player;
	
	// button click action
	void OnMouseDown() {
		if (player.workingTiles.Count>0) {
			player.ScoreWord();
		}	
	}	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
