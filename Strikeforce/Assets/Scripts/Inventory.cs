using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public class Inventory : MonoBehaviour
    {
        protected List<Raider> raiders;
        protected List<Equipment> equipment;
        public int StartingMoney, StartingMoneyLimit, StartingFuel, StartingFuelLimit;
        protected Dictionary<ResourceType, int> resources, resourceLimits;

        protected void Awake()
        {
            raiders = new List<Raider>();
            equipment = new List<Equipment>();
            InitResourceLists();
        }

        private void InitResourceLists()
        {
            resources = new Dictionary<ResourceType, int>();
            resources.Add(ResourceType.money, 0);
            resources.Add(ResourceType.fuel, 0);

            resourceLimits = new Dictionary<ResourceType, int>();
            resourceLimits.Add(ResourceType.money, StartingMoneyLimit);
            resourceLimits.Add(ResourceType.fuel, StartingFuelLimit);

            AddResource(ResourceType.money, StartingMoney);
            AddResource(ResourceType.fuel, StartingFuel);
        }

        public void InsufficientResources(ResourceType type)
        {
            Debug.Log(string.Format("{0} has insufficient {1}", name, type.ToString()));
        }

        public void AddResource(ResourceType type, int amountToAdd)
        {
            resources[type] += amountToAdd;
            if (resourceLimits.ContainsKey(type) == false)
            {
                return;
            }

            resources[type] = Mathf.Clamp(resources[type], 0, resourceLimits[type]);
        }

        public bool HasSufficientResources(ResourceType type, int amountRequired)
        {
            int currentAmount = resources[type];

            bool sufficientResources = amountRequired <= currentAmount;
            return sufficientResources;
        }

        public void RemoveResource(ResourceType type, int amountToRemove)
        {
            resources[type] -= amountToRemove;
            resources[type] = Mathf.Clamp(resources[type], 0, resourceLimits[type]);
        }

        public void SetResourceLimit(ResourceType type, int limit)
        {
            resourceLimits[type] = limit;
        }

        public int GetResourceAmount(ResourceType type)
        {
            int amount = resources[type];
            return amount;
        }

        public int GetMaxResourceAmount(ResourceType type)
        {
            int amount = resourceLimits[type];
            return amount;
        }
    }
}