using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class TimedGameLogic : NetworkBehaviour, GameLogic {

	public ServerManager server;
	public ResultContainer result;

	//Constants
	public const float RESPAWN_TIME = 1;
	public static Vector3 SPAWN_POSITION;

	//Time
	public float timeLength = 300f;
	public float startTime;
	//[SyncVar]
	public float remainingTime;

	// Use this for initialization
	void Start () {
		server = FindObjectOfType<ServerManager> ();
		result = FindObjectOfType<ResultContainer> ();

		SPAWN_POSITION = new Vector3 (0f, 0.5f, 0f);

		startTime = Time.time;
		remainingTime = timeLength;
	}

	// Update is called once per frame
	void Update () {
		//Tick down the time
		if (!isServer)
			return;
		remainingTime -= Time.deltaTime;
		if (remainingTime < 0) {
			remainingTime = 0;
			OnGameOver ();
		}
	}

	public void OnPlayerDeath(GameObject player) {
		Debug.Log (player.name + " died! Respawning in 1 second...");
		result.Increment (player.GetComponent<Fighter> ().getPlayer ());
		Respawn (player);
	}
		
	//[ClientRpc]
	public void Respawn(GameObject player) {
		if (!isServer)
			return;
		player.GetComponent<Fighter> ().Revive ();
		player.SetActive (false);
		//yield return new WaitForSeconds (RESPAWN_TIME);
		player.transform.position = SPAWN_POSITION;
		player.SetActive (true);
	}

	public void OnGameOver() {
		//Do something when time is over
		var min = result.result.OrderBy(kvp => kvp.Value).First();
		var minKey = min.Key;

		result.SetWinner(
			PlayerContainer.Get (minKey).getName ());
		SceneManager.LoadScene ("ResultScene");
	}
}
