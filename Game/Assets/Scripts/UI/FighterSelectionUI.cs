using UnityEngine;
using System.Collections;

public enum SpriteImg {
	Mario
}

public class FighterSelectionUI : MonoBehaviour {

	SpriteRenderer sprite;

	public Sprite MarioImg;

	// Use this for initialization
	void Start () {
		sprite = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetSprite(SpriteImg img) {
		if (img == SpriteImg.Mario) {
			sprite.sprite = MarioImg;
		}
	}
}
