using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FighterController : NetworkBehaviour {

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
}
