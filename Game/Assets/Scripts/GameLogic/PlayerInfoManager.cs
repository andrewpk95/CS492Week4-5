using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerInfoManager : NetworkBehaviour {

	public bool dontDestroyOnLoad;

	[SyncVar] public int numPlayer;

	PlayerInfo Player1;
	PlayerInfo Player2;
	PlayerInfo Player3;
	PlayerInfo Player4;

	// Use this for initialization
	void Start () {
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad (this.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ChooseCharacter(Player pl, string n) {
		PlayerInfo plInfo = Get (pl);
		plInfo.ChooseCharacter (n);
	}

	public PlayerInfo Get(Player pl) {
		if (pl == Player.Player1)
			return Player1;
		if (pl == Player.Player2)
			return Player2;
		if (pl == Player.Player3)
			return Player3;
		if (pl == Player.Player4)
			return Player4;
		return null;
	}

	public void Add(PlayerInfo info) {
		
	}

	public Player Register(string playerName) {
		if (numPlayer == 0) {
			Player1 = new PlayerInfo (Player.Player1, playerName);
			numPlayer++;
			return Player.Player1;
		}
		if (numPlayer == 1) {
			Player2 = new PlayerInfo (Player.Player2, playerName);
			numPlayer++;
			return Player.Player2;
		}
		if (numPlayer == 2) {
			Player3 = new PlayerInfo (Player.Player3, playerName);
			numPlayer++;
			return Player.Player3;
		}
		if (numPlayer == 3) {
			Player4 = new PlayerInfo (Player.Player3, playerName);
			numPlayer++;
			return Player.Player3;
		}
		return Player.Null;
	}
}

public class PlayerInfo {

	public Player player;
	public string playerName;
	public string characterName;

	public PlayerInfo(Player pl, string plName, string charName) {
		player = pl;
		playerName = plName;
		characterName = charName;
	}

	public PlayerInfo(Player pl, string plName) {
		player = pl;
		playerName = plName;
	}

	public PlayerInfo(Player pl) {
		player = pl;
		playerName = pl.ToString ();
	}

	public void ChooseCharacter(string n) {
		characterName = n;
	}
}
