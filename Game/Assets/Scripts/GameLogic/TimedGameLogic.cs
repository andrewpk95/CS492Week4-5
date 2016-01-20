using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimedGameLogic : NetworkBehaviour, GameLogic {

	//Constants
	public const float RESPAWN_TIME = 1;
	public static Vector3 SPAWN_POSITION;

	//Time
	public float timeLength = 300f;
	public float startTime;
	[SyncVar]
	public float remainingTime;

	// Use this for initialization
	void Start () {
		SPAWN_POSITION = new Vector3 (0f, 0.5f, 0f);

		startTime = Time.time;
		remainingTime = timeLength;
	}

	// Update is called once per frame
	void Update () {
		//Tick down the time
		UITime.write (remainingTime);
		if (!isServer)
			return;
		remainingTime -= Time.deltaTime;
		if (remainingTime < 0) {
			UITime.write (0f);
			OnGameOver ();
		}
	}

	public void OnPlayerDeath(GameObject player) {
		Debug.Log (player.name + " died! Respawning in 1 second...");
		StartCoroutine (Respawn (player));
	}

	public IEnumerator Respawn(GameObject player) {
		player.GetComponent<Fighter> ().Revive ();
		player.SetActive (false);
		yield return new WaitForSeconds (RESPAWN_TIME);
		player.transform.position = SPAWN_POSITION;
		player.SetActive (true);
	}

	public void OnGameOver() {
		//Do something when time is over
		SceneManager.LoadScene ("ResultScene");
	}
}
