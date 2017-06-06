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
            Fire(0, 0, 1);
        }

        public void Fire(int angleSpread, int horizontalSpread, int groupingBonus)
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

            if (Mathf.Abs(angleSpread) > 0)
            {
                SplitShot(firingPoint, angleSpread, horizontalSpread);
                return;
            }

            if(Mathf.Abs(horizontalSpread) > 0)
            {
                SplitShot(firingPoint, angleSpread, horizontalSpread);
                return;
            }

            SingleShot(firingPoint);
        }

        protected void SingleShot(Vector3 firingPoint)
        {
            // create the bullet object from the bullet prefab
            GameObject bullet = Instantiate(projectilePrefab, firingPoint, Quaternion.identity) as GameObject;

            Projectile projectile = bullet.GetComponent<Projectile>();

            // make the bullet move away in front of the player
            Vector3 angleVector = Parent.transform.forward;

            bullet.GetComponentInChildren<Rigidbody>().velocity = angleVector * projectile.MaxVelocity;

            // spawn the bullet on the clients
            NetworkServer.Spawn(bullet);

            firingSound.Play();

            // make bullet disappear after 10 seconds
            Destroy(bullet, 10.0f); // testing
        }

        protected void SplitShot(Vector3 firingPoint, int angleSpread, int horizontalSpread)
        {
            Vector3 rightOffet = Parent.transform.right * 0.1f;
            Vector3 leftOffset = -rightOffet;

            Vector3 leftFiringPoint = new Vector3(firingPoint.x, firingPoint.y, firingPoint.z) + leftOffset;
            Vector3 rightFiringPoint = new Vector3(firingPoint.x, firingPoint.y, firingPoint.z) + rightOffet;

            // make the bullets move away in front of the player
            Vector3 forwardVector = Parent.transform.forward;
            Quaternion rightAngle = Quaternion.AngleAxis(angleSpread, Parent.transform.forward);
            Quaternion leftAngle = Quaternion.AngleAxis(-angleSpread, Parent.transform.forward);

            // create the bullet objects from the bullet prefab
            GameObject leftBullet = Instantiate(projectilePrefab, leftFiringPoint, leftAngle) as GameObject;
            GameObject rightBullet = Instantiate(projectilePrefab, rightFiringPoint, rightAngle) as GameObject;

            Projectile projectile = leftBullet.GetComponent<Projectile>();

            leftBullet.GetComponentInChildren<Rigidbody>().velocity = (forwardVector + rightAngle.eulerAngles) * projectile.MaxVelocity;
            rightBullet.GetComponentInChildren<Rigidbody>().velocity = (forwardVector + leftAngle.eulerAngles) * projectile.MaxVelocity;

            // spawn the bullets on the clients
            NetworkServer.Spawn(leftBullet);
            NetworkServer.Spawn(rightBullet);

            firingSound.Play();

            // make bullet disappear after 10 seconds
            Destroy(leftBullet, 10.0f); // testing
            Destroy(rightBullet, 10.0f);
        }
    }
}