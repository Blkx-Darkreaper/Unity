using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class AssetManager : MonoBehaviour
    {
        public Texture2D healthy, damaged, critical;
        public GameObject shot, flameburst, bolt, wave, beam;
        public GameObject[] Structures, Vehicles, Misc;

        public void Awake()
        {
            // Load all game assets
            LoadHealthBarTextures();
            LoadWeaponPrefabs();
            LoadStructurePrefabs();
            LoadVehiclePrefabs();
            LoadMiscPrefabs();
        }

        protected void LoadHealthBarTextures()
        {
            GlobalAssets.HealthBarTextures.Healthy = healthy;
            GlobalAssets.HealthBarTextures.Damaged = damaged;
            GlobalAssets.HealthBarTextures.Critical = critical;
        }

        protected void LoadWeaponPrefabs()
        {
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.SHOT, shot);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.FLAMEBURST, flameburst);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.BOLT, bolt);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.WAVE, wave);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.BEAM, beam);
        }

        protected void LoadStructurePrefabs()
        {
            LoadPrefabs<Structure>(ref GlobalAssets.StructurePrefabs, Structures, "Structure");
        }

        protected void LoadVehiclePrefabs()
        {
            LoadPrefabs<Selectable>(ref GlobalAssets.VehiclePrefabs, Vehicles, "Vehicle");
        }

        protected void LoadMiscPrefabs()
        {
            LoadPrefabs<Entity>(ref GlobalAssets.MiscPrefabs, Misc, "Entity");
        }

        protected void LoadPrefabs<T>(ref Dictionary<string, GameObject> collection, GameObject[] items, string className)
        where T : Entity
        {
            collection = new Dictionary<string,
            GameObject>();
            foreach (GameObject gameObject in items)
            {
                T controller = gameObject.GetComponent<T>();
                if (controller == null)
                {
                    Debug.Log(string.Format("{0} gameobject has no controller", gameObject.name));
                    continue;
                }

                SetEntityName(gameObject, controller);
                string name = controller.name;
                if (name.Equals(string.Empty) == true)
                {
                    Debug.Log(string.Format("{0} {1} has no name", className, gameObject.ToString()));
                    continue;
                }

                collection.Add(name, gameObject);
            }
        }

        public void SetEntityName(GameObject gameObject, Entity entity)
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