using UnityEngine;
using System.Collections;

public class ConquestVictory : VictoryCondition {

    public override string GetDescription()
    {
        string description = string.Format("Conquer your opponents");
        return description;
    }

    public override bool IsGameOver()
    {
        if (activePlayers == null)
        {
            return true;
        }

        int playersRemaining = 0;
        foreach (PlayerController player in activePlayers)
        {
            bool playerStillAlive = HasPlayerMetWinConditions(player);
            if (playerStillAlive == false)
            {
                continue;
            }

            playersRemaining++;
            if (playersRemaining > 1)
            {
                return false;
            }
        }

        bool gameOver = playersRemaining <= 1;
        return gameOver;
    }

    public override bool HasPlayerMetWinConditions(PlayerController player)
    {
        if (player == null)
        {
            return false;
        }

        var stillAlive = !player.isDead;
        return stillAlive;
    }
}