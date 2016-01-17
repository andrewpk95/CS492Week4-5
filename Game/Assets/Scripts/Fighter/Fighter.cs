using System;
using UnityEngine;

public interface Fighter {

	void HitStun(float duration);

	void Launch(Vector2 direction, float strength);

	void Die();

	void Revive();

	string getName();

	void setName(string name);

	bool facingRight ();

}

