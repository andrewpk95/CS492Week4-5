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
		RpcDraw (scale);
	}

	[ClientRpc]
	void RpcDraw(float scale) {
		shield.transform.localScale = new Vector3 (1.5f * scale, 1.5f * scale, 1f);
	}
}
