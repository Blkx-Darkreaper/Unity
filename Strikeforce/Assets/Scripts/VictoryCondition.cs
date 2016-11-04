using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public abstract class VictoryCondition : MonoBehaviour
    {
        public Player[] activePlayers { get; set; }
        protected List<Player> winningPlayers { get; set; }

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

            winningPlayers = new List<Player>();

            foreach (Player player in activePlayers)
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

        public Player GetWinningPlayer()
        {
            if (winningPlayers == null)
            {
                return null;
            }
            if (winningPlayers.Count == 0)
            {
                return null;
            }

            Player winner = winningPlayers[0];
            return winner;
        }

        public Player[] GetWinningTeam()
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

        public abstract bool HasPlayerMetWinConditions(Player player);
    }
}