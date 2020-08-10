using UnityEngine;

public abstract class Weapon : ScriptableObject
{
    public enum Handed { Single, Double};
    public Handed handed;
    public int points;
}