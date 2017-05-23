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
            this.projectilePrefab = GlobalAssets.GetProjectilePrefab(ProjectileType);
        }

        public void SetFiringPointOffset(Vector3 offset)
        {
            this.firingPointOffset = offset;
        }

        public void Fire()
        {
            Fire(Direction.CENTER, 0, 0, 1);
        }

        public void Fire(string direction, int angleSpread, int horizontalSpread, int groupingBonus)
        {
            // Command function is called from the client, but invoked on the server
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

            // create the bullet object from the bullet prefab
            GameObject bullet = Instantiate(projectilePrefab, firingPoint, Quaternion.identity) as GameObject;

            Projectile projectile = bullet.GetComponent<Projectile>();

            // make the bullet move away in front of the player
            bullet.GetComponentInChildren<Rigidbody>().velocity = Parent.transform.forward * projectile.MaxVelocity;

            // spawn the bullet on the clients
            NetworkServer.Spawn(bullet);

            firingSound.Play();

            // make bullet disappear after 10 seconds
            Destroy(bullet, 10.0f); // testing
        }
    }
}