using UnityEngine;
using UnityEngine.Networking;
using System;

namespace Strikeforce
{
    public class Aircraft : Vehicle
    {
        public int StallSpeed = 3;
        public float MaxBank = 30f;
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

        public void TakeOff()
        {
            if (isServer == false)
            {
                return;
            }

            if (IsAirborne == true)
            {
                return;
            }

            Rigidbody rigidbody = gameObject.GetComponentInChildren<Rigidbody>();
            float velocity = rigidbody.velocity.z;
            if (velocity < StallSpeed)
            {
                return;
            }

            this.IsAirborne = true;
        }

        public void Land()
        {
            if (isServer == false)
            {
                return;
            }

            if (IsAirborne == false)
            {
                return;
            }

            if (MaxVelocity > StallSpeed)
            {
                return;
            }

            this.IsAirborne = false;

            if (LandingSound == null)
            {
                return;
            }

            LandingSound.Play();
        }

        public void BankLeft(float amount)
        {
            Bank(Vector3.left, amount);
        }

        public void BankRight(float amount)
        {
            Bank(Vector3.right, amount);
        }

        protected void Bank(Vector3 direction, float amount)
        {
            amount = Mathf.Clamp(amount, -MaxBank, MaxBank);

            transform.Rotate(direction * amount);
        }

        public void PitchUp(float amount)
        {
            Pitch(Vector3.up, amount);
        }

        public void PitchDown(float amount)
        {
            Pitch(Vector3.down, amount);
        }

        protected void Pitch(Vector3 direction, float amount)
        {
            transform.Rotate(direction * amount);
        }

        public override void TakeDamage(int amount)
        {
            if (isServer == false)
            {
                return;
            }

            CurrentHitPoints -= amount;

            string ownersName = "Neutral";
            if (Owner != null)
            {
                ownersName = string.Format("{0}'s", Owner.PlayerId.ToString());
            }

            Debug.Log(string.Format("{0} {1} has taken {2} damage", ownersName, name, amount));

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

        public void ChangeAltitude(float amount)
        {
            if (IsAirborne == false)
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