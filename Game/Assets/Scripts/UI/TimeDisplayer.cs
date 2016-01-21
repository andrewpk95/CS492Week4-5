using UnityEngine;
using System.Collections;

public class TimeDisplayer : MonoBehaviour {

	TimedGameLogic gameLogic;

	// Use this for initialization
	void Start () {
		gameLogic = GetComponent<TimedGameLogic> ();
	}
	
	// Update is called once per frame
	void Update () {
		UITime.write (gameLogic.remainingTime);
	}
}
