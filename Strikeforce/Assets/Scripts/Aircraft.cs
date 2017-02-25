using UnityEngine;
using UnityEngine.Networking;
using System;

namespace Strikeforce
{
    public class Aircraft : Vehicle
    {
        public int StallSpeed = 3;
        [SyncVar]
        public float BankAngle = 0f;
        public float MaxBank = 30f;
        [SyncVar]
        public float PitchAngle = 0f;
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

            if (CurrentMoveSpeed < StallSpeed)
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

            if (CurrentMoveSpeed > StallSpeed)
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
            Bank(-amount);
        }

        public void BankRight(float amount)
        {
            Bank(amount);
        }

        protected void Bank(float amount)
        {
            float amount = Mathf.Clamp(amount, -MaxBank, MaxBank);

            this.BankAngle += amount;

            if (Math.Abs(BankAngle) <= 180)
            {
                return;
            }

            if (BankAngle < 0)
            {
                this.BankAngle += 180;
                return;
            }

            this.BankAngle += -180;
        }

        public void PitchUp(float amount)
        {
            Pitch(amount);
        }

        public void PitchDown(float amount)
        {
            Pitch(-amount);
        }

        protected void Pitch(float amount)
        {
            this.PitchAngle += amount;

            if (Math.Abs(PitchAngle) <= 90)
            {
                return;
            }

            // TODO
            throw new NotImplementedException("Pitch handling incomplete");
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