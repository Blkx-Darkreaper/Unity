using UnityEngine;
using System.Collections;

public class SurvivalVictory : VictoryCondition {

    public float gameDuration = 1f; //minutes
    protected float timeRemaining = 0f;

    protected void Awake()
    {
        timeRemaining = gameDuration * 60;
    }

    protected void Update()
    {
        timeRemaining -= Time.deltaTime;
    }

    public override string GetDescription()
    {
        string description = string.Format("Survive for {0} minutes", gameDuration);
        return description;
    }

    public override bool IsGameOver()
    {
        int humansRemaining = 0;
        foreach (PlayerController player in activePlayers)
        {
            if (player.isNPC == true)
            {
                continue;
            }
            if (player.isDead == true)
            {
                continue;
            }

            humansRemaining++;
        }

        if (humansRemaining == 0)
        {
            return true;
        }

        bool timesUp = timeRemaining <= 0;
        return timesUp;
    }

    public override bool HasPlayerMetWinConditions(PlayerController player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.isNPC == true)
        {
            return false;
        }

        bool stillAlive = !player.isDead;
        return stillAlive;
    }
}