using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HitController : NetworkBehaviour {

	//Attributes
	[SyncVar]
	public float percentage;

	//Components
	Fighter fighter;
	Player player;
	UIPercentage UI;

	// Use this for initialization
	void Start () {
		if (!isLocalPlayer)
			return;
		percentage = 0f;
		fighter = GetComponentInParent<Fighter> ();
		player = fighter.getPlayer ();
		UIPercentage[] UIs = FindObjectsOfType<UIPercentage> ();
		Debug.Log (player.ToString());
		for (int i = 0; i < UIs.Length; i++) {
			if (UIs [i].player == player) {
				UI = UIs [i];
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
			return;
		write ();
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (!isServer)
			return;
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (fighter.getName () + " left the zone!");
			fighter.Die ();
			Reset ();
		}
	}

	void OnTriggerExit2D (Collider2D col) {
		if (!isServer)
			return;
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (fighter.getName () + " entered the battlefield");
			fighter.Revive ();
		}
	}

	public void Reset() {
		if (!isServer)
			return;
		percentage = 0f;
	}

	public void TakeDamage(float damage) {
		if (!isServer)
			return;
		percentage += damage;
		Clamp ();
	}

	public void HitStun(float duration) {
		if (!isServer)
			return;
		fighter.HitStun (duration);
	}

	public void Launch(Vector2 direction, float strength) {
		if (!isServer)
			return;
		fighter.Launch (direction, strength);
	}

	public void Heal(float heal) {
		if (!isServer)
			return;
		percentage -= heal;
		Clamp ();
	}

	void Clamp() {
		percentage = Mathf.Clamp (percentage, 0f, 999f);
	}

	public float GetPercentage() {
		return percentage;
	}

	void write() {
		UI.write (player, fighter.getCharacterName (), percentage);
	}
}
