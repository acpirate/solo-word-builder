using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class GameRules : MonoBehaviour {
	
	public int totalLetters;
	
	public static List<String> dictionary;
	static GameRules gameRules;
	
	
	void Awake() {
		dictionary=new List<String>();
		
		if (!(gameRules)) {
			gameRules=GameObject.Find("Game").GetComponent<GameRules>();
		}	
		
		//open the dictionary data file
		FileInfo dictionaryData = new FileInfo ("Assets\\Resources\\Rules\\dictionary.txt");	
		StreamReader dictionaryDataReader = dictionaryData.OpenText();
		//read each line and add the results to the dictionary list
		while (dictionaryDataReader.Peek()>=0) {		
			GameRules.dictionary.Add(dictionaryDataReader.ReadLine());	
		}	
		dictionaryDataReader.Close();
		//Debug.Log(dictionary.Count.ToString());
	}	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
