using UnityEngine;
using System.Collections;

public class EscortVictory : VictoryCondition {

    public Vector3 destination = Vector3.zero;
    public EntityController vip;
    public Texture2D highlight;

    protected void Start()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Ground";
        cube.transform.localScale = new Vector3(3, 0.01f, 3);
        cube.transform.position = new Vector3(destination.x, 0.005f, destination.z);
        if (highlight != null)
        {
            cube.GetComponent<Renderer>().material.mainTexture = highlight;
        }
        cube.transform.parent = this.transform;
    }

    public override string GetDescription()
    {
        string vipName = "VIP";
        if (vip != null)
        {
            vipName = vip.name;
        }

        string description = string.Format("Escort the {0} to the rendez-vous point", vipName);
        return description;
    }

    public override bool HasPlayerMetWinConditions(PlayerController player)
    {
        if (player == null)
        {
            return false;
        }
        if (vip == null)
        {
            return false;
        }

        bool playerStillAlive = !player.isDead;
        bool vipAtDestination = IsVipAtDestination(vip);

        bool winConditionMet = playerStillAlive && vipAtDestination;
        return winConditionMet;
    }

    protected bool IsVipAtDestination(EntityController vip)
    {
        if (vip == null)
        {
            return false;
        }

        float distanceThreshold = 3f;
        Vector3 currentPosition = vip.transform.position;

        bool atDestinationX = Mathf.Abs(destination.x - currentPosition.x) < distanceThreshold;

        bool atDestinationY = Mathf.Abs(destination.y - currentPosition.y) < distanceThreshold;

        bool atDestination = atDestinationX && atDestinationY;
        return atDestination;
    }
}