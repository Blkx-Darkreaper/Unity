using System;
using UnityEngine;

public abstract class Unit : ScriptableObject
{
    public ModelType modelType;
    public bool isVehicle;

    public struct ModelType
    {
        public const string infantry = "Infantry";
        public const string bike = "Bike";
        public const string jetbike = "Jetbike";
        public const string monstrousCreature = "Monstrous creature";
        public const string jumpInfantry = "Jump infantry";
        public const string artillery = "Artillery";
        public const string beast = "Beasts & cavalry";
        public const string walker = "Walker";
        public const string skimmer = "Skimmer";
        public const string other = "Other vehicle";
    }

    public int points;
    public int weaponSkill; //melee weapons
    public int ballisticSkill; //ranged weapons
    public int strength;
    public int toughness;
    public int wounds;
    public int initiative;
    public int attacks;    // only used for close combat
    public int leadership;
    public int save;
    public int armourValue;
    public int invulnerableSave;

    public float baseRadius;
    public float modelHeight;

    public float moveDistance;
}