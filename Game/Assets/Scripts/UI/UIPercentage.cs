using UnityEngine;
using System.Collections;

public class UIPercentage : MonoBehaviour {

	public Player player;
	UILabel label;

	// Use this for initialization
	void Start () {
		label = GetComponent<UILabel> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void write(Player pl, string characterName, float percentage) {
		if (pl != player)
			return;
		int p = (int) Mathf.Round (percentage);
		string text = characterName + "\n" + p.ToString () + "%";
		label.text = text;
	}
}
