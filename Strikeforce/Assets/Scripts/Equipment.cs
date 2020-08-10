using UnityEngine;

namespace Strikeforce
{
    public class Equipment : Entity
    {
        public int width;
        public int height;
        public string description = string.Empty;
        public bool isRemovable = true;
        public bool isWeapon = false;
        public bool isActive { get; protected set; }
        public Raider parent;
        public float cost;
        public int level = 1;
        public float energyCost;
        public float cooldown;
        protected float cooldownRemaining = 0f;
        public string currentStatus { get; protected set; }
        public struct Status
        {
            public const string READY = "Ready";
            public const string RECHARGING = "Recharging";
            public const string DISABLED = "Disabled";
        }

        protected override void Awake()
        {
            this.currentStatus = Status.READY;
        }

        protected virtual void Start()
        {
            GameEntityManager.singleton.RegisterEntity(this);
        }

        protected virtual void Update()
        {
            if (cooldownRemaining <= 0)
            {
                this.cooldownRemaining = 0;
                return;
            }

            this.cooldownRemaining -= Time.deltaTime;

            if (currentStatus.Equals(Status.DISABLED))
            {
                return;
            }

            if (cooldownRemaining > 0)
            {
                return;
            }

            this.currentStatus = Status.READY;

            Use(isActive);
        }

        public virtual bool Activate()
        {
            if (cooldownRemaining > 0)
            {
                return false;
            }

            // Check equipment has sufficient energy
            if (parent.CurrentEnergy < energyCost)
            {
                return false;
            }

            this.parent.UpdateEnergy(energyCost);
            this.currentStatus = Equipment.Status.RECHARGING;
            this.cooldownRemaining = cooldown;

            return true;
        }

        public virtual void Use()
        {
            Use(true);

            this.isActive = false;
        }

        public virtual void Use(bool isActive)
        {
            if (isActive == false)
            {
                this.isActive = false;
                return;
            }

            this.isActive = Activate();
            if (this.isActive == false)
            {
                return;
            }

            // Do something
        }
    }
}