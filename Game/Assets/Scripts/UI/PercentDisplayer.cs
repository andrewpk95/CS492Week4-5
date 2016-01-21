using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PercentDisplayer : NetworkBehaviour {
	
	//Components
	Fighter fighter;
	Player player;
	public HitController hit;
	UIPercentage UI;

	// Use this for initialization
	void Start () {
		fighter = GetComponent<Fighter> ();
		player = fighter.getPlayer ();
		UIPercentage[] UIs = FindObjectsOfType<UIPercentage> ();
		Debug.Log (player.ToString());
		for (int i = 0; i < UIs.Length; i++) {
			if (UIs [i].player == player) {
				UI = UIs [i];
				break;
			}
		}
		hit = GetComponentInChildren<HitController> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isServer)
			return;
		Player pl = player;
		string charName = fighter.getCharacterName ();
		float per = hit.percentage;
		RpcWrite (pl, charName, per);
	}

	[ClientRpc]
	void RpcWrite(Player pl, string charName, float per) {
		hit.percentage = per;
		UI.write (pl, charName, per);
	}
}
