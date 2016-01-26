using UnityEngine;
using System.Collections;

public class ResultLogic : MonoBehaviour {

	public ResultContainer container;

	// Use this for initialization
	void Start () {
		container = FindObjectOfType<ResultContainer> ();

	}
	
	// Update is called once per frame
	void Update () {
		UIWinner.write (container.Winner);
	}
}
