using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public class BoardBehavior : MonoBehaviour {
	

	//arrays start a 0 so I'm adding 1 whenever I use these so the indexes make more sense
	//I'm assuming a square board so I only need to define one value
	static int boardRowSize=15;	
	
	//static properties of the board, assuming a square board with an odd number of spaces otherwise this fails
	static float boardSpaceUnit = (float)1.0/ (float)boardRowSize;
	static int boardCenter=(int)((boardRowSize+1)*.5);	

	//space prefab so I don't have to reference it through resource.load
	public GameObject spaceObject;
	
	public PlayerBehavior player;
	
	//adding the board light so i can access it later
	public Light boardLight;
	
	//assign materials to placeholder variables so I can reference them later
	static Material tripleLetterMaterial;
	static Material doubleLetterMaterial;
	static Material doubleWordMaterial;
	static Material tripleWordMaterial;
	
	//array of space objects so that spaces can be referenced by their coordinates on the board
	public static Transform[,] spaces;
		
	// load the resources into the class variables so they are accessible later
	void Awake () {
	
		tripleLetterMaterial = Resources.Load("Materials/Space/SpaceCenter/TripleLetter", typeof(Material)) as Material;
		doubleLetterMaterial = Resources.Load("Materials/Space/SpaceCenter/DoubleLetter", typeof(Material)) as Material;
		doubleWordMaterial = Resources.Load("Materials/Space/SpaceCenter/DoubleWord", typeof(Material)) as Material;
		tripleWordMaterial = Resources.Load("Materials/Space/SpaceCenter/TripleWord", typeof(Material)) as Material;	
		
		//initialize spaces array
		spaces=new Transform[boardRowSize+1,boardRowSize+1];
		
	}	
	
	
	// Use this for initialization
	void Start () {
		/* Here I'm building each space object on the board.  The space objects have to be tinted properly so the player knows which spaces 
		 * have special scoring properties
		 * 
		 * The board data is read in from a text file
		 * 
		 */
		
		//storage for each space as it is instantiated
		GameObject spaceHolder;
		//storage for each line of the board data file as it is read in
		String boardDataLine;
		
		//open the board data file
		FileInfo boardData = new FileInfo ("Assets\\Resources\\Rules\\solo-word-builder-board.txt");
		StreamReader boardDataReader = boardData.OpenText();			
		
		//outer loop iterates over each row of the board
		for (int counterY=1;counterY<boardRowSize+1;counterY++) {
			
			//read next line of board data
			boardDataLine=boardDataReader.ReadLine();
			
			// inner loop iterates over each column of each row making the grid
			for (int counterX=1;counterX<boardRowSize+1;counterX++) {
				
				// instantiate a new space object and assign it to placeholder object, im giving it the same rotation as this board object
				spaceHolder=(GameObject)Instantiate(spaceObject,new Vector3(0,0,0),transform.rotation);				
				
				// parent the space object to the board so I can set its location and size properties relative to the board;
				spaceHolder.transform.parent=transform;
				
				// move the space to the correct spot and scale it
				//((SpaceBehavior)spaceHolder.GetComponent(typeof(SpaceBehavior))).defineSpace(transform,counterX,counterY);
				spaceHolder.transform.localPosition=new Vector3((counterX-boardCenter)*boardSpaceUnit,-1*(counterY-boardCenter)*boardSpaceUnit,-.5f);
				spaceHolder.transform.localScale=new Vector3(boardSpaceUnit,boardSpaceUnit,.5f);				
				
				//tell the space where it is located
				spaceHolder.GetComponent<SpaceBehavior>().y=counterY;
				spaceHolder.GetComponent<SpaceBehavior>().x=counterX;
				
				//fill the spaces array
				spaces[counterX,counterY]=spaceHolder.transform;
				
				//assign the correct tint to the space
				switch (boardDataLine[counterX-1]) 
				{
					
				case 'e' :
					spaceHolder.GetComponent<SpaceBehavior>().type="Double Letter";
					spaceHolder.transform.Find("SpaceCenter").renderer.material = doubleLetterMaterial;					
					break;
					
				case 'r' :
					spaceHolder.GetComponent<SpaceBehavior>().type="Triple Word";
					spaceHolder.transform.Find("SpaceCenter").renderer.material = tripleWordMaterial;						
					break;
					
				case 't' :
					spaceHolder.GetComponent<SpaceBehavior>().type="Triple Letter";
					spaceHolder.transform.Find("SpaceCenter").renderer.material = tripleLetterMaterial;				
					break;
					
				case 'o' :
					spaceHolder.GetComponent<SpaceBehavior>().type="Double Word";
					spaceHolder.transform.Find("SpaceCenter").renderer.material = doubleWordMaterial;
					break;
					
				case 'n' :
					spaceHolder.GetComponent<SpaceBehavior>().type="Normal";
					// not assigning material because the spaceobject has a default normal material already assigned
					break;
					
				default :
					Debug.LogError("bad data in board data file : " + boardDataLine[counterX-1]);
					break;		
				}				
					
				
			}
		}
		
	}
	
	//calculate the first and last space in the row or column with the word in progress
	static int CalculateFirstPlacedSpace (string downOrAcross,List<Transform> placedTiles) {
		int firstPlacedSpacePosition=boardRowSize+1;
		
		foreach (Transform tileToCheck in placedTiles) {
			if (downOrAcross=="down") {
				if (tileToCheck.parent.GetComponent<SpaceBehavior>().y<firstPlacedSpacePosition) {
					firstPlacedSpacePosition=tileToCheck.parent.GetComponent<SpaceBehavior>().y;
				}		
			}		
			if (downOrAcross=="across") {
				//Debug.Log("y coordinate "+tileToCheck.parent.GetComponent<SpaceBehavior>().x);
				if (tileToCheck.parent.GetComponent<SpaceBehavior>().x<firstPlacedSpacePosition) {
					firstPlacedSpacePosition=tileToCheck.parent.GetComponent<SpaceBehavior>().x;
				}		
			}			
		}
		//Debug.Log("first place position "+firstPlacedSpacePosition);
		return firstPlacedSpacePosition;
		
		
	}	
	
	

	static int CalculateLastPlacedSpace (string downOrAcross,List<Transform> placedTiles) {
		int lastPlacedSpacePosition=0;
		
		foreach (Transform tileToCheck in placedTiles) {
			if (downOrAcross=="down") {
				if (tileToCheck.parent.GetComponent<SpaceBehavior>().y>lastPlacedSpacePosition) {
					lastPlacedSpacePosition=tileToCheck.parent.GetComponent<SpaceBehavior>().y;
				}		
			}		
			if (downOrAcross=="across") {
				//Debug.Log(lastPlacedSpacePosition);
				if (tileToCheck.parent.GetComponent<SpaceBehavior>().x>lastPlacedSpacePosition) {
					lastPlacedSpacePosition=tileToCheck.parent.GetComponent<SpaceBehavior>().x;
				}		
			}			
		}
		
		return lastPlacedSpacePosition;
		
	}		
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	//  returns true if middle is covered
	public static bool IsMiddleCovered() {
		bool middleCovered=false;
		
		if (spaces[boardCenter,boardCenter].GetComponent<SpaceBehavior>().hasTile) {
			middleCovered=true;	
			
		}		
		
		return middleCovered;
	}	
	
	
	//returns true if this is the first word on the board
	public static bool IsFirstword() {
		bool firstWord=true;
		
		if (spaces[boardCenter,boardCenter].GetComponentInChildren<TileBehavior>().scored) {
			firstWord=false;
		}
			
		return firstWord;
		
	}	
	
	
	//method called by the player to validate the word
	public static List<List<Transform>> GetScoredWords(List<Transform> placedTiles) {
		
		//stores the word before it is added to the scored words list
		List <Transform> tempword= new List<Transform>();  
		
		bool wordGoingDown=true;
		bool wordGoingAcross=true;
		bool middleCovered=false;
		bool tilesContiguous=false;
		bool singleTile=false;
		//tiles adjacent is only checked if the middle is covered
		bool tilesAdjacent=false;		
		//flag for first word, checked later to see if we test for adjacent tiles
		bool firstWord=true;
		//list of scored words which is returned to the player to check against the dictionary
		List<List<Transform>> scoredWords=new List<List <Transform>>();		
		
	/*---------------check to make sure the center is covered ----------------------------- */
		middleCovered=IsMiddleCovered();
		
	/*--------------- check to see if this is the first word
	 * only fire this if the middle is covered or else it will give an out of range error ----------------------------- */
		
		if (middleCovered) firstWord=IsFirstword();
		
	// only check for adjacent tiles if this isn't the first word
		if (!(firstWord)) tilesAdjacent=CheckAdjacent(placedTiles);
		//Debug.Log("tiles adjacent"+tilesAdjacent);
		
		//logic to check for a single tile
		if (placedTiles.Count==1) singleTile=true;
		
		//check to make sure tiles are all in the same row or column, only do this if there isn't a single tile
		if (!(singleTile)) {		
			for (int i = 1;i<placedTiles.Count;i++) {
			//Debug.Log ("letter of tile i"+placedTiles[i].GetComponent<TileBehavior>().glyph.ToString()+", letter of tile i-1 "+placedTiles[i-1].GetComponent<TileBehavior>().glyph.ToString());
			//Debug.Log ("x coordinates of tiles i"+placedTiles[i].parent.GetComponent<SpaceBehavior>().x+", x coordinates of tiles i-1 "+placedTiles[i-1].parent.GetComponent<SpaceBehavior>().x);
				if ((placedTiles[i].parent.GetComponent<SpaceBehavior>().x)!=(placedTiles[i-1].parent.GetComponent<SpaceBehavior>().x)) {
				//Debug.Log ("X"+i);
					wordGoingDown=false; 		
				}
				if (placedTiles[i].parent.GetComponent<SpaceBehavior>().y != placedTiles[i-1].parent.GetComponent<SpaceBehavior>().y) {
				//Debug.Log ("Y"+i);
					wordGoingAcross=false;
				}
			}
			
			//check to see if tiles are contiguous
			if (wordGoingDown || wordGoingAcross) {
				if (wordGoingDown) {
					tilesContiguous=TilesContiguous(CalculateFirstPlacedSpace("down",placedTiles),CalculateLastPlacedSpace("down",placedTiles),placedTiles,"down");
				}
				else {
					//Debug.Log(CalculateLastPlacedSpace("across",placedTiles));
					tilesContiguous=TilesContiguous(CalculateFirstPlacedSpace("across",placedTiles),CalculateLastPlacedSpace("across",placedTiles),placedTiles,"across");
					
				}
			}									
		}
		//Debug.Log("wordgoingdown "+wordGoingDown+" wordgoingacross "+wordGoingAcross);
		
		/* if tile placement rules aren't followed correctly display error message and stop
			
			tests are either it is a single tile
			or the multiple tiles are in one row or column, and there are contiguous tiles between the first placed tile and the last placed tile
			and the middle is covered and this is the first word OR the tiles are placed next to a previously scored word
			
			((((wordGoingDown || wordGoingAcross) && tilesContiguous) || singleTile) && middleCovered)			
			
		*/
		if (!((((wordGoingDown || wordGoingAcross) && tilesContiguous) || singleTile) && ((middleCovered && firstWord) || tilesAdjacent) )) {
			//Debug.Log("wordGoingDown "+wordGoingDown.ToString()+" wordGoingAcross "+wordGoingAcross.ToString()+" middlecovered "+middlecovered.ToString()+" tilescontiguous "+tilesContiguous.ToString());
			StatusBarBehavior.Display("tiles must be in same row or column and\neither in the center or adjacent to previously scored tiles");

			
		}
		else {
		//if we made it this far then we know the tiles are placed correctly now we need to build the word that is being scored	
			
			//check to see if there is only one tile, if there is we skip building the "initial" word
			if(!(singleTile)) {
				if (wordGoingDown) {
					//Debug.Log("entering buildword down");
					//Debug.Log("wordGoingDown"+placedTiles[0].parent.GetComponent<SpaceBehavior>().x.ToString()+","+CalculateFirstPlacedSpace("down",placedTiles).ToString());
					tempword=BuildWord("down",   spaces[placedTiles[0].parent.GetComponent<SpaceBehavior>().x,CalculateFirstPlacedSpace("down",placedTiles)]);		
				}	
				else {
					//Debug.Log ("wordGoingAcross"+CalculateLastPlacedSpace("across",placedTiles).ToString()+","+placedTiles[0].parent.GetComponent<SpaceBehavior>().x.ToString());
					tempword=BuildWord("across",spaces[CalculateFirstPlacedSpace("across",placedTiles),placedTiles[0].parent.GetComponent<SpaceBehavior>().y]);
				}	
				scoredWords.Add(tempword);
			}
			
			//now loop over all the placed tiles and check each one to see if it makes a word in the opposite of the "main" direction"
			foreach(Transform tile in placedTiles) {
				tempword=new List<Transform>();
				if (wordGoingDown || singleTile) {
					//Debug.Log("in check perpendicular down");
					tempword=GetPerpendicularWord("down",tile);
					if (tempword.Count>0) scoredWords.Add(tempword);
				}	
				if (wordGoingAcross || singleTile) {
					//Debug.Log("in check perpendicular across");
					tempword=GetPerpendicularWord("across",tile);
					if (tempword.Count>0) scoredWords.Add(tempword);
				}	
			}
			
			//fixing scoring of single tile in center
			if (singleTile && middleCovered && (!(spaces[boardCenter,boardCenter].GetComponentInChildren<TileBehavior>().scored))) {
				List<Transform> singleTileWord=new List<Transform>();
				singleTileWord.Add(placedTiles[0]);
				scoredWords.Add(singleTileWord);
			}
			
			
		}
		//Debug.Log(scoredWords.Count);
		
		return scoredWords;
	}
	
	//check tiles to see if there are any words being made perpendicular to the main word
	static List<Transform> GetPerpendicularWord(string acrossOrDown, Transform tile) {
		List<Transform> tempWord=new List<Transform>();
		
		//Debug.Log("in getperpendicular with the value of acrossOrDown ="+acrossOrDown);
		
		//check neighbors on perpendicular to see if there are any letters
		if (CheckPerpendicular(acrossOrDown,tile.parent)) {
			Debug.Log("there is a word perpendicular to "+tile.GetComponent<TileBehavior>().glyph+" in the "+acrossOrDown+" direction");
			List<Transform> tempTiles=new List<Transform>();
			tempTiles.Add(tile);
				if (acrossOrDown=="down") {
					Debug.Log("entering perpendicular buildword down");
					//Debug.Log("wordGoingDown"+placedTiles[0].parent.GetComponent<SpaceBehavior>().x.ToString()+","+CalculateFirstPlacedSpace("down",placedTiles).ToString());
					
					tempWord=BuildWord("across",spaces[CalculateFirstPlacedSpace("across",tempTiles),tile.parent.GetComponent<SpaceBehavior>().y]);
				}	
				else {
					//Debug.Log("entering perpendicular buildword across");
				//Debug.Log ("wordGoingAcross"+CalculateLastPlacedSpace("across",placedTiles).ToString()+","+placedTiles[0].parent.GetComponent<SpaceBehavior>().x.ToString());
					tempWord=BuildWord("down", spaces[tile.parent.GetComponent<SpaceBehavior>().x,CalculateFirstPlacedSpace("down",tempTiles)]);
				}			
			
		}	
		return tempWord;
	}	
	
	
	//this checks the perpendicular for each tile in the opposite direction of the initial word
	static bool CheckPerpendicular(string acrossOrDown, Transform space) {
		bool makesWord=false;
		SpaceBehavior tempSpace=space.GetComponent<SpaceBehavior>();
		
		
		if (acrossOrDown=="across") {
			
			Debug.Log("in checkperpendicular across");
			if (tempSpace.y>1) {
				if (spaces[tempSpace.x,tempSpace.y-1].GetComponent<SpaceBehavior>().hasTile) makesWord=true;
			}	
			if (tempSpace.y<15) {	
				if (spaces[tempSpace.x,tempSpace.y+1].GetComponent<SpaceBehavior>().hasTile) makesWord=true;
			}				
		}
		if (acrossOrDown=="down") {
			Debug.Log("in checkperpendicular down");
			if (tempSpace.x>1) {
				if (spaces[tempSpace.x-1,tempSpace.y].GetComponent<SpaceBehavior>().hasTile) makesWord=true;
			}	
			if (tempSpace.x<15) {
				if (spaces[tempSpace.x+1,tempSpace.y].GetComponent<SpaceBehavior>().hasTile) makesWord=true;
			}			
		}	
		
		
		return makesWord;
	}	
		
	
	//method used to check to make sure tiles are adjacent
	static bool CheckAdjacent(List<Transform> placedTiles) {
		bool isAdjacent=false;
		
		foreach(Transform tile in placedTiles) {
			if (CheckNeighbors(tile)) {
				isAdjacent=true;	
			}	
		}	
		
		return isAdjacent;
	}
	
	
	//chescks a tiles neighbors to see if there is a scored tile there
	static bool CheckNeighbors(Transform tile) {
		bool hasNeighbors=false;
		
		SpaceBehavior tileSpace=tile.parent.GetComponent<SpaceBehavior>();
		//check top
		if (tileSpace.y>1) {
			Transform neighborToCheck=spaces[tileSpace.x,tileSpace.y-1];
			
			if (neighborToCheck.GetComponent<SpaceBehavior>().hasTile && neighborToCheck.GetComponentInChildren<TileBehavior>().scored) {
				hasNeighbors=true;
			}			
		}	
		
		//check bottom
		if (tileSpace.y<boardRowSize) {
			Transform neighborToCheck=spaces[tileSpace.x,tileSpace.y+1];			
			if (neighborToCheck.GetComponent<SpaceBehavior>().hasTile && neighborToCheck.GetComponentInChildren<TileBehavior>().scored) {
				hasNeighbors=true;
			}				
		}			
		
		//check left
		if (tileSpace.x>1) {
			Transform neighborToCheck=spaces[tileSpace.x-1,tileSpace.y];			
			if (neighborToCheck.GetComponent<SpaceBehavior>().hasTile && neighborToCheck.GetComponentInChildren<TileBehavior>().scored) {
				hasNeighbors=true;
			}				
		}	
		
		//check right
		if (tileSpace.x<boardRowSize) {
			Transform neighborToCheck=spaces[tileSpace.x+1,tileSpace.y];
			if (neighborToCheck.GetComponent<SpaceBehavior>().hasTile && neighborToCheck.GetComponentInChildren<TileBehavior>().scored) {
				hasNeighbors=true;
			}			
		}	
		
		
		return hasNeighbors;
	}	
	
	
	
	//method that is used to build a word from scoring
	static List<Transform> BuildWord(String downOrAcross, Transform firstSpace) {
		int walkcounter;
		List<Transform> buildWord=new List<Transform>();
		Transform tempLetter;
		
		
		if (downOrAcross=="down") {
			//Debug.Log("in walk down function");
			walkcounter=firstSpace.GetComponent<SpaceBehavior>().y-1;
			//walk backwards
			while(walkcounter>=1 && (spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter].GetComponent<SpaceBehavior>().hasTile)) {
				walkcounter--;
				//Debug.Log("down word walk backwards loop");
			}
			walkcounter++;
			//Debug.Log(firstSpace.GetComponent<SpaceBehavior>().x+" "+walkcounter);
			//walk forwards and fill word
			while(walkcounter<=15 && (spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter].GetComponent<SpaceBehavior>().hasTile)) {
				tempLetter=spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter];
				//Debug.Log("in walk foward loop");
				//Debug.Log(tempLetter.ToString());
				buildWord.Add(tempLetter);
				//Debug.Log(buildWord);
				walkcounter++;
				//Debug.Log("next space is "+walkcounter+" "+firstSpace.GetComponent<SpaceBehavior>().y);
			}			
			
			
		}
		else {
			//Debug.Log("in across walk function");
			walkcounter=firstSpace.GetComponent<SpaceBehavior>().x-1;
			//walk backwards
			while(walkcounter>=1 && (spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y].GetComponent<SpaceBehavior>().hasTile)) {
				//Debug.Log("in walk backwards");
				walkcounter--;
			}
			walkcounter++;
			//Debug.Log(walkcounter+" "+firstSpace.GetComponent<SpaceBehavior>().y);
			//walk forwards and fill word
			while(walkcounter<=15 && (spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y].GetComponent<SpaceBehavior>().hasTile)) {
				tempLetter=spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y];
				//Debug.Log("in walk foward loop");
				//Debug.Log(tempLetter.ToString());
				buildWord.Add(tempLetter);
				//Debug.Log(buildWord);
				walkcounter++;
				//Debug.Log("next space is "+walkcounter+" "+firstSpace.GetComponent<SpaceBehavior>().y);
			}	
		}	
		
		
		return buildWord;	
	}	
	
	
	
	//check to see if the tiles are contiguous
	static bool TilesContiguous(int firstPosition, int lastPosition, List<Transform> placedTiles,string downOrAcross) {
		bool tilesContiguous=true;
		//Debug.Log("first "+firstPosition+" last "+lastPosition);
		
		//loop between the first and last tile and check to see if all of the spaces are covered
		for (int i=firstPosition+1;i<lastPosition;i++) {
			
			
			
			if (downOrAcross=="down") {
				if (!(spaces[placedTiles[0].parent.GetComponent<SpaceBehavior>().x,i].GetComponent<SpaceBehavior>().hasTile)) {
					tilesContiguous=false;
					//Debug.Log(placedTiles[0].parent.GetComponent<SpaceBehavior>().x);
				}				
			}
			else {
				if (!(spaces[i,placedTiles[0].parent.GetComponent<SpaceBehavior>().y].GetComponent<SpaceBehavior>().hasTile)) {
					tilesContiguous=false;
					//Debug.Log("incolumntest");
				}				
			}
			//Debug.Log(i+" tilescontiguous "+tilesContiguous);
		}
		
		return tilesContiguous;
	}			
	
}