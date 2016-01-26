using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PlayerChoose : NetworkBehaviour {

	public GameObject playerInput;

	public Player player;
	public PlayerInfoManager infoManager;
	public bool dontDestroyOnLoad;
	// Use this for initialization
	void Start () {
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad (this.gameObject);
		}
		if (!isLocalPlayer)
			return;
		CharacterUIManager UI = FindObjectOfType<CharacterUIManager> ();
		UI.playerChoose = this;

		CmdAdd ();
	}

	[Command]
	void CmdAdd() {
		infoManager = FindObjectOfType<PlayerInfoManager> ();
		player = infoManager.Register ("Haha");
		this.gameObject.name = player.ToString ();
		RpcAdd (player);
	}

	[ClientRpc]
	void RpcAdd(Player pl) {
		player = pl;
	}

	public void OnCharacterChoose(string n) {
		CmdChoose (n);
	}

	[Command]
	void CmdChoose(string n) {
		infoManager.ChooseCharacter (player, n);
	}

	void OnLevelWasLoaded(int i) {
		if (i == 1) {
			CmdReplace ();
		}
	}

	[Command]
	void CmdReplace() {
		GameObject go = (GameObject)Instantiate (playerInput);
		NetworkServer.Spawn (go);
		NetworkServer.ReplacePlayerForConnection (connectionToClient, go, playerControllerId);
	}
}
