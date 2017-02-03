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
        private const string FIRE = "Fire";

        public Weapon(string type, int priority, int width, int height) : this(-1, type, priority, width, height) { }

        public Weapon(int id, string type, int priority, int width, int height) : this(id, null, type, priority, width, height) { }

        public Weapon(int id, Raider parent, string type, int priority, int width, int height) : base(id, parent, width, height) {
            this.Type = type;
            this.Priority = priority;
            this.IsWeapon = true;
        }

        public void Fire()
        {
            if (Cooldown > 0)
            {
                return;
            }

            // Check weapon has sufficient energy
            if (parent.CurrentEnergy < EnergyCost)
            {
                return;
            }

            parent.UseEnergy(EnergyCost);
            this.CurrentStatus = EquipmentStatus.RECHARGING;

            Debug.Log(string.Format("{0} fired!", Type));

            //// create the bullet object from the bullet prefab
            //GameObject bullet = (GameObject)Instantiate(
            //    NetworkManager.singleton.spawnPrefabs[0],
            //    parent.transform.position + parent.transform.forward,
            //    Quaternion.identity);

            //// make the bullet move away in front of the player
            //bullet.GetComponentInChildren<Rigidbody>().velocity = parent.transform.forward * 4;

            //// spawn the bullet on the clients
            //NetworkServer.Spawn(bullet);

            //// make bullet disappear after 2 seconds
            //Destroy(bullet, 2.0f);
        }

        //public void Start()
        //{
        //    InvokeRepeating(FIRE, AttackDelay, weapon.cooldown);
        //}

        //private void Fire()
        //{
        //    Instantiate(weapon.projectileType, ShotSpawn.position, ShotSpawn.rotation);
        //    var blasterSound = GetComponent<AudioSource>();
        //    blasterSound.Play();
        //}
    }
}