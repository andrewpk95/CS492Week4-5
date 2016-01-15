using UnityEngine;
using System.Collections;

public class UITime : MonoBehaviour {

	static UILabel label;

	// Use this for initialization
	void Start () {
		label = GetComponent<UILabel> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void write(float time) {
		int minute = (int)time / 60;
		time %= 60;
		string text = minute.ToString ("D2") + ":" + time.ToString ("00.000");
		label.text = text;
	}
}
