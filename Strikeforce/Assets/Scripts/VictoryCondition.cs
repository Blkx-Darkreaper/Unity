using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public abstract class VictoryCondition : MonoBehaviour
    {
        public Profile[] activePlayers { get; set; }
        protected List<Profile> winningPlayers { get; set; }

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

            winningPlayers = new List<Profile>();

            Profile[] sortedPlayers = GetSortedPlayers(activePlayers);

            foreach (Profile player in sortedPlayers)
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

        public Profile GetWinningPlayer()
        {
            if (winningPlayers == null)
            {
                return null;
            }
            if (winningPlayers.Count == 0)
            {
                return null;
            }

            Profile winner = winningPlayers[0];
            return winner;
        }

        public Profile[] GetWinningTeam()
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

        public abstract Profile[] GetSortedPlayers(Profile[] allPlayersToSort);

        public abstract bool HasPlayerMetWinConditions(Profile player);
    }
}