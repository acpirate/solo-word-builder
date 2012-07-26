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
	
	//used to check to see if no tiles have been placed
	static bool noTiles=true;
	
	//list of valid spaces
	List<Transform> validSpaces = new List<Transform>();
	
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
		
		// this will always highlight the center tile
		//CalculateValidSpaces();
		
	}
	
	// this doesnt help gameplay so I'm not doing it anymore, leaving in the code for possible revival in the future
	//light up the valid spaces
	public void CalculateValidSpaces() {
		// store the value of the previous spaces before the lights are cleared
		/*List<Transform> previousValidSpaces = new List<Transform>();
		previousValidSpaces=validSpaces;*/
		//clear all the lights
		foreach(Transform space in transform) {
			space.GetComponent<SpaceBehavior>().UnHighlight();
		}
		validSpaces.Clear();
		
		
		/* three scenarios, no tiles placed yet, 1 tile placed, and more than one tile placed
		 * if there are no tiles placed and there are no tiles scored the only valid space is the center space
		 * 
		 * if there is one tile placed then either the row or column containing that tile becomes valid
		 * 
		 * if there is more than one tile placed then the only valid space is in the row or column lined up with the two placed tiles
		 * 
		 */
		
		// prevent he possibility of invalid placement on the first turn, if the center tile is removed then all the other tiles are removed
		if (!(spaces[boardCenter,boardCenter].GetComponent<SpaceBehavior>().hasTile)) {
			player.RemoveAllUnscoredTiles();
		}	
		
		
		switch (player.workingTiles.Count) {
			
		case 0:
			if (noTiles) {	
				validSpaces.Add(spaces[boardCenter,boardCenter]);	
			}			
			break;
		case 1:
			//wow this actually works
			SpaceBehavior firstPlacedSpace = player.workingTiles[0].transform.parent.GetComponent<SpaceBehavior>();
			for (int i=((ShelfBehavior.maxTiles-1)*-1);i<ShelfBehavior.maxTiles;i++) {
				if ((firstPlacedSpace.y+i>0) && (firstPlacedSpace.x+i>0) && (firstPlacedSpace.y+i<16) &&(firstPlacedSpace.x+i<16)) {
					//dont highlight the space that already has a tile in it probably do this better in a separate function
					if (i!=0) {
					validSpaces.Add(spaces[firstPlacedSpace.x,firstPlacedSpace.y+i]);
					validSpaces.Add(spaces[firstPlacedSpace.x+i,firstPlacedSpace.y]);
					}
				}	
			}
			break;
		// case 2-7	
		default :
	/*		string downOrAcross="error";
			if (player.workingTiles[0].parent.GetComponent<SpaceBehavior>().y == player.workingTiles[1].parent.GetComponent<SpaceBehavior>().y) {
				downOrAcross="down";
			}
			if (player.workingTiles[0].parent.GetComponent<SpaceBehavior>().x == player.workingTiles[1].parent.GetComponent<SpaceBehavior>().x) {
				downOrAcross="across";
			}			
			Debug.Log("first space position"+CalculateFirstPlacedSpace(downOrAcross).ToString()+"lastspaceposition"+CalculateLastPlacedSpace(downOrAcross).ToString());
			for (int i=(CalculateLastPlacedSpace(downOrAcross)-shelf.childCount);i<(CalculateFirstPlacedSpace(downOrAcross)+shelf.childCount);i++) {
				if (downOrAcross=="down") {
					validSpaces.Add(spaces[i,player.workingTiles[0].parent.GetComponent<SpaceBehavior>().x]);
				}
				if (downOrAcross=="across") {
					validSpaces.Add(spaces[player.workingTiles[0].parent.GetComponent<SpaceBehavior>().y,i]);
				}	
			}	*/
			
			
			break;
		}
		

		
		
		TurnOnSpaceLights(validSpaces);
		
	}	
	
	//turn on the lights in the valid spaces
	void TurnOnSpaceLights(List<Transform> validSpaces) {
		/* for every space in the valid space list turn on its light */
		foreach (Transform validSpace in validSpaces) {	
			validSpace.GetComponent<SpaceBehavior>().Highlight();
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
				if (tileToCheck.parent.GetComponent<SpaceBehavior>().x>lastPlacedSpacePosition) {
					lastPlacedSpacePosition=tileToCheck.parent.GetComponent<SpaceBehavior>().x;
				}		
			}		
			if (downOrAcross=="across") {
				if (tileToCheck.parent.GetComponent<SpaceBehavior>().y>lastPlacedSpacePosition) {
					lastPlacedSpacePosition=tileToCheck.parent.GetComponent<SpaceBehavior>().y;
				}		
			}			
		}
		
		return lastPlacedSpacePosition;
		
	}		
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	//method called by the player to validate the word
	public static List<List<Transform>> GetScoredWords(List<Transform> placedTiles) {
		List <Transform> tempword= new List<Transform>();
		
		bool wordGoingDown=true;
		bool wordGoingAcross=true;
		bool middlecovered=false;
		bool tilesContiguous=false;
		List<List<Transform>> scoredWords=new List<List <Transform>>();
		

		
		//check to make sure tiles are all in the same row or column
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
		// check to make sure the center is covered
		if (spaces[boardCenter,boardCenter].GetComponent<SpaceBehavior>().hasTile) {
			middlecovered=true;	
		}
		
		//check to see if tiles are contiguous
		if (wordGoingDown || wordGoingAcross) {
			if (wordGoingDown) {
				tilesContiguous=TilesContiguous(CalculateFirstPlacedSpace("down",placedTiles),CalculateLastPlacedSpace("down",placedTiles),placedTiles,"down");
			}
			else tilesContiguous=TilesContiguous(CalculateFirstPlacedSpace("across",placedTiles),CalculateLastPlacedSpace("across",placedTiles),placedTiles,"across");
		}
		//Debug.Log("wordgoingdown "+wordGoingDown+" wordgoingacross "+wordGoingAcross);
		
		//if tile placement rules aren't followed correctly display error message and stop
		if (!((wordGoingDown || wordGoingAcross) && middlecovered && tilesContiguous)) {
			//Debug.Log("wordGoingDown "+wordGoingDown.ToString()+" wordGoingAcross "+wordGoingAcross.ToString()+" middlecovered "+middlecovered.ToString()+" tilescontiguous "+tilesContiguous.ToString());
			StatusBarBehavior.Display("the tiles are not placed correctly");

			
		}
		else {
		//if we made it this far then we know the tiles are placed correctly now we need to build the word that is being scored
		
						
			
			if (wordGoingDown) {
			Debug.Log("entering buildword down");
				//Debug.Log("wordGoingDown"+placedTiles[0].parent.GetComponent<SpaceBehavior>().x.ToString()+","+CalculateFirstPlacedSpace("down",placedTiles).ToString());
				tempword=BuildWord("down",   spaces[placedTiles[0].parent.GetComponent<SpaceBehavior>().x,CalculateFirstPlacedSpace("down",placedTiles)]);
				
				
				
			}	
			else {
				
				//Debug.Log ("wordGoingAcross"+CalculateLastPlacedSpace("across",placedTiles).ToString()+","+placedTiles[0].parent.GetComponent<SpaceBehavior>().x.ToString());
				tempword=BuildWord("across",spaces[CalculateFirstPlacedSpace("across",placedTiles),placedTiles[0].parent.GetComponent<SpaceBehavior>().y]);
			}	
				
			scoredWords.Add(tempword);
			
		}
		
		return scoredWords;
		
	}
	
	//method that is used to build a word from scoring
	static List<Transform> BuildWord(String downOrAcross, Transform firstSpace) {
		int walkcounter;
		List<Transform> buildWord=new List<Transform>();
		Transform tempLetter;
		
		
		if (downOrAcross=="down") {
			walkcounter=firstSpace.GetComponent<SpaceBehavior>().y-1;
			//walk backwards
			while((spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter].GetComponent<SpaceBehavior>().hasTile) && walkcounter>=1) {
				walkcounter--;
				Debug.Log("down word walk backwards loop");
			}
			walkcounter++;
			Debug.Log(firstSpace.GetComponent<SpaceBehavior>().x+" "+walkcounter);
			//walk forwards and fill word
			while((spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter].GetComponent<SpaceBehavior>().hasTile) && walkcounter<=15) {
				tempLetter=spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter];
				Debug.Log("in walk foward loop");
				//if (tempLetter=='*') tempLetter=spaces[firstSpace.GetComponent<SpaceBehavior>().x,walkcounter].GetComponentInChildren<TileBehavior>().wildLetter;
				//Debug.Log(tempLetter.ToString());
				buildWord.Add(tempLetter);
				//Debug.Log(buildWord);
				walkcounter++;
				Debug.Log("next space is "+walkcounter+" "+firstSpace.GetComponent<SpaceBehavior>().y);
			}			
			
			
		}
		else {
			//Debug.Log("in across walk function");
			walkcounter=firstSpace.GetComponent<SpaceBehavior>().x-1;
			//walk backwards
			while((spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y].GetComponent<SpaceBehavior>().hasTile) && walkcounter>=1) {
				walkcounter--;
			}
			walkcounter++;
			//Debug.Log(walkcounter+" "+firstSpace.GetComponent<SpaceBehavior>().y);
			//walk forwards and fill word
			while((spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y].GetComponent<SpaceBehavior>().hasTile) && walkcounter<=15) {
				tempLetter=spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y];
				//Debug.Log("in walk foward loop");
				//if (tempLetter=='*') tempLetter=spaces[walkcounter,firstSpace.GetComponent<SpaceBehavior>().y].GetComponentInChildren<TileBehavior>().wildLetter;
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
					Debug.Log(placedTiles[0].parent.GetComponent<SpaceBehavior>().x);
				}				
			}
			else {
				if (!(spaces[i,placedTiles[0].parent.GetComponent<SpaceBehavior>().y].GetComponent<SpaceBehavior>().hasTile)) {
					tilesContiguous=false;
					//Debug.Log("incolumntest");
				}				
			}
			Debug.Log(i+" tilescontiguous "+tilesContiguous);
		}
		
		return tilesContiguous;
	}
 	

	//Not used anymore, not checking if space is valid before placing tile
	public bool CheckIfSpaceIsValid(Transform spaceToCheck) {
		if (validSpaces.Contains(spaceToCheck)) {
			return true;
		}	
		else return false;
		
	}	
				
	
}

public class WordSpaces {
	
	public List<Transform> wordSpaces= new List<Transform>();
	
	
	
}