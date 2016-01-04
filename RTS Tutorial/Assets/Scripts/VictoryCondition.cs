using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class VictoryCondition : MonoBehaviour {

    public PlayerController[] activePlayers { get; set; }
    protected List<PlayerController> winningPlayers { get; set; }

    public virtual bool IsGameOver()
    {
        if (activePlayers == null)
        {
            return true;
        }
        if (activePlayers.Length == 0)
        {
            return true;
        }

        winningPlayers = new List<PlayerController>();

        foreach (PlayerController player in activePlayers)
        {
            bool conditionsMet = HasPlayerMetWinConditions(player);
            if (conditionsMet == false)
            {
                continue;
            }

            winningPlayers.Add(player);
            return true;
        }

        return false;
    }

    public PlayerController GetWinningPlayer()
    {
        if (winningPlayers == null)
        {
            return null;
        }
        if (winningPlayers.Count == 0)
        {
            return null;
        }

        PlayerController winner = winningPlayers[0];
        return winner;
    }

    public PlayerController[] GetWinningTeam()
    {
        if (winningPlayers == null)
        {
            return null;
        }
        if (winningPlayers.Count == 0)
        {
            return null;
        }

        return winningPlayers.ToArray();
    }

    public abstract string GetDescription();

    public abstract bool HasPlayerMetWinConditions(PlayerController player);
}
