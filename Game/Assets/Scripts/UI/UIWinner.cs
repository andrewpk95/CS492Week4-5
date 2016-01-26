using UnityEngine;
using System.Collections;

public class UIWinner : MonoBehaviour {

	static UILabel label;

	// Use this for initialization
	void Start () {
		label = GetComponent<UILabel> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void write(string n) {
		string text = "The Winner Is " + n + "!";
		label.text = text;
	}
}
