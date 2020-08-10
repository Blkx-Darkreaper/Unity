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
        protected GameObject[] projectilePrefabs;
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

            this.isWeapon = true;
            firingSound = GetComponent<AudioSource>();

            LoadProjectilePrefab();
        }

        protected void LoadProjectilePrefab()
        {
            this.projectilePrefabs = GlobalAssets.GetProjectilePrefabs(ProjectileType);
        }

        public void SetFiringPointOffset(Vector3 offset)
        {
            this.firingPointOffset = offset;
        }

        [Command]
        public void CmdFireSimple()
        {
            CmdFire(0, 0, 1);
        }

        [Command]
        public void CmdFire(int angleSpread, int horizontalSpread, int groupingBonus)
        {
            // Command function is called from the client, but invoked on the server
            bool active = Activate();
            if (active == false)
            {
                return;
            }

            Debug.Log(string.Format("{0} fired!", Type));

            GameObject singleShotPrefab = projectilePrefabs[groupingBonus - 1]; //Grouping is 1 based

            Vector3 parentLocation = parent.transform.position;
            float x = parentLocation.x;
            float y = parentLocation.y;
            float z = parentLocation.z;
            Vector3 firingPoint = new Vector3(x, y, z) + firingPointOffset;

            //firingSound.Play();

            if (Mathf.Abs(angleSpread) > 0)
            {
                SplitShot(firingPoint, singleShotPrefab, angleSpread, horizontalSpread);
                return;
            }

            if(Mathf.Abs(horizontalSpread) > 0)
            {
                SplitShot(firingPoint, singleShotPrefab, angleSpread, horizontalSpread);
                return;
            }

            SingleShot(firingPoint, singleShotPrefab);
        }

        protected void SingleShot(Vector3 firingPoint, GameObject bulletPrefab)
        {
            // create the bullet object from the bullet prefab
            GameObject bullet = Instantiate(bulletPrefab, firingPoint, Quaternion.identity) as GameObject;

            Projectile projectile = bullet.GetComponent<Projectile>();

            // make the bullet move away in front of the player
            Vector3 angleVector = parent.transform.forward;

            bullet.GetComponentInChildren<Rigidbody>().velocity = angleVector * projectile.maxVelocity;

            // spawn the bullet on the clients
            NetworkServer.Spawn(bullet);

            // make bullet disappear after 10 seconds
            Destroy(bullet, 10.0f); // testing
        }

        protected void SplitShot(Vector3 firingPoint, GameObject bulletPrefab, int angleSpread, int horizontalSpread)
        {
            Vector3 rightOffet = parent.transform.right * 0.1f;
            Vector3 leftOffset = -rightOffet;

            Vector3 leftFiringPoint = new Vector3(firingPoint.x, firingPoint.y, firingPoint.z) + leftOffset;
            Vector3 rightFiringPoint = new Vector3(firingPoint.x, firingPoint.y, firingPoint.z) + rightOffet;

            // create the bullet objects from the bullet prefab
            GameObject leftBullet = Instantiate(bulletPrefab, leftFiringPoint, Quaternion.identity) as GameObject;
            GameObject rightBullet = Instantiate(bulletPrefab, rightFiringPoint, Quaternion.identity) as GameObject;

            Projectile projectile = leftBullet.GetComponent<Projectile>();

            // make the bullets move away in front of the player
            Vector3 angleVector = parent.transform.forward;
            Quaternion rightAngle = Quaternion.AngleAxis(angleSpread, parent.transform.forward);
            Quaternion leftAngle = Quaternion.AngleAxis(-angleSpread, parent.transform.forward);

            leftBullet.GetComponentInChildren<Rigidbody>().velocity = (angleVector + rightAngle.eulerAngles) * projectile.maxVelocity;
            rightBullet.GetComponentInChildren<Rigidbody>().velocity = (angleVector + leftAngle.eulerAngles) * projectile.maxVelocity;

            // spawn the bullets on the clients
            NetworkServer.Spawn(leftBullet);
            NetworkServer.Spawn(rightBullet);

            // make bullet disappear after 10 seconds
            Destroy(leftBullet, 10.0f); // testing
            Destroy(rightBullet, 10.0f);
        }
    }
}