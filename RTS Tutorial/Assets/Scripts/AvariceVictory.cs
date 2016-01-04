using UnityEngine;
using System.Collections;
using RTS;

public class AvariceVictory : VictoryCondition {

    public int threshold = 1050;
    protected ResourceType type = ResourceType.money;

    public override string GetDescription()
    {
        string description = string.Format("Accumulate {0} {1}", threshold, type.ToString());
        return description;
    }

    public override bool HasPlayerMetWinConditions(PlayerController player)
    {
        if (player == null)
        {
            return false;
        }

        bool playerIsDead = player.isDead;
        if (playerIsDead == true)
        {
            return false;
        }
        
        float weathAccumulated = player.GetResourceAmount(type);
        if (weathAccumulated < threshold)
        {
            return false;
        }

        return true;
    }
}