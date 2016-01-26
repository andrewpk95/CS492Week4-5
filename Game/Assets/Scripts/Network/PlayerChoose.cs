using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerChoose : NetworkBehaviour {

	public Player player;
	public PlayerInfoManager infoManager;

	// Use this for initialization
	void Start () {
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

	// Update is called once per frame
	void Update () {
		
	}
}
