using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SelectionLogic : MonoBehaviour {

	public GameObject readytofight;
	public GameObject nextButton;

	public PlayerInfoManager info;

	// Use this for initialization
	void Start () {
		//info = FindObjectOfType<PlayerInfoManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (info.numPlayer > 0) {
			OnReadyToFight ();
		}
	}

	public void OnReadyToFight() {
		readytofight.SetActive (true);
		nextButton.SetActive (true);
	}

	public void NextScene() {
		SceneManager.LoadScene ("GameScene");
	}
}
