using UnityEngine;
using System.Drawing;
using System.Collections;

namespace Strikeforce
{
    public class Equipment : Entity
    {
        public int Width;
        public int Height;
        public string Description = string.Empty;
        public bool IsRemovable = true;
        public bool IsWeapon = false;
        public Raider Parent;
        public float Cost;
        public int Level = 1;
        public float EnergyCost;
        public float Cooldown;
        protected float cooldownRemaining = 0f;
        public string CurrentStatus { get; protected set; }
        public struct Status
        {
            public const string READY = "Ready";
            public const string RECHARGING = "Recharging";
            public const string DISABLED = "Disabled";
        }

        protected override void Awake()
        {
            this.CurrentStatus = Status.READY;
        }

        protected virtual void Start()
        {
            GameManager.Singleton.RegisterEntity(this);
        }

        protected virtual void Update()
        {
            cooldownRemaining -= Time.deltaTime;

            if (CurrentStatus.Equals(Status.DISABLED))
            {
                return;
            }

            if (cooldownRemaining > 0)
            {
                return;
            }

            this.CurrentStatus = Status.READY;
        }

        public virtual bool Activate()
        {
            if (cooldownRemaining > 0)
            {
                return false;
            }

            // Check equipment has sufficient energy
            if (Parent.CurrentEnergy < EnergyCost)
            {
                return false;
            }

            Parent.UpdateEnergy(EnergyCost);
            this.CurrentStatus = Equipment.Status.RECHARGING;
            this.cooldownRemaining = Cooldown;

            return true;
        }

        public virtual void Use()
        {
            bool active = Activate();
            if (active == false)
            {
                return;
            }

            // Do something
        }
    }
}