using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResultContainer : MonoBehaviour {

	public bool dontDestroyOnLoad;

	public string Winner;

	public Dictionary<Player, int> result;
	public int p1;
	public int p2;
	public int p3;
	public int p4;

	// Use this for initialization
	void Start () {
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad (this.gameObject);
		}

		result = new Dictionary<Player, int> ();
	}
	
	// Update is called once per frame
	void Update () {
		result.TryGetValue (Player.Player1, out p1);
		result.TryGetValue (Player.Player2, out p2);
		result.TryGetValue (Player.Player3, out p3);
		result.TryGetValue (Player.Player4, out p4);
	}

	public void Add(Player pl) {
		result.Add (pl, 0);
	}

	public void Increment(Player pl) {
		result [pl]++;
	}

	public void SetWinner(string n) {
		Winner = n;
	}
}
