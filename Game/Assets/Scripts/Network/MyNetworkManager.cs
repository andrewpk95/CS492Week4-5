using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MyNetworkManager : NetworkManager {

	public GameObject gameLogic;
	public GameObject container;
	public IsitHost IsHost;

	public override void OnStartHost ()
	{
		base.OnStartHost ();
		IsHost = FindObjectOfType<IsitHost> ();
		IsHost.isHost = true;
	}

	void OnLevelWasLoaded(int i) {
		if (i == 1) {
			GameObject go1 = (GameObject)Instantiate (gameLogic);
			NetworkServer.Spawn (go1);
			GameObject go2 = (GameObject)Instantiate (container);
			NetworkServer.Spawn (go2);
			GameInputManager playerInput = FindObjectOfType<GameInputManager> ();
			if (playerInput != null)
				playerInput.isHost = true;
		}
	}
}
