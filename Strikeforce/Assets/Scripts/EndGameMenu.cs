using UnityEngine;
using System.Collections.Generic;

namespace Strikeforce
{
    public class EndGameMenu : Menu
    {
        protected Player[] winningTeam { get; set; }
        protected VictoryCondition winCondition { get; set; }
        protected struct Message
        {
            public const string NEW_GAME = "New Game";
            public const string MAIN_MENU = "Main Menu";
            public const string GAME_OVER = "Game Over";
        }

        protected override void DrawMenu() { }

        public void Victory(VictoryCondition endCondition) { }
    }
}