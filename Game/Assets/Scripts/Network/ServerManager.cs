using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocketIO;

public class ServerManager : MonoBehaviour {
	
	public bool dontDestroyOnLoad = false;
	bool isConnected = false;

	//public GameObject ErrorMessage;

	private SocketIOComponent socket;
	public NetworkManager network;

	//GameManager gameManager;

	void Start() {
		//Makes this object stay throughout the entire scene
		if (dontDestroyOnLoad) {
			DontDestroyOnLoad (transform.gameObject);
		}
		//Find the Socket Component in the Game Scene
		GameObject go = GameObject.Find("SocketIO");
		socket = go.GetComponent<SocketIOComponent>();

		//Register what kind of messages the client receives
		socket.On ("ConnectionTest", NetTest);
		socket.On ("JoinRoom", NetLogin);
		//socket.On ("Response", NetResponse);
		socket.Connect ();

		//Start Connection Test
		if (socket.IsConnected) {
			//Debug.Log ("Connected to Server!");
			StartCoroutine("ConnectionTest");
		} else {
			Debug.Log ("Not Connected...");
		}

	}

	//This function is called when the server responds to Connection Test. 
	//The client stops pinging the server and enables the main Game Logic. 
	public void NetTest(SocketIOEvent socketEvent) {
		//Process Data from the Server.
		isConnected = true;
		string data = socketEvent.data.ToString ();
		Debug.Log ("Response from server: " + data);
		StopCoroutine ("ConnectionTest");

		//Enable Game Logic in the Scene. 
		//GameObject go = GameObject.FindGameObjectWithTag ("GameController");
		//gameManager = go.GetComponent<GameManager> ();
		//((MonoBehaviour)gameManager).enabled = true;
		socket.Emit ("JoinRoom");
	}

	public void NetLogin(SocketIOEvent socketEvent) {
		JSONObject data = socketEvent.data;
		Debug.Log ("Response from server: " + data.ToString ());
		bool isHost = data.GetField ("isHost").b;
		if (isHost) {
			string hostIP = data.GetField("HostIP").str;
			string debug = "You are the new host: " + hostIP;
			Debug.Log (debug);
			UIDebugScript.write (debug);
			network.StartHost ();
		} else {
			string hostIP = data.GetField("HostIP").str;
			string clientIP = data.GetField("ClientIP").str;
			string debug = "You are the client " + clientIP + " connected to " + hostIP;
			Debug.Log (debug);
			UIDebugScript.write (debug);
			network.networkAddress = hostIP;
			network.StartClient ();
		}
	}
	/*
	//This function is called when the Server responds to a Request. 
	//The Response is sent to the current Scene's GameManager to handle it. 
	public void NetResponse(SocketIOEvent socketEvent) {
		string data = socketEvent.data.ToString ();
		Debug.Log ("Response from server: " + data);
		//gameManager.handleResponse (data);
	}
	*/	

	//This function sends pings "abcd" to the Server until the Server Responds. 
	private IEnumerator ConnectionTest()
	{
		int waitTime = 50;
		while (waitTime > 0) {
			JSONObject data = new JSONObject ();
			data.AddField ("Save", "abcd");
			socket.Emit ("ConnectionTest", data);
			Debug.Log ("ConnectionTest abcd");
			yield return new WaitForSeconds (0.1f);
			yield return null;
			waitTime--;
		}
		if (!isConnected) {
			Debug.Log("Can't connect to server...");
			//Instantiate (ErrorMessage, Vector3.zero, Quaternion.identity);
		}
	}
	/*
	//Call this function from the GameManager if you want to update the current PokeDama information. 
	public void RequestSave(PokeDama pokedama) {
		string jsonString = JsonUtility.ToJson (pokedama);
		JSONObject jsonPoke = new JSONObject (jsonString);
		Debug.Log (jsonPoke.ToString ());
		JSONObject json = new JSONObject ();
		json.AddField ("RequestType", "Save");
		json.AddField ("PokeDama", jsonPoke);
		Debug.Log (json.ToString());
		socket.Emit ("Request", json);
	}

	//Call this function from the GameManager if you want to find PokeDama that matches the given IMEI. 
	public void RequestData(string IMEI) {
		Dictionary<string, string> data = new Dictionary<string, string> ();
		data ["RequestType"] = "FindByIMEI";
		data ["IMEI"] = IMEI;
		socket.Emit ("Request", new JSONObject (data));
	}

	//Call this function from the GameManager if you want to request data
	//about PokeDamas that are in 'range' of the given 'location'. 
	public void RequestData(GoogleMapLocation location, int range) {
		string jsonString = JsonUtility.ToJson (location);
		JSONObject jsonLocation = new JSONObject (jsonString);
		Debug.Log (jsonLocation.ToString ());
		JSONObject json = new JSONObject ();
		json.AddField ("RequestType", "FindByLocation");
		json.AddField ("Location", jsonLocation);
		json.AddField ("Range", range);
		Debug.Log (json.ToString());
		socket.Emit ("Request", json);
	}

	//Call this function from the GameManager if you want to create new PokeDama. 
	public void RequestCreation(PokeDama pokedama) {
		string jsonString = JsonUtility.ToJson (pokedama);
		JSONObject jsonPoke = new JSONObject (jsonString);
		Debug.Log (jsonPoke.ToString ());
		JSONObject json = new JSONObject ();
		json.AddField ("RequestType", "Create");
		json.AddField ("PokeDama", jsonPoke);
		Debug.Log (json.ToString());
		socket.Emit ("Request", json);
	}
	*/
}