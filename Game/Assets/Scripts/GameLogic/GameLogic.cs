using UnityEngine;
using System.Collections;

public interface GameLogic {

	void OnPlayerDeath(GameObject player);

	void OnGameOver();

}
