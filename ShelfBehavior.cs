using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ShelfBehavior : MonoBehaviour {
	
	// bag reference
	public BagBehavior bag;
	
	// player reference
	public PlayerBehavior player;
	
	//shelf space limit
	public static int maxTiles = 7;
	
	//placeholder units to fit tiles on shelf
	static float shelfSpaceUnit = (float)1.0/ (float)maxTiles;
	static int shelfCenter=(int)((maxTiles+1)*.5);
	
	
	// Use this for initialization
	void Awake () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		
	//take the passed in tile and add it to the shelf, scale and position the tile on the shelf
	public void AddTile (Transform tile) {
		
		//storage for the number of tiles on the shelf
		int shelfTileAmount=0;
		//parent the tile to the shelf
		tile.parent=transform;
		
		shelfTileAmount=transform.childCount;
		
		//enable renderer of the tile we drew;
		tile.renderer.enabled=true;
		
		
		//position and scale the tile - scale y value is negative so the texture is flipped in the correct direction
		tile.localScale=new Vector3(shelfSpaceUnit*.8f*-1f,-.8f,.5f);
		tile.rotation=transform.rotation;
		tile.localPosition=new Vector3((shelfTileAmount-shelfCenter)*shelfSpaceUnit,0,-2);	
		tile.GetComponent<BoxCollider>().size=new Vector3(1,1,1);
		//set the old position so that if the tile is returned from the board it goes back to the right spot
		tile.GetComponent<TileBehavior>().oldPostion=(shelfTileAmount-shelfCenter)*shelfSpaceUnit;

	}
	
	
	// put the tile back after it is on the board and right clicked
	public void PutBackTile (Transform tile) {
		
		//flag the tile's parent space as not having a tile
		tile.parent.GetComponent<SpaceBehavior>().hasTile=false;
		
		
		tile.parent=transform;
		tile.localScale=new Vector3(shelfSpaceUnit*.8f*-1f,-.8f,.5f);
		tile.rotation=transform.rotation;
		tile.localPosition=new Vector3(tile.GetComponent<TileBehavior>().oldPostion,0,-2);
		tile.GetComponent<BoxCollider>().size=new Vector3(1,1,1);
	}
	
	//called by the player after a word is scored
	public void FillTiles() {
		
		CompactTiles();
		
		
		for (int i = transform.childCount;i<maxTiles;i++) {
			AddTile(bag.Draw());
		}
		
	}
		
	//moves the tiles down the shelf so new tiles can be added
	void CompactTiles() {
		List<Transform> tempTiles=new List<Transform>();
		
		foreach(Transform tile in transform) {
			tempTiles.Add(tile);
		}	
		
		foreach(Transform tile in tempTiles) {
			tile.parent=null;
		}	
		
		foreach(Transform tile in tempTiles) {	
			AddTile(tile);	
		}
		
	}
	
	// called when the player presses a key to determine which tile should be selected
	public void KeyboardSelect(char selectedLetter) {
		foreach(Transform tile in transform) {
			if (tile.GetComponent<TileBehavior>().glyph==selectedLetter) {
				player.PickTile(tile);
				break;
			}
		}
	}
	
	
	
}
