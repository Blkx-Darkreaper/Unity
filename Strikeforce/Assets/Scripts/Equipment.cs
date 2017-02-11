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
        public HardpointPosition EquippedHardpoint { get; protected set; }
        public float Cost;
        public int Level = 1;
        public float EnergyCost;
        public float Cooldown;
        public string CurrentStatus { get; protected set;}
        public struct Status
        {
            public const string READY = "Ready";
            public const string RECHARGING = "Recharging";
            public const string DISABLED = "Disabled";
        }

        protected virtual void Start() {
            GameManager.Singleton.RegisterEntity(this);
        }

        protected override void Awake()
        {
            this.CurrentStatus = Status.READY;
        }

        protected virtual void Update()
        {
            if (CurrentStatus.Equals(Status.DISABLED))
            {
                return;
            }

            if(Cooldown > 0) {
                return;
            }

            this.CurrentStatus = Status.READY;
        }

        public void Equip(Raider parent, HardpointPosition hardpoint)
        {
            this.Parent = parent;
            this.EquippedHardpoint = hardpoint;
        }
    }
}