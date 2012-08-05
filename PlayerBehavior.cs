using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerBehavior : MonoBehaviour {

	//variables used to initialize and access other game objects
	public BagBehavior bag;
	public ShelfBehavior shelf;
	public BoardBehavior board;

	public int initalDraw;
	Transform selectedTile;
	
	//list of tiles that the player is working with on the board
	public List<Transform> workingTiles;
	
	//the mask on the screen when the gui blank tile character is being chosen
	static GameObject blur;
	
	//flag to determine whether or not to draw blank tile pick gui elements
	bool pickingBlank=false;
	
	//flag to determine if player is in end game dialog
	bool endGameDialog=false;
	
	//flag for game over confirmed
	bool gameOverConfirmed=false;
	
	//flag for in high score stage\
	bool inHighScores=false;
	
	//username
	private string playerName="";
	
	//score
	static int totalScore;
	//string for storing the high score listing returned from the server
	//stupid 0 indexed arrays
	string[] splitHighScore=new string[44];
	
	//initialization as object is created
	void Awake() {
		workingTiles = new List<Transform>();
		//find the blur object which is used to make a backdrop for the gui		
		if (!(blur)) {
			blur= GameObject.Find("Blur");
			blur.active=false;
		}		
	}	
	
	// Use this for initialization (on first frame?)
	void Start () {
		//routine for testing blank tile, forcing blank to always be first tile
		//shelf.AddTile(bag.DrawBlank());
		
		// draw tiles to fill the shelf	
		shelf.FillTiles();
	}
		
	// called when the player has finished placing his tiles and clicks the "score" button	
	public void ScoreWord() {
		//list of lists of spaces containing all of the words that were scored
		List<List <Transform>> scoredWords=BoardBehavior.GetScoredWords(workingTiles);
		bool wordsInDictionary=true;
		string extractedWord;
		
		
		if (scoredWords.Count>0) {
			string invalidWord="";
			
			foreach(List<Transform> word in scoredWords) {
				extractedWord=ExtractWord(word);
				if (!(GameRules.dictionary.Contains(extractedWord))) {
					wordsInDictionary=false;
					invalidWord=extractedWord;
				}	
			}			
				
			if (wordsInDictionary) {
				string statusString="";
				foreach (List<Transform> word in scoredWords) {
					extractedWord=ExtractWord(word);
					int tempScore=CalculateScore(word);
					statusString+=extractedWord+" scored "+tempScore+" ";
					totalScore+=tempScore;					
				}	
					statusString+="\nTotal Score Is "+totalScore;
				
					//score each working tile
					foreach (Transform tile in workingTiles) {
						tile.GetComponent<TileBehavior>().ScoreTile();	
					}
					//clear the working tiles so they can't be removed with backspace
					workingTiles.Clear();
					//draw new tiles from the bag to fill the shelf
					shelf.FillTiles();
					//tell the player the results of their placement
					StatusBarBehavior.Display(statusString);
			}
			else {
					StatusBarBehavior.Display(invalidWord+" is not in the dictionary");

			}
									
		}
	}
	
	public int CalculateScore(List <Transform> scoredWord) {
		/* Double Word, Double Letter, Triple word, Triple Letter */
		
		int tempScore=0;
		int scoreDoubleWord=0;
		int scoreTripleWord=0;
		
		for (int i=0;i<scoredWord.Count;i++) {
			//temp varialbe to reference the child tile of the space we are scoring
			TileBehavior tileReference=scoredWord[i].GetComponentInChildren<TileBehavior>();
			SpaceBehavior spaceReference=scoredWord[i].GetComponent<SpaceBehavior>();
			//if a tiles hasn't been scored yet take its special space into account
			if (!(tileReference.scored)) {
				if (spaceReference.type=="Double Word") scoreDoubleWord++;
				if (spaceReference.type=="Triple Word") scoreTripleWord++;
				if (spaceReference.type=="Double Letter") tempScore+=tileReference.scoreValue;
				if (spaceReference.type=="Triple Letter") tempScore+=(tileReference.scoreValue*2);
			}
			tempScore+=tileReference.scoreValue;
			//remove the tile from the working tiles list so i can't be removed with backspace
		}
		
	
		//double and triple letter bonus, cant get both on standard scrabble board and can't get any more than once so not worrying about extra multipliers
		tempScore=(int) (tempScore * Math.Pow(2d,(double) scoreDoubleWord));
		tempScore=(int) (tempScore * Math.Pow(3d,(double) scoreTripleWord));
		
		//50 point bonus for using 7 letters
		if (workingTiles.Count==7) tempScore+=50;
		
		return tempScore;
	}	
	
	//extract the word out of the list of spaces containing the tiles the player is trying to score
	public static string ExtractWord(List <Transform> scoredWord) {
		string tempWord="";
		
		for (int i=0;i<scoredWord.Count;i++) {
			char tempLetter=scoredWord[i].GetComponentInChildren<TileBehavior>().glyph;
			if (tempLetter=='*') tempLetter=scoredWord[i].GetComponentInChildren<TileBehavior>().wildLetter;
			tempWord+=tempLetter;	
		}	
		
		return tempWord;
		
		
	}	
	
	//handles player clicks on tiles that are on shelf
	public void PickTile(Transform inTile) {
		if (!(selectedTile)) {
			DesignateSelectedTile (inTile);
		}
		else {
			//indicate old tile is no longer selected
			UnDesignateSelectedTile();
			DesignateSelectedTile (inTile);			
		}
	}
	
	// functions to group tile selection behavior
	void DesignateSelectedTile(Transform inTile) {
		selectedTile=inTile;
		HighlightSelectedTile();
	}
	
	void UnDesignateSelectedTile() {
		UnhighlightSelectedTile();
	}
	
	// functions to group highlighting behavior for selected tiles
	void HighlightSelectedTile() {
		selectedTile.GetComponent<TileBehavior>().Highlight();	
	}
	void UnhighlightSelectedTile() {
		selectedTile.GetComponent<TileBehavior>().Unhighlight();	
	}	
	
	
	//using player update to select tiles by keypress
	// Update is called once per frame
	void Update () {
		
		if (!(pickingBlank || endGameDialog || gameOverConfirmed)) {
		// have to test each key because Input.inputString is flakey
			if (Input.GetKeyDown(KeyCode.A)) KeyBoardSelect('A');      
			if (Input.GetKeyDown(KeyCode.B)) KeyBoardSelect('B');
			if (Input.GetKeyDown(KeyCode.C)) KeyBoardSelect('C');
			if (Input.GetKeyDown(KeyCode.D)) KeyBoardSelect('D');
			if (Input.GetKeyDown(KeyCode.E)) KeyBoardSelect('E');      
			if (Input.GetKeyDown(KeyCode.F)) KeyBoardSelect('F');
			if (Input.GetKeyDown(KeyCode.G)) KeyBoardSelect('G');
			if (Input.GetKeyDown(KeyCode.H)) KeyBoardSelect('H');
			if (Input.GetKeyDown(KeyCode.I)) KeyBoardSelect('I');      
			if (Input.GetKeyDown(KeyCode.J)) KeyBoardSelect('J');
			if (Input.GetKeyDown(KeyCode.K)) KeyBoardSelect('K');
			if (Input.GetKeyDown(KeyCode.L)) KeyBoardSelect('L');
			if (Input.GetKeyDown(KeyCode.M)) KeyBoardSelect('M');      
			if (Input.GetKeyDown(KeyCode.N)) KeyBoardSelect('N');
			if (Input.GetKeyDown(KeyCode.O)) KeyBoardSelect('O');
			if (Input.GetKeyDown(KeyCode.P)) KeyBoardSelect('P');
			if (Input.GetKeyDown(KeyCode.Q)) KeyBoardSelect('Q');      
			if (Input.GetKeyDown(KeyCode.R)) KeyBoardSelect('R');
			if (Input.GetKeyDown(KeyCode.S)) KeyBoardSelect('S');
			if (Input.GetKeyDown(KeyCode.T)) KeyBoardSelect('T');
			if (Input.GetKeyDown(KeyCode.U)) KeyBoardSelect('U');      
			if (Input.GetKeyDown(KeyCode.V)) KeyBoardSelect('V');
			if (Input.GetKeyDown(KeyCode.W)) KeyBoardSelect('W');
			if (Input.GetKeyDown(KeyCode.X)) KeyBoardSelect('X');
			if (Input.GetKeyDown(KeyCode.Y)) KeyBoardSelect('Y');      
			if (Input.GetKeyDown(KeyCode.Z)) KeyBoardSelect('Z');
			if (Input.GetKeyDown(KeyCode.Space)) KeyBoardSelect('*');
			if ((Input.GetKeyDown(KeyCode.Backspace)) || (Input.GetKeyDown(KeyCode.Delete))) RemoveLastTile();
			if ((Input.GetKeyDown(KeyCode.Return)) || (Input.GetKeyDown(KeyCode.KeypadEnter))) ScoreWord();
		}
		
		//allow keyboard select of blank tile
		if (pickingBlank) {
			if (Input.GetKeyDown(KeyCode.A)) SelectedBlankLetter('A');      
			if (Input.GetKeyDown(KeyCode.B)) SelectedBlankLetter('B');
			if (Input.GetKeyDown(KeyCode.C)) SelectedBlankLetter('C');
			if (Input.GetKeyDown(KeyCode.D)) SelectedBlankLetter('D');
			if (Input.GetKeyDown(KeyCode.E)) SelectedBlankLetter('E');      
			if (Input.GetKeyDown(KeyCode.F)) SelectedBlankLetter('F');
			if (Input.GetKeyDown(KeyCode.G)) SelectedBlankLetter('G');
			if (Input.GetKeyDown(KeyCode.H)) SelectedBlankLetter('H');
			if (Input.GetKeyDown(KeyCode.I)) SelectedBlankLetter('I');      
			if (Input.GetKeyDown(KeyCode.J)) SelectedBlankLetter('J');
			if (Input.GetKeyDown(KeyCode.K)) SelectedBlankLetter('K');
			if (Input.GetKeyDown(KeyCode.L)) SelectedBlankLetter('L');
			if (Input.GetKeyDown(KeyCode.M)) SelectedBlankLetter('M');      
			if (Input.GetKeyDown(KeyCode.N)) SelectedBlankLetter('N');
			if (Input.GetKeyDown(KeyCode.O)) SelectedBlankLetter('O');
			if (Input.GetKeyDown(KeyCode.P)) SelectedBlankLetter('P');
			if (Input.GetKeyDown(KeyCode.Q)) SelectedBlankLetter('Q');      
			if (Input.GetKeyDown(KeyCode.R)) SelectedBlankLetter('R');
			if (Input.GetKeyDown(KeyCode.S)) SelectedBlankLetter('S');
			if (Input.GetKeyDown(KeyCode.T)) SelectedBlankLetter('T');
			if (Input.GetKeyDown(KeyCode.U)) SelectedBlankLetter('U');      
			if (Input.GetKeyDown(KeyCode.V)) SelectedBlankLetter('V');
			if (Input.GetKeyDown(KeyCode.W)) SelectedBlankLetter('W');
			if (Input.GetKeyDown(KeyCode.X)) SelectedBlankLetter('X');
			if (Input.GetKeyDown(KeyCode.Y)) SelectedBlankLetter('Y');     
			if (Input.GetKeyDown(KeyCode.Z)) SelectedBlankLetter('Z');			
		}
		
 	}	
	
	// if delete or backpsace is pressed remove last placed tile		
	void RemoveLastTile() {
		if (workingTiles.Count>0) {
			
			Transform tileToRemove = workingTiles[workingTiles.Count-1];
			
			if (tileToRemove.GetComponent<TileBehavior>().glyph=='*') {
				tileToRemove.GetComponent<TileBehavior>().RevertBlank();	
			}	
			
			shelf.PutBackTile(tileToRemove);
			tileToRemove.GetComponent<TileBehavior>().placedOnBoard=false;
			workingTiles.Remove(tileToRemove);
		//	board.CalculateValidSpaces();
		}	
			
	}
	
	public void RemoveClickedTile(Transform tileToRemove) {
			shelf.PutBackTile(tileToRemove);
			workingTiles.Remove(tileToRemove);
			tileToRemove.GetComponent<TileBehavior>().placedOnBoard=false;
		//	board.CalculateValidSpaces();
	}	
			
			
	// passing the keyboard select of tiles to the shelf
	void KeyBoardSelect(char selectedLetter) {
		shelf.KeyboardSelect(selectedLetter);
	}
	
	// activating on a space click
	public void SpaceClick(Transform clickedSpace) {
		SpaceBehavior clickedSpaceBehavior=clickedSpace.GetComponent<SpaceBehavior>();
		
		//if there is a tile selected AND the space clicked is valid then move the tile to the space, add it to the working tiles list, recalculate valid spaces
		//no longer constraining to only valid spaces, checking all placed tiles for validity instead
		//added check to see if there is already an existing tile in the space
		if (selectedTile && (!(clickedSpaceBehavior.hasTile))) {
			//if (board.CheckIfSpaceIsValid(clickedSpace)) {
				if (selectedTile.GetComponent<TileBehavior>().glyph=='*') {
					FillBlank(selectedTile);
				}
			
			
				clickedSpaceBehavior.PlaceTile(selectedTile);			
				workingTiles.Add(selectedTile);
			//	board.CalculateValidSpaces();
				selectedTile=null;
			//}
		}	
	}		

	void OnGUI () {
		
		if (pickingBlank) {
			// Make a group on the center of the screen
				GUI.BeginGroup (new Rect (Screen.width*.5f - 50, Screen.height*.5f - 50, Screen.width*.5f, Screen.height*.5f));
				// All rectangles are now adjusted to the group. (0,0) is the topleft corner of the group.

				// We'll make a box so you can see where the group is on-screen.
				GUI.Box (new Rect (0,0,310,310),"Choose a letter for your blank tile");
				if (GUI.Button (new Rect (10,30,40,40), "A")) SelectedBlankLetter('A');
				if (GUI.Button (new Rect (60,30,40,40), "B")) SelectedBlankLetter('B');
				if (GUI.Button (new Rect (110,30,40,40), "C")) SelectedBlankLetter('C');
				if (GUI.Button (new Rect (160,30,40,40), "D")) SelectedBlankLetter('D');
				if (GUI.Button (new Rect (210,30,40,40), "E")) SelectedBlankLetter('E');
				if (GUI.Button (new Rect (260,30,40,40), "F")) SelectedBlankLetter('F');
				if (GUI.Button (new Rect (10,80,40,40), "G")) SelectedBlankLetter('G');
				if (GUI.Button (new Rect (60,80,40,40), "H")) SelectedBlankLetter('H');
				if (GUI.Button (new Rect (110,80,40,40), "I")) SelectedBlankLetter('I');
				if (GUI.Button (new Rect (160,80,40,40), "J")) SelectedBlankLetter('J');
				if (GUI.Button (new Rect (210,80,40,40), "K")) SelectedBlankLetter('K');
				if (GUI.Button (new Rect (260,80,40,40), "L")) SelectedBlankLetter('L');
				if (GUI.Button (new Rect (10,130,40,40), "M")) SelectedBlankLetter('M');
				if (GUI.Button (new Rect (60,130,40,40), "N")) SelectedBlankLetter('N');
				if (GUI.Button (new Rect (110,130,40,40), "O")) SelectedBlankLetter('O');
				if (GUI.Button (new Rect (160,130,40,40), "P")) SelectedBlankLetter('P');
				if (GUI.Button (new Rect (210,130,40,40), "Q")) SelectedBlankLetter('Q');
				if (GUI.Button (new Rect (260,130,40,40), "R")) SelectedBlankLetter('R');
				if (GUI.Button (new Rect (10,180,40,40), "S")) SelectedBlankLetter('S');
				if (GUI.Button (new Rect (60,180,40,40), "T")) SelectedBlankLetter('T');
				if (GUI.Button (new Rect (110,180,40,40), "U")) SelectedBlankLetter('U');
				if (GUI.Button (new Rect (160,180,40,40), "V")) SelectedBlankLetter('V');
				if (GUI.Button (new Rect (210,180,40,40), "W")) SelectedBlankLetter('W');
				if (GUI.Button (new Rect (260,180,40,40), "X")) SelectedBlankLetter('X');
				if (GUI.Button (new Rect (10,230,40,40), "Y")) SelectedBlankLetter('Y');
				if (GUI.Button (new Rect (60,230,40,40), "Z")) SelectedBlankLetter('Z');				
				// End the group we started above. This is very important to remember!
				GUI.EndGroup ();		
		}
		
		if (endGameDialog) {
			GUI.BeginGroup (new Rect (Screen.width*.5f - 50, Screen.height*.5f - 50, Screen.width*.5f, Screen.height*.5f));	
				GUI.Box (new Rect (0,0,310,310),"Your current score is "+totalScore+"\nThere are "+bag.transform.childCount+" tiles left in the bag.\n\n" +
					"Are you sure you want to end your game?");
				if (GUI.Button (new Rect (100,100,100,40), "End Game")) GameOver();
				if (GUI.Button (new Rect (100,200,100,40), "Continue")) EndGameCancel();
			
			GUI.EndGroup ();
		}
		
		if (gameOverConfirmed) {
			//apparently you can't use the input functions with gui text boxes.....have to use the event in the gui before the dialog box is drawn
			if ((playerName!="")&&(Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return)))	{
				//this doesn't make sense, need to fix
				gameOverConfirmed=false;
				inHighScores=true;
				GetHighScores();
				
			}	
			
			GUI.BeginGroup (new Rect (Screen.width*.5f - 50, Screen.height*.5f - 50, Screen.width*.5f, Screen.height*.5f));
				GUI.Label (new Rect (10, 10, 400, 30), "Type your name then hit enter to see your high score list:");
				playerName = GUI.TextField (new Rect (25, 60, 100, 30), playerName);
			GUI.EndGroup ();
		}	
		
		
		if (inHighScores) {
			GUI.BeginGroup (new Rect (Screen.width*.5f - 50, Screen.height*.2f - 50, Screen.width*.7f, Screen.height*.9f));
				GUI.Label (new Rect (0,0,Screen.height*.4f - 50,Screen.height*.7f),"This game you scored "+totalScore+"\nPlease wait while the server loads the Top 10 scores for "+playerName+":");
				if(splitHighScore[0]!=null) {
					for(int i=1;i<11;i++) {
						if(splitHighScore[i]!=null) {
							GUI.Label (new Rect (10, 25*i+75, 400, 20), (i)+". "+splitHighScore[i*4]+" "+splitHighScore[(i*4)+1]);
						}
					}
				}
			GUI.EndGroup();
		}	
		
		
	}
	
	void GetHighScores() {
		StartCoroutine(ScoreServerConnect());
		
		
				
	}	
	
	IEnumerator ScoreServerConnect() {
		/*Debug.Log(DateTime.Now.Year+"-"+
			DateTime.Now.Month+"-"+
			DateTime.Now.Day);*/
		
		
		string url="http://nodj.host-ed.me/solo-word-builder/solo-word-builder-highscore.php?Name="+playerName+
			"&Score="+totalScore+
			"&Year="+DateTime.Now.Year.ToString("0000")+
			"&Month="+DateTime.Now.Month.ToString("00")+
			"&Day="+DateTime.Now.Day.ToString("00")+
			"&Hour="+DateTime.Now.Hour.ToString("00")+
			"&Minute="+DateTime.Now.Minute.ToString("00");
		
		
		//Debug.Log(url);
		WWW www = new WWW(url);
        yield return www;
		Debug.Log(www.text);
		string[] stringSeparators = new string[] {"<br/>"," "};
		
		splitHighScore = www.text.Split(stringSeparators, StringSplitOptions.None);
    }	
	
	
	//after the game over is confirmed
	void GameOver() {
		gameOverConfirmed=true;
		//http://nodj.host-ed.me/solo-word-builder/solo-word-builder-highscore.php?Name=chode&Score=245&Year=1328&Month=08&Day=08&Hour=08&Minute=08
		endGameDialog=false;
		
	}
	
	//functions to control ending the game
	public void EndGameConfirm() {
		blur.active=true;
		endGameDialog=true;
	}	
	
	//put things back if game over is cancelled
	public void EndGameCancel() {
		blur.active=false;
		endGameDialog=false;
	}	
	
	
	//fill in a blank tile, need to do it here so I don't do 100 ongui draws each time the blank is active
	
	//turns on the overlay to prevent any game stuff from being selected and also the letter select GUI elements
	void FillBlank(Transform selectedTile) {
		blur.active=true;
		pickingBlank=true;
			
	}
	
	void SelectedBlankLetter(char selectedLetter) {
		pickingBlank=false;
		blur.active=false;
		//the blank tile will be the last one whenever this is called
		workingTiles[workingTiles.Count-1].GetComponent<TileBehavior>().BlankTileFill(selectedLetter);
	}
	
	//method to remove all of the remaining tiles on the board
	public void RemoveAllUnscoredTiles() {
		
		//iterate over the number of tiles that are placed and pop them off the list in order
		for(int i = workingTiles.Count;i>0;i--) {
			//one off array index is screwing me up
			shelf.PutBackTile(workingTiles[i-1]);
			workingTiles[i-1].GetComponent<TileBehavior>().placedOnBoard=false;
			workingTiles.RemoveAt(i-1);
			
		}			
	}
}
