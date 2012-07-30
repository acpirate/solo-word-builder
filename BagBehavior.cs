using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class BagBehavior : MonoBehaviour {
	
	
	//indicator if bag is empty, used by the player to determine if he can draw or not
	public bool isEmpty = false;
	
	//configure rule link from heirarchy
	public GameRules rules;
	
	// configure tile prefab from heirarchy so I don't have to use to use resources.load
	public GameObject tilePrefab;
	
	// reference to shelf where the tiles will be placed
	public ShelfBehavior shelf;
	
	// Use this for initialization
	void Awake () {
		/* fill bag with tiles
		 * we are going to ask the game rules object how many letters there are and iterate over each line in the rules file
		 * in an inner loop we are going to parse the rules file and add the correct number of tiles matching the letter count
		 * in addtion we are going to add glyph and score information to each tile so that each tile can be looked at for its own information
		 * finally we are going to parent each tile to the bag effectively "filling" the bag with the correct number of tiles
		 */
		//store the glyph amount from the tile data file lines
		int glyphAmount=0;
		int score=0;
		
		//temporary object which stores a tile so I can edit its properties, 
		GameObject tempTile;
		
		//storage for each line of the tile data file as it is read in
		String tileDataLine;
		
		//open the tile data file
		FileInfo tileData = new FileInfo ("Assets\\Resources\\Rules\\solo-word-builder-tiles.txt");
		StreamReader tileDataReader = tileData.OpenText();
		
		// outer loop 
		for (int letterCounter=0;letterCounter<rules.totalLetters;letterCounter++) {
			//read in one line of the data file
			tileDataLine=tileDataReader.ReadLine();
			
			
			
			//take the 2nd and 3rd characters in the tile data line, turn them into an integer and assign it to glyphamount
			int.TryParse(tileDataLine[1].ToString()+tileDataLine[2].ToString(),out glyphAmount);
						
			//inner loop
				for(int glyphAmountCounter=0;glyphAmountCounter<glyphAmount;glyphAmountCounter++) {
				
					//instantiate tile, setting x corrordinate so it will appear off camera
					tempTile=(GameObject)Instantiate(tilePrefab,new Vector3(-1000,0,0),transform.rotation);	
					
				
					//parent temporary tile with the bag object	
					tempTile.transform.parent=transform;
				
					//convert the tile score to an integer
					int.TryParse(tileDataLine[3].ToString()+tileDataLine[4].ToString(),out score);
				
			
					//set the tile score and glyph, also this function adds the correct texture to the tile
					tempTile.GetComponent<TileBehavior>().MakeTile(tileDataLine[0],score);												
				}
			

			// end inner loop
			
			
		}
		
	}
	
	
	//called by the player to take a tile 
	public Transform Draw() {
		
		
		Transform tempTile=null;
		//choose random child tile object and pass it to the shelf
		
		
		//only run this function if there is at least one tile in the bag
		if (transform.childCount>0) {
			tempTile=transform.GetChild(UnityEngine.Random.Range(0,transform.childCount));
		}
		return tempTile;
		
		
	}
	
	
	// always pull a blank tile to test the blank tile functions
	public Transform DrawBlank() {
		
		Transform temptile=null;
		
		foreach (Transform tile in transform) {
			if (tile.GetComponent<TileBehavior>().glyph=='*') {
				temptile=tile;	
			}
		}
		
		return (temptile);
	}	
	
	
	void OnMouseDown() {
		// when the bag is clicked check to see if it is empty, if not tell call the shelf's add function
		
		
		
	}	
	
	
	// Update is called once per frame
	void Update () {
		
	}
}
