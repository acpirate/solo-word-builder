using UnityEngine;
using System.Collections;

public class EndGameButtonBehavior : MonoBehaviour {

	// adding player object so i can reference it later
	public PlayerBehavior player;	
	
	
	void OnMouseDown() {
		player.EndGameConfirm();	
	}		
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
