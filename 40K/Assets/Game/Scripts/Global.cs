using System;
using Random = System.Random;
using UnityEngine;

public class Global : ScriptableObject
{
    public static GUISkin selectionBoxSkin;

    public struct Tags
    {
        public const string PLAYER = "Player";
        public const string GROUND = "Ground";
        public const string STRUCTURE = "Structure";
        public const string UNIT = "Unit";
        public const string SUN = "Sun";
        public const string MAIN_CAMERA = "MainCamera";
        public const string RESOURCE = "Resource";
    }

    public struct Factions
    {
        public const string spaceMarines = "Space Marines";
        public const string imperialGuard = "Imperial Guard";
        public const string chaosSpaceMarines = "Chaos Space Marines";
        public const string orks = "Orks";
        public const string eldar = "Eldar";
        public const string darkEldar = "Dark Eldar";
        public const string tau = "Tau";
        public const string tyrannids = "Tyrannids";
        public const string necrons = "Necrons";
        public const string teqs = "Teqs";
    }

    [Serializable]
    public struct Cover
    {
        public const string clear = "No cover";
        public const string razorWire = "Razor wire";
        public const string wireMesh = "Wire mesh";
        public const string highGrass = "High grass";
        public const string crops = "Crops";
        public const string bushes = "Bushes";
        public const string hedge = "Hedge";
        public const string fence = "Fence";
        public const string unit = "Unit";
        public const string trenche = "Trenche";
        public const string gunPit = "Gun pit";
        public const string tankTraps = "Tank traps";
        public const string emplacement = "Emplacement";
        public const string sandbags = "Sandbags";
        public const string barricade = "Barricade";
        public const string logs = "Logs";
        public const string pipes = "Pipes";
        public const string crates = "Crates";
        public const string barrels = "Barrels";
        public const string hillCrest = "Hill crest";
        public const string woods = "Woods";
        public const string jungle = "Jungle";
        public const string wreckage = "Wreckage";
        public const string crater = "Crater";
        public const string rubble = "Rubble";
        public const string rocks = "Rocks";
        public const string ruins = "Ruins";
        public const string wall = "Wall";
        public const string building = "Building";
        public const string wreckedVehicle = "Wrecked vehicle";
        public const string fortification = "Fortification";
    }

    public static int getCoverSave(string coverType)
    {
        int coverSave;
        switch (coverType)
        {
            case Cover.razorWire:
            case Cover.wireMesh:
                coverSave = 6;
                break;

            case Cover.highGrass:
            case Cover.crops:
            case Cover.bushes:
            case Cover.hedge:
            case Cover.fence:
                coverSave = 5;
                break;

            case Cover.unit:
            case Cover.trenche:
            case Cover.gunPit:
            case Cover.tankTraps:
            case Cover.emplacement:
            case Cover.sandbags:
            case Cover.barricade:
            case Cover.logs:
            case Cover.pipes:
            case Cover.crates:
            case Cover.barrels:
            case Cover.hillCrest:
            case Cover.woods:
            case Cover.jungle:
            case Cover.wreckage:
            case Cover.crater:
            case Cover.rubble:
            case Cover.rocks:
            case Cover.ruins:
            case Cover.wall:
            case Cover.building:
            case Cover.wreckedVehicle:
                coverSave = 4;
                break;

            case Cover.fortification:
                coverSave = 3;
                break;

            default:
                coverSave = 0;
                break;
        }

        return coverSave;
    }

    public struct Ranges
    {
        public const int inchToIntConversionRatio = 4;

        public const float infantryBaseRadius = 1.25f / 2;

        public const float coherency = 2;
        public const float minimumDistance = 1;

        public const float footMovement = 6;

        public const float lasPistol = 12;
        public const float boltgun = 24;
        public const float autocannon = 48;
    }

    private static Random die = new Random();

    private static int RollDie()
    {
        int roll = die.Next(1, 6);
        return roll;
    }

    public static int RollDice(int numberOfDice)
    {
        int sum = 0;
        for (int i = 0; i < numberOfDice; i++)
        {
            int roll = RollDie();
            sum += roll;
        }

        return sum;
    }

    public static int RollDiceKeepHighest(int numberOfRolls)
    {
        int highestRoll = 0;
        for (int i = 0; i < numberOfRolls; i++)
        {
            int roll = RollDie();
            if (roll < highestRoll)
            {
                continue;
            }

            highestRoll = roll;
        }

        return highestRoll;
    }

    public static float GetDistance(Vector3 origin, Vector3 destination)
    {
        float distance = Vector3.Distance(origin, destination);
        return distance;
    }

    public static Vector3 GetDirection(Vector3 origin, Vector3 target)
    {
        Vector3 direction = target - origin;
        return direction;
    }

    public static Tuple<float, float> SolveQuadraticEquation(float a, float b, float c)
    {
        float positive = (float)(-b + Math.Sqrt(Math.Pow(b, 2) - 4 * a * c)) / (2 * a);
        float negative = (float)(-b - Math.Sqrt(Math.Pow(b, 2) - 4 * a * c)) / (2 * a);

        return new Tuple<float, float>(positive, negative);
    }
}