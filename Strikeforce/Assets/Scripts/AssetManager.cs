using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class AssetManager : MonoBehaviour
    {
        public GUISkin HudSkin;
        public Texture2D Healthy, Damaged, Critical;
        public GameObject BasicShot, Flameburst, Bolt, Wave, Beam;
        public GameObject[] Structures, Vehicles, Misc;
        public GameObject BoundingBox;

        public void Awake()
        {
            // Load all game assets
            LoadGuiSkins();
            LoadHealthBarTextures();
            LoadWeaponPrefabs();
            LoadStructurePrefabs();
            LoadVehiclePrefabs();
            LoadMiscPrefabs();
            LoadBoundingBoxPrefab();
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
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.SHOT, BasicShot);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.FLAMEBURST, Flameburst);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.BOLT, Bolt);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.WAVE, Wave);
            GlobalAssets.WeaponPrefabs.Add(Weapon.Types.BEAM, Beam);
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

        protected void LoadBoundingBoxPrefab()
        {
            string name = BoundingBox.name;
            GlobalAssets.MiscPrefabs.Add(name, BoundingBox);
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