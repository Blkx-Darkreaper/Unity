using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "wargear", menuName = "40K/Item/Wargear", order = 4)]
public class Wargear : ScriptableObject
{
    public int points;
}