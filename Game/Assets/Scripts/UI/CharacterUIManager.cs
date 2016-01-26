using UnityEngine;
using System.Collections;

public class CharacterUIManager : MonoBehaviour {

	public PlayerChoose playerChoose;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnCharacterClick(string n) {
		playerChoose.OnCharacterChoose (n);
	}
}
