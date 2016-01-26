using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MyNetworkManager : NetworkManager {

	public override void OnStartHost ()
	{
		base.OnStartHost ();
		GameInputManager playerInput = FindObjectOfType<GameInputManager> ();
		playerInput.isHost = true;
	}
}
