using UnityEngine;
using System.Collections;

public class ConnectionHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnConnectedToServer() {
		Debug.Log ("Connected to server!");
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		Debug.Log ("Disconnected from server: " + info);
	}
}
