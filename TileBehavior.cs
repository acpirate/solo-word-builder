using UnityEngine;

using System.Collections;


public class TileBehavior : MonoBehaviour {
	
	// why doesnt the char variable show up in inspector?
	public char glyph;
	public int scoreValue; 
	
	
	//set the shelf so we can reference it later;
	static ShelfBehavior shelf;
	
	// boolean to determine if the tile is placed or not, can't select placed tiles
	public bool placedOnBoard=false;
	// determine if the tile was scored or not, if it was then it can't be moved anymore.
	public bool scored=false;
	
	//store the player for later reference
	static PlayerBehavior player=null;
	
	public char wildLetter;  // stores value of blank tile after it is placed
	
	
	// remeber the previous position on the shelf in case we want to put it back after placing it on the board
	public float oldPostion=0f;
	
	
	
	
	// Use this for initialization
	void Awake () {
		if (!(player)) {
		player=GameObject.Find("Player").GetComponent<PlayerBehavior>(); }
		if (!(shelf)) {
		shelf=	GameObject.Find("Shelf").GetComponent<ShelfBehavior>(); }

		
	}
	
	
	public void ScoreTile() {
		//disable the collider so the tile no longer responds to clicks
		GetComponent<BoxCollider>().enabled=false;
		scored=true;
		transform.parent.GetComponent<BoxCollider>().enabled=false;
	}	
	
	//called by the player to assign a letter to a blank tile after it is placed
	void AssignWildLetter(char inLetter) {
		
	}	
	
	
	
	public void MakeTile(char inGlyph, int inScore) {
		
		// texture reference will be used later to set the texture of the tile, 
		string textureReference=inGlyph.ToString();
		//make sure the blank tile gets the correct texture
		if (textureReference=="*") {
			textureReference="blank";
		}
		
		// set the values of the tile
		glyph=inGlyph;
		scoreValue=inScore;
		
		//create a material for the tile and set it to the correct texture
		transform.renderer.material=new Material(Shader.Find(" Diffuse"));	
		transform.renderer.material.SetTexture("_MainTex",Resources.Load("LetterTileTextures/tile-"+textureReference,typeof(Texture)) as Texture);
		
		//change render queue of shader so it shows up after the space renderer
	}		
	
	//when the tile is clicked on the player picked tile action is called
	void OnMouseDown() {
		if (!(placedOnBoard)) {
		player.PickTile(transform); }
	}	
	
	//stupid unity program developed on macs so you need an alternate method to detect right clicks
	void  OnMouseOver () 
	{	

   		if(Input.GetMouseButtonDown(1) && placedOnBoard && (!(scored))) {
        	placedOnBoard=false;
			// flag the space as not having a tile
		 	transform.parent.gameObject.GetComponent<SpaceBehavior>().hasTile=false;
			
			if (glyph=='*') {
				RevertBlank();	
			}	
			
			player.RemoveClickedTile(transform);
			
		}	

	}
	
	public void RevertBlank() {
		transform.renderer.material.SetTexture("_MainTex",Resources.Load("LetterTileTextures/tile-blank",typeof(Texture)) as Texture);
	}	
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	//highlight a tile after it is selected
	public void Highlight() {
		transform.renderer.material.color=Color.green;	
	}
	
	//unhighlight a tile after it is unselected or placed
	public void Unhighlight() {
		transform.renderer.material.color=Color.white;
	}
	
	//set the wildletter value for scoring, and set the correct mesh so the letter shows up
	public void BlankTileFill(char selectedLetter) {
		
		
		wildLetter=selectedLetter;
		transform.renderer.material.SetTexture("_MainTex",Resources.Load("LetterTileTextures/tile-blank-"+selectedLetter.ToString().ToLower(),typeof(Texture)) as Texture);
		
	}
}
