using System;
using UnityEngine;

public interface Fighter {

	void SetInput (PlayerInput input);

	void SetPlayer (Player pl);

	void SetLocalClient (bool local);

	void HitStun(float duration);

	void ShieldStun(float duration);

	void Launch(Vector2 direction, float strength);

	void Die();

	void Revive();

	string getName();

	void setName(string name);

	Transform getTransform ();

	Player getPlayer();

	string getCharacterName();

	bool facingRight ();

}

