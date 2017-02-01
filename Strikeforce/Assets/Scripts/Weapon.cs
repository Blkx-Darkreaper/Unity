using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class Weapon : Equipment
    {
        public string Type;
        public int Priority;
        public bool IsOrdnanceWeapon = false;
        public HardpointPosition EquippedHardpoint { get; protected set; }
        private const string FIRE = "Fire";

        public Weapon(string type, int priority, int width, int height) : this(-1, type, priority, width, height) { }

        public Weapon(int id, string type, int priority, int width, int height) : base(id, width, height) {
            this.Type = type;
            this.Priority = priority;
            this.IsWeapon = true;
        }

        public void Equip(HardpointPosition hardpoint)
        {
            this.EquippedHardpoint = hardpoint;
        }

        public void Fire()
        {

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