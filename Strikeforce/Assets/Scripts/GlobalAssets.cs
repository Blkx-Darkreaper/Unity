using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Color = UnityEngine.Color;

namespace Strikeforce
{
    public struct Tags
    {
        public const string PLAYER = "Player";
        public const string GROUND = "Ground";
        public const string BOUNDARY = "Boundary";
        public const string UNIT = "Unit";
        public const string STRUCTURE = "Structure";
        public const string ENEMY = "Enemy";
        public const string SUN = "Sun";
        public const string MAIN_CAMERA = "MainCamera";
        public const string GAME_MANAGER = "GameManager";
        public const string MENU_OBJECT = "MenuObject";
        public const string NETWORK_MANAGER = "NetworkManager";
        public const string HEADER = "Header";
        public const string BUTTON = "Button";
        public const string LEVEL = "Level";
        public const string SCENE = "Scene";
    }

    public struct Layers
    {
        public const string TRANSPARENT_FX = "TransparentFX";
        public const string IGNORE_RAYCAST = "Ignore Raycast";
        public const string WATER = "Water";
        public const string UI = "UI";
        public const string BASE = "Base";
        public const string EFFECTS = "Effects";
        public const string GROUND = "Ground";
        public const string AIR = "Air";
    }

    public struct Hooks
    {
        public const string CHANGE_HEALTH = "OnChangeHealth";
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

    public struct Scenes
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

    public struct Direction
    {
        public const string UP = "Up";
        public const string DOWN = "Down";
        public const string MIDDLE = "Middle";
        public const string LEFT = "Left";
        public const string RIGHT = "Right";
        public const string CENTER = "Center";
        public const string X = "X";
        public const string Y = "Y";
    }

    public static class GlobalAssets
    {
        public static string DefaultUsername = "NewPlayer";
        public static Vector3 InvalidLocation { get { return new Vector3(-99999f, -99999f, -99999f); } }
        public static GUISkin SelectionBoxSkin { get; set; }
        public static Bounds InvalidBounds { get { return new Bounds(new Vector3(-99999f, -99999f, -99999f), Vector3.zero); } }
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
        private static int nextPrefabIndex = 0;
        private static Dictionary<string, int> weaponPrefabs = new Dictionary<string, int>();
        private static Dictionary<string, int> structurePrefabs = new Dictionary<string, int>();
        private static Dictionary<string, int> vehiclePrefabs = new Dictionary<string, int>();
        private static Dictionary<string, int> projectilePrefabs = new Dictionary<string, int>();
        private static Dictionary<string, int> miscPrefabs = new Dictionary<string, int>();

        public struct Prefabs
        {
            public const string WEAPONS = "Weapons";
            public const string STRUCTURES = "Structures";
            public const string VEHICLES = "Vehicles";
            public const string PROJECTILES = "Projectiles";
            public const string MISC = "Misc";
        }

        public struct Camera
        {
            public static float ScrollSpeed = 25f;
            public static int ScrollArea = 15;
            public static float MinHeight = 10;
            public static float MaxHeight = 40;
            public static float RotationAmount = 10f;
            public static float RotationSpeed = 100f;
        }

        public struct HealthBarTextures
        {
            public static Texture2D Healthy;
            public static Texture2D Damaged;
            public static Texture2D Critical;
        }

