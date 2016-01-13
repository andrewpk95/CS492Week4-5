using UnityEngine;
using System.Collections;

public class ErrorMessageScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine (QuitApplication ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator QuitApplication() {
		yield return new WaitForSeconds (2);
		Destroy (this.transform.gameObject);
		Application.Quit ();
	}
}
