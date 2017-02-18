using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Strikeforce
{
    public class Weapon : Equipment
    {
        public string Type;
        public int Priority;
        public bool IsOrdnanceWeapon = false;
        protected Vector3 firingPointOffset;
        protected AudioSource firingSound;
        public string ProjectileType;
        protected GameObject projectilePrefab;
        //protected const string FIRE = "Fire";
        protected const string PROJECTILE = "Projectile";
        public struct Types
        {
            public const string BASIC_SHOT = "Basic Shot";
            public const string FLAMEBURST = "Flameburst";
            public const string CANNON = "Cannon";
            public const string BOLT = "Bolt";
            public const string WAVE = "Wave";
            public const string BEAM = "Beam";
        }

        protected override void Awake()
        {
            base.Awake();

            this.IsWeapon = true;
            firingSound = GetComponent<AudioSource>();

            LoadProjectilePrefab();
        }

        protected void LoadProjectilePrefab()
        {
            string projectileName = string.Format("{0} {1}", Type, PROJECTILE);
            this.projectilePrefab = GlobalAssets.GetMiscPrefab(projectileName);
        }

        public void SetFiringPointOffset(Vector3 offset)
        {
            this.firingPointOffset = offset;
        }

        public void Fire()
        {
            Fire(0, 0, 1);
        }

        public void Fire(int angleSpread, int horizontalSpread, int groupingBonus)
        {
            bool active = Activate();
            if (active == false)
            {
                return;
            }

            Debug.Log(string.Format("{0} fired!", Type));

            Vector3 parentLocation = Parent.transform.position;
            float x = parentLocation.x;
            float y = parentLocation.y;
            float z = parentLocation.z;
            Vector3 firingPoint = new Vector3(x, y, z) + firingPointOffset;

            GameObject projectilePrefab = NetworkManager.singleton.spawnPrefabs[0];
            // create the bullet object from the bullet prefab
            GameObject bullet = Instantiate(projectilePrefab, firingPoint, Quaternion.identity) as GameObject;

            // make the bullet move away in front of the player
            bullet.GetComponentInChildren<Rigidbody>().velocity = Parent.transform.forward * 4;

            // spawn the bullet on the clients
            NetworkServer.Spawn(bullet);

            firingSound.Play();

            // make bullet disappear after 10 seconds
            Destroy(bullet, 10.0f);
        }

        //public void Start()
        //{
        //	InvokeRepeating(FIRE, AttackDelay, weapon.cooldown);
        //}

        //private void Fire()
        //{
        //	Instantiate(weapon.projectileType, ShotSpawn.position, ShotSpawn.rotation);
        //	var blasterSound = GetComponent<AudioSource>();
        //	blasterSound.Play();
        //}
    }
}