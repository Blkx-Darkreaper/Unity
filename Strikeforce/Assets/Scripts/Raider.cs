using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Raider : Aircraft
    {
        public float MaxEnergy;
        public float CurrentEnergy { get; protected set; }
        public Hardpoint Center { get; protected set; }
        public Hardpoint LeftWing { get; protected set; }
        public Hardpoint RightWing { get; protected set; }
        public Hardpoint LeftOuterWing { get; protected set; }
        public Hardpoint RightOuterWing { get; protected set; }
        public TriggerLink PrimaryFire { get; protected set; }
        public TriggerLink SecondaryFire { get; protected set; }
        public TriggerLink SpecialFire { get; protected set; }  // Only Ordnance weapons can be linked
        public Equipment EquippedItem { get; protected set; }

        public void Start()
        {
            this.PrimaryFire = new TriggerLink();
            this.SecondaryFire = new TriggerLink();
            this.SpecialFire = new TriggerLink();
            this.EquippedItem = null;
        }

        public void Init(float maxEnergy, Hardpoint[] hardpoints)
        {
            this.MaxEnergy = maxEnergy;
            this.CurrentEnergy = maxEnergy;

            // Hardpoints Left 0 1 2 3 4 Right
            this.Center = hardpoints[2];
            this.LeftWing = hardpoints[1];
            this.RightWing = hardpoints[3];
            this.LeftOuterWing = hardpoints[0];
            this.RightOuterWing = hardpoints[4];
        }

        public void Fire()  // Testing
        {
            //// create the bullet object from the bullet prefab
            //GameObject bullet = (GameObject)Instantiate(
            //    NetworkManager.singleton.spawnPrefabs[0],
            //    transform.position + transform.forward,
            //    Quaternion.identity);

            //// make the bullet move away in front of the player
            //bullet.GetComponentInChildren<Rigidbody>().velocity = transform.forward * 4;

            //// spawn the bullet on the clients
            //NetworkServer.Spawn(bullet);

            //// make bullet disappear after 2 seconds
            //Destroy(bullet, 2.0f);
            PrimaryFire.IsFiring = true;
        }

        public void TogglePrimaryFire()
        {
            PrimaryFire.IsFiring = !PrimaryFire.IsFiring;
        }

        public void Update()
        {
            PrimaryFire.IsFiring = false;
        }

        public void UseEnergy(float amount)
        {
            this.CurrentEnergy -= amount;
        }

        public bool EquipWeapon(Weapon weapon, HardpointPosition hardpointPosition, int row, int column)
        {
            Hardpoint hardpoint = null;
            switch (hardpointPosition)
            {
                case HardpointPosition.LeftOuterWing:
                    hardpoint = LeftOuterWing;
                    break;

                case HardpointPosition.LeftWing:
                    hardpoint = LeftWing;
                    break;

                case HardpointPosition.Center:
                    hardpoint = Center;
                    break;

                case HardpointPosition.RightWing:
                    hardpoint = RightWing;
                    break;

                case HardpointPosition.RightOuterWing:
                    hardpoint = RightOuterWing;
                    break;
            }

            bool isEquippable = hardpoint.IsEquippable(weapon, row, column);
            if (isEquippable == false)
            {
                return false;
            }

            hardpoint.Equip(weapon, row, column);
            this.PrimaryFire.LinkWeapon(weapon);

            return true;
        }

        public class TriggerLink : MonoBehaviour
        {
            public string DominantWeaponType { get; protected set; }
            public float AngledSpread { get; protected set; }
            public float HorizontalSpread { get; protected set; }
            public int FiringGroups { get; protected set; }
            public int GroupingSize { get; protected set; }
            protected LinkedList<Weapon> allLinkedWeapons { get; set; }
            protected List<FiringGroup> allFiringGroups { get; set; }
            protected Dictionary<string, int> allWeaponTypes { get; set; }
            public float CyclePeriod = 1f;
            protected float cycleRemaining { get; set; }
            protected int currentFiringGroup { get; set; }
            public bool IsFiring { get; set; }

            public TriggerLink()
            {
                this.DominantWeaponType = string.Empty;
                this.AngledSpread = 0f;
                this.HorizontalSpread = 0f;
                this.GroupingSize = 1;
                this.allLinkedWeapons = new LinkedList<Weapon>();
                this.allWeaponTypes = new Dictionary<string, int>();
                this.cycleRemaining = this.CyclePeriod;
                this.currentFiringGroup = -1;
                this.IsFiring = false;
            }

            public void LinkWeapon(Weapon weapon)
            {
                this.allLinkedWeapons.AddFirst(weapon);
                
                bool hasType = this.allWeaponTypes.ContainsKey(weapon.Type);
                if (hasType == false)
                {
                    this.allWeaponTypes.Add(weapon.Type, 1);
                }
                else
                {
                    this.allWeaponTypes[weapon.Type]++;
                }

                SetDominantWeaponType();
                SetAngledSpread();
            }

            protected void SetDominantWeaponType()
            {
                int maxValue = 0;

                foreach (string type in allWeaponTypes.Keys)
                {
                    Weapon weapon = GlobalAssets.Weapons[type];
                    int priority = weapon.Priority;
                    int quantity = this.allWeaponTypes[type];

                    int triples = quantity / 3;
                    int pairs = (quantity - 3 * triples) / 2;
                    int value = 100 * triples + 10 * pairs + priority;

                    if (value < maxValue)
                    {
                        continue;
                    }

                    maxValue = value;
                    this.DominantWeaponType = type;
                }
            }

            protected void SetAngledSpread()
            {
                if(DominantWeaponType.Equals(WeaponTypes.BOLT) == true) {
                    return;
                }

                if (allWeaponTypes.ContainsKey(WeaponTypes.BOLT) == false)
                {
                    return;
                }

                int quantity = allWeaponTypes[WeaponTypes.BOLT];
                this.AngledSpread = quantity * 0.05f;
            }

            protected void SetHorizontalSpread()
            {
                if (DominantWeaponType.Equals(WeaponTypes.FLAMEBURST) == true)
                {
                    return;
                }

                if(allWeaponTypes.ContainsKey(WeaponTypes.FLAMEBURST) == false) {
                    return;
                }

                int quantity = allWeaponTypes[WeaponTypes.FLAMEBURST];
                this.HorizontalSpread = quantity * .1f;
            }

            protected void SetFiringGroups()
            {
                int quantity = 0;
                if (allWeaponTypes.ContainsKey(WeaponTypes.WAVE) == true)
                {
                    quantity = allWeaponTypes[WeaponTypes.WAVE];
                }

                this.FiringGroups = quantity + 1;

                if (allLinkedWeapons.Count == 0)
                {
                    return;
                }

                allFiringGroups = new List<FiringGroup>(this.FiringGroups);

                // Sort equipped weapons
                PriorityQueue<int, Weapon>[] sortedWeapons = new PriorityQueue<int, Weapon>[5];

                foreach (Weapon weapon in allLinkedWeapons)
                {
                    int hardpoint = (int)weapon.EquippedHardpoint;

                    PriorityQueue<int, Weapon> hardpointWeapons = sortedWeapons[hardpoint];
                    if (hardpointWeapons == null)
                    {
                        hardpointWeapons = new PriorityQueue<int, Weapon>();
                        sortedWeapons[hardpoint] = hardpointWeapons;
                    }

                    int priority = weapon.Priority;
                    hardpointWeapons.Enqueue(priority, weapon);
                }

                // Place sorted weapons into firing groups
                for (int i = 0; i < this.FiringGroups; i++)
                {
                    SortedList<int, Weapon> sortedGroup = new SortedList<int, Weapon>();

                    int[] pattern = FiringGroupPatterns.GetFiringPattern(this.FiringGroups, i);
                    foreach (int hardpoint in pattern)
                    {
                        PriorityQueue<int, Weapon> hardpointWeapons = sortedWeapons[hardpoint];
                        Weapon weapon = hardpointWeapons.Dequeue();

                        int priority = weapon.Priority;
                        sortedGroup.Add(priority, weapon);
                    }

                    FiringGroup currentGroup = new FiringGroup(sortedGroup);
                    allFiringGroups.Add(currentGroup);
                }
            }

            //protected void SetGroupingSize()
            //{
            //    if (DominantWeaponType.Equals(WeaponTypes.CANNON) == true)
            //    {
            //        return;
            //    }

            //    if (allWeaponTypes.ContainsKey(WeaponTypes.CANNON) == false)
            //    {
            //        return;
            //    }

            //    int quantity = allWeaponTypes[WeaponTypes.CANNON];
            //    this.GroupingSize = quantity + 1;
            //}

            public void Update()
            {
                float timeElapsed = Time.deltaTime;

                if (IsFiring == false)
                {
                    if (cycleRemaining == CyclePeriod)
                    {
                        return;
                    }
                }

                cycleRemaining -= timeElapsed;
                if (cycleRemaining <= 0)
                {
                    cycleRemaining += CyclePeriod;

                    currentFiringGroup++;
                    currentFiringGroup %= allFiringGroups.Count;
                }

                if (IsFiring == false)
                {
                    return;
                }

                FireGroup(currentFiringGroup, CyclePeriod, cycleRemaining);
            }

            public void FireGroup(int groupIndex, float cyclePeriod, float periodRemaining)
            {
                FiringGroup currentGroup = allFiringGroups[groupIndex];

                int totalSequences = currentGroup.Count;

                float periodElapsed = cyclePeriod - periodRemaining;

                int sequenceIndex = (int)Math.Round(totalSequences * periodElapsed / cyclePeriod, 0);
                currentGroup.FireSequence(sequenceIndex);
            }

            protected class FiringGroup
            {
                protected List<LinkedList<Weapon>> allSequences { get; set; }
                public int Count { get { return allSequences.Count; } }

                public FiringGroup(SortedList<int, Weapon> sortedWeapons)
                {
                    this.allSequences = new List<LinkedList<Weapon>>();

                    int currentPriority = 0;
                    LinkedList<Weapon> currentSequence = null;
                    foreach (KeyValuePair<int, Weapon> pair in sortedWeapons)
                    {
                        int priority = pair.Key;
                        Weapon weapon = pair.Value;

                        if (priority != currentPriority)
                        {
                            currentSequence = new LinkedList<Weapon>();
                            this.allSequences.Add(currentSequence);

                            currentPriority = priority;
                        }

                        currentSequence.AddLast(weapon);
                    }
                }

                public void FireSequence(int sequenceIndex)
                {
                    LinkedList<Weapon> sequence = allSequences[sequenceIndex];

                    foreach (Weapon weapon in sequence)
                    {
                        weapon.Fire();
                    }
                }
            }
        }

        protected override void DestroyEntity()
        {
            base.DestroyEntity();

            Debug.Log(string.Format("You have been destroyed."));
        }
    }
}