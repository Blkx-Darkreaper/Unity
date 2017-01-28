using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class Weapon : Equipment
    {
        public Transform shotSpawn;
        public float attackDelay;
        private const string FIRE = "Fire";
        //public BallisticWeapon weapon;

        public Weapon(int width, int height) : base(width, height) { }

        //public void Start()
        //{
        //    InvokeRepeating(FIRE, attackDelay, weapon.cooldown);
        //}

        //private void Fire()
        //{
        //    Instantiate(weapon.projectileType, shotSpawn.position, shotSpawn.rotation);
        //    var blasterSound = GetComponent<AudioSource>();
        //    blasterSound.Play();
        //}
    }
}