using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS
{
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
        money, power, ore, unknown
    }

    public struct ResourceProperties
    {
        public const string MONEY = "Money";
        public const string MONEY_LIMIT = "MoneyLimit";
        public const string POWER = "Power";
        public const string POWER_LIMIT = "PowerLimit";
        public const string ORE = "Ore";
        public const string ORE_LIMIT = "OreLimit";
        public const string UNKNOWN = "Unknown";
    }

    public struct Menu
    {
        public static float width { get { return headerWidth + 2 * buttonHeight + 4 * padding; } }
        public static float padding = 10f;
        public static float textHeight = 25f;
        public static float headerWidth = 256f;
        public static float headerHeight = 32f;
        public static float buttonWidth { get { return (width - 3 * padding) / 2; } }
        public static float buttonHeight = 40f;
    }

    public struct Levels
    {
        public static string mainMenu = "mainMenu";
        public static string game = "map";
        public static string loadTest = "blankTest";
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

    public struct KeyMappings
    {
        public const string mouseXAxis = "Mouse X";
        public const string mouseYAxis = "Mouse Y";
        public const string scrollWheel = "Mouse ScrollWheel";
    }

    public static class ResourceManager
    {
        private static Vector3 invalidPositionValue = new Vector3(-99999f, -99999f, -99999f);
        public static Vector3 invalidPoint { get { return invalidPositionValue; } }
        public static GUISkin selectionBoxSkin { get; set; }
        private static Bounds invalidBoundsValue = new Bounds(new Vector3(-99999f, -99999f, -99999f), Vector3.zero);
        public static Bounds invalidBounds { get { return invalidBoundsValue; } }
        public static int buildSpeed { get { return 2; } }
        private static Dictionary<ResourceType, Texture2D> resourceHealthBarTextures;
        private static Dictionary<int, Texture2D> playerAvatars;
        public static KeyValuePair<string, string>[] PlayerResourceProperties = new KeyValuePair<string, string>[] 
        { new KeyValuePair<string, string>(ResourceProperties.MONEY, ResourceProperties.MONEY_LIMIT),
        new KeyValuePair<string, string>(ResourceProperties.POWER, ResourceProperties.POWER_LIMIT)};

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

                case ResourceProperties.POWER:
                    return ResourceType.power;

                case ResourceProperties.ORE:
                    return ResourceType.ore;

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
                    case "ore":
                        resourceHealthBarTextures.Add(ResourceType.ore, resourceHealthBar);
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
            foreach(Texture2D avatar in playerAvatars.Values)
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
            for (int i = 0; i < images.Length; i++ )
            {
                Texture2D avatar = images[i];
                playerAvatars.Add(i, avatar);
            }
        }
    }
}
