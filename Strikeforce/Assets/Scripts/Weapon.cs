using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class Weapon : Equipment
    {
        public string Type;
        public int Priority;
        public bool IsOrdnanceWeapon = false;
        private const string FIRE = "Fire";

        public Weapon(int width, int height) : base(width, height) {
            this.IsWeapon = true;
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