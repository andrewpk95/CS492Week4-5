using UnityEngine;
using System.Collections;

public class PlayerContainer {

	public static bool isFull;
	public static int numPlayers;

	public static Fighter Player1;
	public static Fighter Player2;
	public static Fighter Player3;
	public static Fighter Player4;

	static Fighter[] Players;

	public static Player Add(Fighter player) {
		if (Players == null) {
			Players = new Fighter[4];
			isFull = false;
			numPlayers = 0;
		}
		for (int i = 0; i < Players.Length; i++) {
			if (Players [i] == null) {
				Players [i] = player;
				numPlayers++;
				Update ();
				switch (i) {
				case 0:
					Player1 = player;
					return Player.Player1;
				case 1:
					Player2 = player;
					return Player.Player2;
				case 2:
					Player3 = player;
					return Player.Player3;
				case 3:
					Player4 = player;
					return Player.Player4;
				default:
					Player1 = player;
					return Player.Player1;
				}
			}
		}
		return Player.Null;
	}

	public static bool Remove(Player player) {
		if (player == Player.Null) {
			return false;
		}
		switch (player) {
		case Player.Player1:
			Player1 = null;
			break;
		case Player.Player2:
			Player2 = null;
			break;
		case Player.Player3:
			Player3 = null;
			break;
		case Player.Player4:
			Player4 = null;
			break;
		}
		numPlayers--;
		Update ();
		return true;
	}

	public static Fighter Get(Player player) {
		if (player == Player.Null) {
			return null;
		}
		switch (player) {
		case Player.Player1:
			return Player1;
		case Player.Player2:
			return Player2;
		case Player.Player3:
			return Player3;
		case Player.Player4:
			return Player4;
		}
		return null;
	}

	public static Fighter[] GetAll() {
		return Players;
	}

	static void Update() {
		if (numPlayers >= 4) {
			isFull = true;
		} else {
			isFull = false;
		}
	}
}
