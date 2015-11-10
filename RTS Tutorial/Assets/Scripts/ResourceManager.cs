using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS
{
    public static class ResourceManager
    {
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

        private static Vector3 invalidPositionValue = new Vector3(-99999f, -99999f, -99999f);
        public static Vector3 invalidPoint { get { return invalidPositionValue; } }
        public static GUISkin selectionBoxSkin { get; set; }
        private static Bounds invalidBoundsValue = new Bounds(new Vector3(-99999f, -99999f, -99999f), Vector3.zero);
        public static Bounds invalidBounds { get { return invalidBoundsValue; } }
        public static int buildSpeed { get { return 2; } }
        private static Dictionary<ResourceType, Texture2D> resourceHealthBarTextures;

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

        public static void SetResourceBarTextures(Dictionary<ResourceType, Texture2D> images)
        {
            resourceHealthBarTextures = images;
        }
    }

    public struct KeyMappings
    {
        public const string mouseXAxis = "Mouse X";
        public const string mouseYAxis = "Mouse Y";
        public const string scrollWheel = "Mouse ScrollWheel";
    }

    public struct Tags
    {
        public const string player = "Player";
        public const string ground = "Ground";
        public const string structure = "Structure";
        public const string unit = "Unit";
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
}
