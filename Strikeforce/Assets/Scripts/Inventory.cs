using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Strikeforce
{
    public enum ResourceType
    {
        Money, Fuel, Rockets, Missiles, Bombs, Materiel, Unknown
    }

    public struct ResourceProperties
    {
        public const string MONEY = "Money";
        public const string MONEY_LIMIT = "MoneyLimit";
        public const string FUEL = "Fuel";
        public const string FUEL_LIMIT = "FuelLimit";
        public const string ROCKETS = "Rockets";
        public const string ROCKETS_LIMIT = "RocketsLimit";
        public const string MISSILES = "Missiles";
        public const string MISSILES_LIMIT = "MissilesLimit";
        public const string BOMBS = "Bombs";
        public const string BOMBS_LIMIT = "BombsLimit";
        public const string MATERIEL = "Materiel";
        public const string MATERIEL_LIMIT = "MaterielLimit";
        public const string UNKNOWN = "Unknown";
    }

    public class Inventory : MonoBehaviour
    {
        public List<Raider> AllRaiders;
        public List<Equipment> AllEquipment;
        public int StartingMoney = 1000, MoneyLimit = 9999999, StartingFuel = 500, FuelLimit = 9999,
            StartingRockets = 0, RocketLimit = 9999, StartingMissiles = 0, MissileLimit = 9999, StartingBombs = 0, BombLimit = 9999;
        protected Dictionary<ResourceType, int> resources, resourceLimits;

        protected void Awake()
        {
            AllRaiders = new List<Raider>();
            AllEquipment = new List<Equipment>();
            InitResourceLists();
        }

        private void InitResourceLists()
        {
            resources = new Dictionary<ResourceType, int>();
            resources.Add(ResourceType.Money, 0);
            resources.Add(ResourceType.Fuel, 0);
            resources.Add(ResourceType.Rockets, 0);
            resources.Add(ResourceType.Missiles, 0);
            resources.Add(ResourceType.Bombs, 0);

            resourceLimits = new Dictionary<ResourceType, int>();
            SetResourceLimit(ResourceType.Money, MoneyLimit);
            SetResourceLimit(ResourceType.Fuel, FuelLimit);
            SetResourceLimit(ResourceType.Rockets, RocketLimit);
            SetResourceLimit(ResourceType.Missiles, MissileLimit);
            SetResourceLimit(ResourceType.Bombs, BombLimit);

            UpdateResource(ResourceType.Money, StartingMoney);
            UpdateResource(ResourceType.Fuel, StartingFuel);
            UpdateResource(ResourceType.Rockets, StartingRockets);
            UpdateResource(ResourceType.Missiles, StartingMissiles);
            UpdateResource(ResourceType.Bombs, StartingBombs);
        }

        public void InsufficientResources(ResourceType type)
        {
            Debug.Log(string.Format("{0} has insufficient {1}", name, type.ToString()));
        }

        public int UpdateResource(ResourceType type, int amount)
        {
            int currentAmount = resources[type];

            int maxAmount = int.MaxValue;
            if (resourceLimits.ContainsKey(type) == true)
            {
                maxAmount = resourceLimits[type];
            }

            int adjustedAmount = Mathf.Clamp(currentAmount + amount, 0, maxAmount);

            int remainder = amount - adjustedAmount + currentAmount;
            return remainder;
        }

        public bool HasSufficientResources(ResourceType type, int amountRequired)
        {
            int currentAmount = resources[type];

            bool sufficientResources = amountRequired <= currentAmount;
            return sufficientResources;
        }

        public void SetResourceLimit(ResourceType type, int limit)
        {
            if (resourceLimits.ContainsKey(type) == false)
            {
                resourceLimits.Add(type, limit);
                return;
            }

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