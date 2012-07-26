using UnityEngine;
using System.Collections;

public class StatusBarBehavior : MonoBehaviour {
	
	
	// Use this for initialization
	
	static GUIText statusbar;
	
	void Awake() {
		statusbar=GameObject.Find("StatusBar").GetComponent<GUIText>();
		
	}
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static void Display(string textToDisplay) {
		statusbar.text=textToDisplay;	
	}	
}
