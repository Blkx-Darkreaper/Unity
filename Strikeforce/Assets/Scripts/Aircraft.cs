using UnityEngine;
using UnityEngine.Networking;
using Strikeforce;

namespace Strikeforce
{
    public class Aircraft : Vehicle
    {
        public int StallSpeed = 3;
        [SyncVar]
        protected float altitude;
        public const int MAX_ALTITUDE = 5;
        [SyncVar]
        public bool IsFalling = false;
        public AudioSource LandingSound;

        protected override void Awake()
        {
            base.Awake();

            this.altitude = 0f;
        }

        protected override void Update()
        {
            if (IsFalling == false)
            {
                return;
            }

            ChangeAltitude(-Time.deltaTime);
        }

        protected void TakeOff()
        {
            if (isServer == false)
            {
                return;
            }

            if (CurrentOrbit == Orbit.Air)
            {
                return;
            }

            if (CurrentMoveSpeed < StallSpeed)
            {
                return;
            }

            this.CurrentOrbit = Orbit.Air;
        }

        protected void Land()
        {
            if (isServer == false)
            {
                return;
            }

            if (CurrentOrbit != Orbit.Air)
            {
                return;
            }

            if (CurrentMoveSpeed > StallSpeed)
            {
                return;
            }

            this.CurrentOrbit = Orbit.Ground;

            if (LandingSound == null)
            {
                return;
            }

            LandingSound.Play();
        }

        public override void TakeDamage(int damage)
        {
            if (isServer == false)
            {
                return;
            }

            CurrentHitPoints -= damage;

            string ownersName = "Neutral";
            if (Owner != null)
            {
                ownersName = string.Format("{0}'s", Owner.PlayerId.ToString());
            }

            Debug.Log(string.Format("{0} {1} has taken {2} damage", ownersName, name, damage));

            if (CurrentHitPoints > 0)
            {
                return;
            }

            if (IsFalling == false)
            {
                ShootDown();
                return;
            }

            DestroyEntity();
        }

        public void ShootDown()
        {
            if (isServer == false)
            {
                return;
            }

            this.IsFalling = true;
            this.CurrentHitPoints = MaxHitPoints / 4;

            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                return;
            }

            rigidBody.useGravity = true;
        }

        protected void ChangeAltitude(float amount)
        {
            if (CurrentOrbit != Orbit.Air)
            {
                return;
            }

            this.altitude += amount;
            this.altitude = Mathf.Clamp(altitude, 0, MAX_ALTITUDE);

            if (IsFalling == false)
            {
                return;
            }

            if (altitude > 0)
            {
                return;
            }

            TakeDamage(Destructible.MAX_DAMAGE);
        }
    }
}