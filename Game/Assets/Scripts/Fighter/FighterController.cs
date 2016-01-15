using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FighterController : NetworkBehaviour {

	public bool isDead;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
			return;

		float x = GameInputManager.JoystickPosition.x * Time.deltaTime;
		float y = GameInputManager.JoystickPosition.y * Time.deltaTime;

		transform.Translate(x, y, 0);
	}

	void OnTriggerEnter2D (Collider2D col) {
		Debug.Log ("Entered the battlefield");
	}

	void OnTriggerExit2D (Collider2D col) {
		if (col.gameObject.name == "BlastZone") {
			Debug.Log (this.name + " left the zone!");
			isDead = true;
		}
	}
}
