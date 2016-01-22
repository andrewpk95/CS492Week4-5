using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ShieldDisplayer : NetworkBehaviour {

	public ShieldController controller;
	public GameObject shield;
	// Use this for initialization
	void Start () {
		controller = GetComponentInChildren<ShieldController> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isServer)
			return;
		float scale = Mathf.Sqrt(controller.health / controller.ShieldHealth);
		RpcDraw (controller.health, scale);
	}

	[ClientRpc]
	void RpcDraw(float health, float scale) {
		controller.health = health;
		shield.transform.localScale = new Vector3 (scale, scale, 1f);
	}
}
