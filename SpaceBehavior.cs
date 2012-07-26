using UnityEngine;
using System.Collections;

public class SpaceBehavior : MonoBehaviour {
	
	//variables which hold the spaces in-game properties, made public so I can view in inspector
	public string type;
	public int x;
	public int y;
	
	//tells the space if it has a tile or not for effects purposes
	public bool hasTile=false;
	
	//reference to player
	static PlayerBehavior player;
		
	// fancy glowy stuff
	ParticleEmitter myParticles;
	Light myLight;
	
	// first function called when space is initialized
	void Awake() {
		if (!(player)) {
		player=GameObject.Find("Player").GetComponent<PlayerBehavior>(); }
		
		// cacheing references to child objects for later use in the mouse effects
		myParticles=transform.Find("DrippingStarsParticles").GetComponent<ParticleEmitter>();
		myLight=transform.Find("Spotlight").GetComponent<Light>();
		
		// setting the renderqueue of the particles above transparent always puts the particles on top
		transform.Find("DrippingStarsParticles").GetComponent<ParticleRenderer>().renderer.material.renderQueue=4000;
	}
	//mouse effects on board
	void OnMouseEnter()
	{	
		if (!(hasTile)) SelectedEffectsOn();	
	}

	void OnMouseExit()
	{	
		if (!(hasTile)) SelectedEffectsOff();	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void SelectedEffectsOn() {
		myParticles.emit=true;
		//myLight.enabled=true;	
	}
	
	void SelectedEffectsOff() {
		myParticles.emit=false;
		//myLight.enabled=false;	
	}
	
	// when the space is clicked the value of the tile which was clicked is passed to the player
	public void OnMouseDown() {
		player.SpaceClick(transform);
		
	}
	
	public void Highlight() {
		myLight.enabled=true;
	}	
	
	public void UnHighlight() {
		myLight.enabled=false;
	}	
	
	/* this function places a tile on the board, the tile is parented with the spaced, scaled, and positioned.  It retains the same rotation as the space.
	 * Then the tile is unhighlited and its placedonboard setting is set to true so that it can't be moved anymore
	 * finally if it is on a special space the space's spotlight is turned on and the color is switched to the color of the underlying space
	 */
	public void PlaceTile(Transform tile) {
		TileBehavior tileBehavior=tile.GetComponent<TileBehavior>();
		
		tile.parent=transform;
		tile.localScale=new Vector3(-.9f,-.9f,.5f);
		tile.localPosition=new Vector3(0f,0f,-.5f);
		tileBehavior.Unhighlight();
		tileBehavior.placedOnBoard=true;
		//turn off selection effects if they are on, turn on spotlight and switch color to special space color		
		hasTile=true;
		SelectedEffectsOff();
	}
}
