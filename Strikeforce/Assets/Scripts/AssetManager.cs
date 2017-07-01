using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class AssetManager : MonoBehaviour
    {
        public GUISkin HudSkin;
        public Texture2D Healthy, Damaged, Critical;
        public GameObject[] Weapons, Structures, Vehicles, Projectiles, Misc;

        public void Awake()
        {
            // Load all game assets
            LoadGuiSkins();
            LoadHealthBarTextures();
            LoadWeaponPrefabs();
            LoadStructurePrefabs();
            LoadVehiclePrefabs();
            LoadProjectilePrefabs();
            LoadMiscPrefabs();
        }

        protected void LoadGuiSkins()
        {
            GlobalAssets.SelectionBoxSkin = HudSkin;
        }

        protected void LoadHealthBarTextures()
        {
            GlobalAssets.HealthBarTextures.Healthy = Healthy;
            GlobalAssets.HealthBarTextures.Damaged = Damaged;
            GlobalAssets.HealthBarTextures.Critical = Critical;
        }

        protected void LoadWeaponPrefabs()
        {
            LoadPrefabs<Weapon>(Weapons, GlobalAssets.Prefabs.WEAPONS);
        }

        protected void LoadStructurePrefabs()
        {
            LoadPrefabs<Structure>(Structures, GlobalAssets.Prefabs.STRUCTURES);
        }

        protected void LoadVehiclePrefabs()
        {
            LoadPrefabs<Selectable>(Vehicles, GlobalAssets.Prefabs.VEHICLES);
        }

        protected void LoadProjectilePrefabs()
        {
            LoadPrefabs<Projectile>(Projectiles, GlobalAssets.Prefabs.PROJECTILES);
        }

        protected void LoadMiscPrefabs()
        {
            LoadPrefabs<MonoBehaviour>(Misc, GlobalAssets.Prefabs.MISC);
        }

        protected void LoadPrefabs<T>(GameObject[] items, string collectionName)
        where T : MonoBehaviour
        {
            foreach (GameObject gameObject in items)
            {
                T component = gameObject.GetComponent<T>();
                if (component == null)
                {
                    Debug.Log(string.Format("{0} gameobject has no {1} component", gameObject.name, typeof(T).Name));
                    continue;
                }

                SetEntityName(gameObject, component);
                string name = component.name;
                if (name.Equals(string.Empty) == true)
                {
                    Debug.Log(string.Format("{0} {1} has no name", collectionName, gameObject.ToString()));
                    continue;
                }

                GlobalAssets.RegisterPrefab(name, gameObject, collectionName);
            }
        }

        public void SetEntityName(GameObject gameObject, MonoBehaviour entity)
        {
            if (entity.name == null)
            {
                entity.name = string.Empty;
            }

            if (entity.name.Equals(string.Empty) == false)
            {
                return;
            }

            entity.name = gameObject.name;
            if (entity.name.Equals(string.Empty) == false)
            {
                return;
            }

            entity.name = gameObject.tag;
        }
    }
}