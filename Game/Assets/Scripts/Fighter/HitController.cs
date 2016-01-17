using UnityEngine;
using System.Collections;

public class HitController : MonoBehaviour {

	Fighter fighter;

	// Use this for initialization
	void Start () {
		fighter = GetComponentInParent<Fighter> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (fighter.getName () + " left the zone!");
			fighter.Die ();
		}
	}

	void OnTriggerExit2D (Collider2D col) {
		if (col.gameObject.tag == "BlastZone") {
			Debug.Log (fighter.getName () + " entered the battlefield");
			fighter.Revive ();
		}
	}
}
