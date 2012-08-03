using UnityEngine;
using System.Collections;

public class CursorBehavior : MonoBehaviour {
	
	public Texture2D cursorImage;
	
	private int cursorWidth = 64;
	private int cursorHeight = 64;

	void Start()
	{
		//comment this out to show cursor for debugging purposes
    	Screen.showCursor = false;
	}

	
	void Update() {
		//Debug.Log (Input.mousePosition.x.ToString()+","+Input.mousePosition.y.ToString());
		transform.position=new Vector3(Input.mousePosition.x,Input.mousePosition.y,transform.position.z);
		
	}
	
	void OnGUI()
	{
    	GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, cursorWidth, cursorHeight), cursorImage);
	}
	
	
}
