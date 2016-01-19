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
		percentage = 0f;
		fighter = GetComponentInParent<Fighter> ();
		player = fighter.getPlayer ();
		UIPercentage[] UIs = FindObjectsOfType<UIPercentage> ();
		Debug.Log (UIs.Length);
		for (int i = 0; i < UIs.Length; i++) {
			if (UIs [i].player == player) {
				UI = UIs [i];
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		write ();
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (fighter.getName () + " left the zone!");
			fighter.Die ();
			Reset ();
		}
	}

	void OnTriggerExit2D (Collider2D col) {
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (fighter.getName () + " entered the battlefield");
			fighter.Revive ();
		}
	}

	public void Reset() {
		percentage = 0f;
	}

	public void TakeDamage(float damage) {
		percentage += damage;
		Clamp ();
	}

	public void HitStun(float duration) {
		fighter.HitStun (duration);
	}

	public void Launch(Vector2 direction, float strength) {
		fighter.Launch (direction, strength);
	}

	public void Heal(float heal) {
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