        public static ResourceType GetResourceType(string typeName)
        {
            switch (typeName)
            {
                case ResourceProperties.MONEY:
                    return ResourceType.Money;

                case ResourceProperties.FUEL:
                    return ResourceType.Fuel;

                case ResourceProperties.ROCKETS:
                    return ResourceType.Rockets;

                case ResourceProperties.MISSILES:
                    return ResourceType.Missiles;

                case ResourceProperties.BOMBS:
                    return ResourceType.Bombs;

                case ResourceProperties.MATERIEL:
                    return ResourceType.Materiel;

                default:
                    return ResourceType.Unknown;
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
                        resourceHealthBarTextures.Add(ResourceType.Fuel, resourceHealthBar);
                        break;

                    case ResourceProperties.ROCKETS:
                        resourceHealthBarTextures.Add(ResourceType.Rockets, resourceHealthBar);
                        break;

                    case ResourceProperties.MISSILES:
                        resourceHealthBarTextures.Add(ResourceType.Missiles, resourceHealthBar);
                        break;

                    case ResourceProperties.BOMBS:
                        resourceHealthBarTextures.Add(ResourceType.Bombs, resourceHealthBar);
                        break;

                    case ResourceProperties.MATERIEL:
                        resourceHealthBarTextures.Add(ResourceType.Materiel, resourceHealthBar);
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

        public static void RegisterPrefab(string name, GameObject prefab, string collectionName)
        {
            switch (collectionName)
            {
                case Prefabs.WEAPONS:
                    RegisterWeaponPrefab(name, prefab);
                    break;

                case Prefabs.STRUCTURES:
                    RegisterStructurePrefab(name, prefab);
                    break;

                case Prefabs.VEHICLES:
                    RegisterVehiclePrefab(name, prefab);
                    break;

                case Prefabs.PROJECTILES:
                    RegisterProjectilePrefab(name, prefab);
                    break;

                case Prefabs.MISC:
                default:
                    RegisterMiscPrefab(name, prefab);
                    break;
            }
        }

        public static void RegisterWeaponPrefab(string name, GameObject prefab)
        {
            registerPrefab(name, prefab, weaponPrefabs);
        }

        public static void RegisterStructurePrefab(string name, GameObject prefab)
        {
            registerPrefab(name, prefab, structurePrefabs);
        }

        public static void RegisterVehiclePrefab(string name, GameObject prefab)
        {
            registerPrefab(name, prefab, vehiclePrefabs);
        }

        public static void RegisterProjectilePrefab(string name, GameObject prefab)
        {
            registerPrefab(name, prefab, projectilePrefabs);
        }

        public static void RegisterMiscPrefab(string name, GameObject prefab)
        {
            registerPrefab(name, prefab, miscPrefabs);
        }

        private static void registerPrefab(string name, GameObject prefab, Dictionary<string, int> collection)
        {
            if (prefab == null)
            {
                return;
            }

            ClientScene.RegisterPrefab(prefab);
            NetworkManager.singleton.spawnPrefabs.Add(prefab);
            int index = nextPrefabIndex++;

            collection.Add(name, index);
        }

        public static GameObject GetPrefab(string name)
        {
            GameObject entity = GetStructurePrefab(name);
            if (entity != null)
            {
                return entity;
            }

            entity = GetVehiclePrefab(name);
            if (entity != null)
            {
                return entity;
            }

            entity = GetMiscPrefab(name);
            return entity;
        }

        public static GameObject GetWeaponPrefab(string name)
        {
            return getPrefabFromCollection(name, weaponPrefabs);
        }

        public static GameObject GetStructurePrefab(string name)
        {
            return getPrefabFromCollection(name, structurePrefabs);
        }

        public static GameObject GetVehiclePrefab(string name)
        {
            return getPrefabFromCollection(name, vehiclePrefabs);
        }

        public static GameObject[] GetProjectilePrefabs(string name)
        {
            List<GameObject> prefabs = new List<GameObject>();

            GameObject nextPrefab = getPrefabFromCollection(name, projectilePrefabs);
            int index = 2;

            while (nextPrefab != null)
            {
                prefabs.Add(nextPrefab);

                string fullName = string.Format("{0}{1}", name, index);
                nextPrefab = getPrefabFromCollection(fullName, projectilePrefabs);

                index++;
            }

            return prefabs.ToArray();
        }

        public static GameObject GetMiscPrefab(string name)
        {
            return getPrefabFromCollection(name, miscPrefabs);
        }

        private static GameObject getPrefabFromCollection(string name, Dictionary<string, int> collection)
        {
            bool exists = collection.ContainsKey(name);
            if (exists == false)
            {
                return null;
            }

            int index = collection[name];

            int totalPrefabs = NetworkManager.singleton.spawnPrefabs.Count;
            if (index >= totalPrefabs)
            {
                return null;
            }

            GameObject prefab = NetworkManager.singleton.spawnPrefabs[index];
            return prefab;
        }

        public static GameObject GetChildGameObjectWithName(GameObject parent, string nameToFind)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent.transform)
            {
                string childName = child.gameObject.name;
                if (!childName.Equals(nameToFind))
                {
                    continue;
                }

                return child.gameObject;
            }

            return null;
        }

        public static GameObject GetChildGameObjectWithTag(GameObject parent, string tagToFind)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent.transform)
            {
                string childTag = child.gameObject.tag;
                if (!childTag.Equals(tagToFind))
                {
                    continue;
                }

                return child.gameObject;
            }

            return null;
        }

        public static T GetChildComponentWithTag<T>(GameObject parent, string tagToFind) where T : Component
        {
            if (parent == null)
            {
                return null;
            }

            T[] children = parent.GetComponentsInChildren<T>();

            foreach (T child in children)
            {
                string childTag = child.gameObject.tag;
                if (!childTag.Equals(tagToFind))
                {
                    continue;
                }

                return child;
            }

            return null;
        }

        public static string ReadTextFile(string filename)
        {
            string text = string.Empty;
            bool fileExists = File.Exists(filename);
            if (fileExists == false)
            {
                return text;
            }

            using (StreamReader inputFile = new StreamReader(filename))
            {
                text += inputFile.ReadToEnd();
            }

            return text;
        }

        public static void WriteTextFile(string filename, string text)
        {
            using (StreamWriter outputFile = new StreamWriter(filename))
            {
                outputFile.WriteLine(text);
            }
        }

        public static Texture2D MakeTexture(int width, int height, Color colour)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = colour;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static void KeepInBounds(Rectangle bounds, float x, float z, ref float deltaX, ref float deltaZ)
        {
            float finalX = x + deltaX;
            float finalZ = z + deltaZ;

            float halfWidth = bounds.Width / 2;

            deltaX = Mathf.Clamp(finalX, -halfWidth, halfWidth) - x;
            deltaZ = Mathf.Clamp(finalZ, 0, bounds.Height) - z;
        }
    }
}