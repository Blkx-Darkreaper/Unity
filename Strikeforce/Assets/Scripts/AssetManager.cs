using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class AssetManager : MonoBehaviour
    {
        public Texture2D healthy, damaged, critical;
        public Weapon shot, flameburst, bolt, wave, beam;

        public void Awake()
        {
            // Load all game assets
            LoadHealthBarTextures();
        }

        protected void LoadHealthBarTextures()
        {
            GlobalAssets.HealthBarTextures.Healthy = healthy;
            GlobalAssets.HealthBarTextures.Damaged = damaged;
            GlobalAssets.HealthBarTextures.Critical = critical;
        }

        protected void LoadWeapons()
        {
            GlobalAssets.Weapons.Add(Weapon.Types.SHOT, shot); //new Weapon(Weapon.Types.SHOT, 1, 1, 1)},
            GlobalAssets.Weapons.Add(Weapon.Types.FLAMEBURST, flameburst);  //new Weapon(Weapon.Types.FLAMEBURST, 2, 1, 1)},
            //new Weapon(Weapon.Types.CANNON, 3, 1, 1)},
            GlobalAssets.Weapons.Add(Weapon.Types.BOLT, bolt);  //new Weapon(Weapon.Types.BOLT, 3, 1, 1)},
            GlobalAssets.Weapons.Add(Weapon.Types.WAVE, wave);  //new Weapon(Weapon.Types.WAVE, 4, 1, 2)},
            GlobalAssets.Weapons.Add(Weapon.Types.BEAM, beam);  //new Weapon(Weapon.Types.BEAM, 5, 1, 2)}
        }
    }
}