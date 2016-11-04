using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public struct Tags
    {
        public const string PLAYER = "Player";
        public const string GROUND = "Ground";
        public const string BOUNDARY = "Boundary";
        public const string STRUCTURE = "Structure";
        public const string UNIT = "Unit";
        public const string ENEMY = "Enemy";
        public const string SUN = "Sun";
        public const string MAIN_CAMERA = "MainCamera";
        public const string GAMECONTROLLER = "GameController";
        public const string RESOURCE = "Resource";
    }

    public enum CursorState
    {
        select, move, attack, panLeft, panRight, panUp, panDown, harvest, rallyPoint
    }

    //public struct CursorState
    //{
    //    public const string select = "Select Cursor";
    //    public const string move = "Move Cursor";
    //    public const string attack = "Attack Cursor";
    //    public const string panLeft = "Pan Left Cursor";
    //    public const string panRight = "Pan Right Cursor";
    //    public const string panUp = "Pan Up Cursor";
    //    public const string panDown = "Pan Down Cursor";
    //    public const string harvest = "Harvest Cursor";
    //}

    public enum ResourceType
    {
        money, fuel, rockets, missiles, bombs, materiel, unknown
    }

    public struct ResourceProperties
    {
        public const string MONEY = "Money";
        public const string MONEY_LIMIT = "MoneyLimit";
        public const string FUEL = "Fuel";
        public const string FUEL_LIMIT = "FuelLimit";
        public const string ROCKETS = "Rockets";
        public const string ROCKETS_LIMIT = "RocketsLimit";
        public const string MISSILES = "Missiles";
        public const string MISSILES_LIMIT = "MissilesLimit";
        public const string BOMBS = "Bombs";
        public const string BOMBS_LIMIT = "BombsLimit";
        public const string MATERIEL = "Materiel";
        public const string MATERIEL_LIMIT = "MaterielLimit";
        public const string UNKNOWN = "Unknown";
    }

    public struct MenuAttributes
    {
        public static float Width { get { return HeaderWidth + 2 * ButtonHeight + 4 * Padding; } }
        public static float Padding = 10f;
        public static float TextHeight = 25f;
        public static float HeaderWidth = 256f;
        public static float HeaderHeight = 32f;
        public static float ButtonWidth { get { return (Width - 3 * Padding) / 2; } }
        public static float ButtonHeight = 40f;
        public static float pauseMenuHeight = 20f;
    }

    public struct Levels
    {
        public static string Lobby = "lobby";
        public static string MainMenu = "mainMenu";
        public static string Game = "map";
        public static string LoadTest = "blankTest";
    }

    public struct JsonProperties
    {
        public const string GAME_DETAILS = "PlayerAccounts";
        public const string SUN = "Sun";
        public const string GROUND = "Ground";
        public const string CAMERA = "Camera";
        public const string RESOURCES = "Resources";
        public const string PLAYERS = "Players";
        //public const string STRUCTURES = "Structures";
        //public const string UNITS = "Units";
    }

    public struct EntityProperties
    {
        public const string NAME = "Name";
        public const string ID = "Id";
        public const string MESHES = "Meshes";
    }

    public struct KeyMappings
    {
        public const string mouseXAxis = "Mouse X";
        public const string mouseYAxis = "Mouse Y";
        public const string scrollWheel = "Mouse ScrollWheel";
    }

    public static class GlobalAssets
    {
        private static Vector3 invalidPositionValue = new Vector3(-99999f, -99999f, -99999f);
        public static Vector3 InvalidPoint { get { return invalidPositionValue; } }
        public static GUISkin SelectionBoxSkin { get; set; }
        private static Bounds invalidBoundsValue = new Bounds(new Vector3(-99999f, -99999f, -99999f), Vector3.zero);
        public static Bounds InvalidBounds { get { return invalidBoundsValue; } }
        public static int BuildSpeed { get { return 2; } }
        private static Dictionary<ResourceType, Texture2D> resourceHealthBarTextures;
        private static Dictionary<int, Texture2D> playerAvatars;
        public static KeyValuePair<string, string>[] PlayerResourceProperties = new KeyValuePair<string, string>[] 
        { 
		    new KeyValuePair<string, string>(ResourceProperties.MONEY, ResourceProperties.MONEY_LIMIT),
        	new KeyValuePair<string, string>(ResourceProperties.FUEL, ResourceProperties.FUEL_LIMIT),
            new KeyValuePair<string, string>(ResourceProperties.ROCKETS, ResourceProperties.ROCKETS_LIMIT),
            new KeyValuePair<string, string>(ResourceProperties.MISSILES, ResourceProperties.MISSILES_LIMIT),
            new KeyValuePair<string, string>(ResourceProperties.BOMBS, ResourceProperties.BOMBS_LIMIT),
            new KeyValuePair<string, string>(ResourceProperties.MATERIEL, ResourceProperties.MATERIEL_LIMIT)
	    };

        public struct Camera
        {
            public static float scrollSpeed = 25f;
            public static int scrollArea = 15;
            public static float minHeight = 10;
            public static float maxHeight = 40;
            public static float rotationAmount = 10f;
            public static float rotationSpeed = 100f;
        }

        public struct HealthBarTextures
        {
            public static Texture2D healthy;
            public static Texture2D damaged;
            public static Texture2D critical;
        }

        public static ResourceType GetResourceType(string typeName)
        {
            switch (typeName)
            {
                case ResourceProperties.MONEY:
                    return ResourceType.money;

                case ResourceProperties.FUEL:
                    return ResourceType.fuel;

                case ResourceProperties.ROCKETS:
                    return ResourceType.rockets;

                case ResourceProperties.MISSILES:
                    return ResourceType.missiles;

                case ResourceProperties.BOMBS:
                    return ResourceType.bombs;

                case ResourceProperties.MATERIEL:
                    return ResourceType.materiel;

                default:
                    return ResourceType.unknown;
            }
        }

        public static Texture2D GetResourceBarTexture(ResourceType type)
        {
            if (resourceHealthBarTextures == null)
            {
                return null;
            }
            if (!resourceHealthBarTextures.ContainsKey(type))
            {
                return null;
            }

            Texture2D healthBarTexture = resourceHealthBarTextures[type];
            return healthBarTexture;
        }

        public static void SetResourceBarTextures(Texture2D[] images)
        {
            resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
            foreach (Texture2D resourceHealthBar in images)
            {
                switch (resourceHealthBar.name)
                {
                    case ResourceProperties.FUEL:
                        resourceHealthBarTextures.Add(ResourceType.fuel, resourceHealthBar);
                        break;

                    case ResourceProperties.ROCKETS:
                        resourceHealthBarTextures.Add(ResourceType.rockets, resourceHealthBar);
                        break;

                    case ResourceProperties.MISSILES:
                        resourceHealthBarTextures.Add(ResourceType.missiles, resourceHealthBar);
                        break;

                    case ResourceProperties.BOMBS:
                        resourceHealthBarTextures.Add(ResourceType.bombs, resourceHealthBar);
                        break;

                    case ResourceProperties.MATERIEL:
                        resourceHealthBarTextures.Add(ResourceType.materiel, resourceHealthBar);
                        break;
                }
            }
        }

        public static Texture2D GetAvatar(int avatarId)
        {
            if (playerAvatars == null)
            {
                return null;
            }
            if (playerAvatars.ContainsKey(avatarId) == false)
            {
                return null;
            }

            Texture2D avatar = playerAvatars[avatarId];
            return avatar;
        }

        public static int GetAvatarIndex(string avatarName)
        {
            int index = 0;
            foreach (Texture2D avatar in playerAvatars.Values)
            {
                bool match = avatarName.Equals(avatar.name);
                if (match == false)
                {
                    index++;
                    continue;
                }

                return index;
            }

            return -1;
        }

        public static void SetAvatars(Texture2D[] images)
        {
            playerAvatars = new Dictionary<int, Texture2D>();
            for (int i = 0; i < images.Length; i++)
            {
                Texture2D avatar = images[i];
                playerAvatars.Add(i, avatar);
            }
        }
    }
}