using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "army", menuName = "40K/Org/Army", order = 2)]
public class Army : ScriptableObject
{
    public LinkedList<Squad> allSquads;
    public LinkedList<Squad> allFriendlies;
    public LinkedList<Squad> allEnemies;

    public LinkedList<Squad> GetEnemySquadsInRange(Vector3 currentLocation, float range)
    {
        throw new NotImplementedException();
    }

    public LinkedList<Squad> GetEnemySquadsInSight(Vector3 currentLocation)
    {
        throw new NotImplementedException();
    }

    public Squad GetNearestEnemySquad(Vector3 currentLocation)
    {
        throw new NotImplementedException();
    }
}