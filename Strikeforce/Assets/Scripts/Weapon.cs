﻿using UnityEngine;
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
        public struct Types
        {
            public const string SHOT = "Shot";
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
        }

        public void Fire()
        {
            if (Cooldown > 0)
            {
                return;
            }

            // Check weapon has sufficient energy
            if (Parent.CurrentEnergy < EnergyCost)
            {
                return;
            }

            Parent.UpdateEnergy(EnergyCost);
            this.CurrentStatus = Equipment.Status.RECHARGING;

            Debug.Log(string.Format("{0} fired!", Type));

            //// create the bullet object from the bullet prefab
            //GameObject bullet = (GameObject)Instantiate(
            //    NetworkManager.singleton.spawnPrefabs[0],
            //    Parent.transform.position + Parent.transform.forward,
            //    Quaternion.identity);

            //// make the bullet move away in front of the player
            //bullet.GetComponentInChildren<Rigidbody>().velocity = Parent.transform.forward * 4;

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