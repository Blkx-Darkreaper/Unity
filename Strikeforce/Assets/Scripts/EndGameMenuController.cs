using UnityEngine;
using System.Collections.Generic;
using Strikeforce;

public class EndGameMenuController: MenuController {

	protected Player[] winningTeam { get; set; }
	protected VictoryCondition winCondition { get; set; }
	protected struct Message {
		public const string NEW_GAME = "New Game";
		public const string MAIN_MENU = "Main Menu";
		public const string GAME_OVER = "Game Over";
	}

	protected void DrawMenu() {}

	public void Victory(VictoryCondition endCondition) {}
}