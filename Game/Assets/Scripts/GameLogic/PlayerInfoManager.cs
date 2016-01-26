using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerInfoManager : NetworkBehaviour {

	public bool dontDestroyOnLoad;

	[SyncVar] public int numPlayer;

	PlayerInfo Player1;
	PlayerInfo Player2;
	PlayerInfo Player3;
	PlayerInfo Player4;

	public FighterSelectionUI UI1;
	public FighterSelectionUI UI2;
	public FighterSelectionUI UI3;
	public FighterSelectionUI UI4;

	public Dictionary<PlayerInfo, bool> plInfo;
	Dictionary<Player, FighterSelectionUI> UIs;

	// Use this for initialization
	void Start () {
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad (this.gameObject);
		}
		/*
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");

		AndroidJavaObject intent = context.Call<AndroidJavaObject>("getIntent");
		string value = intent.Call<string>("getStringExtra", "username");
		Debug.Log("Unity user name : " + value);
		*/

		UIs = new Dictionary<Player, FighterSelectionUI> ();
		UIs.Add (Player.Player1, UI1);
		UIs.Add (Player.Player2, UI2);
		UIs.Add (Player.Player3, UI3);
		UIs.Add (Player.Player4, UI4);
		plInfo = new Dictionary<PlayerInfo, bool> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ChooseCharacter(Player pl, string n) {
		PlayerInfo plInfo = Get (pl);
		plInfo.ChooseCharacter (n);
		CmdChooseCharacter (pl);
	}

	[Command]
	void CmdChooseCharacter(Player pl) {
		UIs [pl].SetSprite (SpriteImg.Mario);
		RpcChooseCharacter (pl);
	}

	[ClientRpc]
	void RpcChooseCharacter(Player pl) {
		UIs [pl].SetSprite (SpriteImg.Mario);
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
			plInfo.Add (Player1, false);
			numPlayer++;
			return Player.Player1;
		}
		if (numPlayer == 1) {
			Player2 = new PlayerInfo (Player.Player2, playerName);
			plInfo.Add (Player2, false);
			numPlayer++;
			return Player.Player2;
		}
		if (numPlayer == 2) {
			Player3 = new PlayerInfo (Player.Player3, playerName);
			plInfo.Add (Player3, false);
			numPlayer++;
			return Player.Player3;
		}
		if (numPlayer == 3) {
			Player4 = new PlayerInfo (Player.Player4, playerName);
			plInfo.Add (Player4, false);
			numPlayer++;
			return Player.Player4;
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
		Debug.Log (player.ToString () + " chose " + characterName);
	}
}
