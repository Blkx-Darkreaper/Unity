using UnityEngine;
using System;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class ResourceController : EntityController
{

    public float startingAmount;
    protected float currentAmount;
    public ResourceType type { get; protected set; }
    public bool isEmpty
    {
        get
        {
            if (currentAmount <= 0)
            {
                return true;
            }

            return false;
        }
    }
    protected struct ResourceProperties {
        public const string CURRENT_AMOUNT = "CurrentAmount";
    }

    protected override void Start()
    {
        base.Start();
        type = ResourceType.unknown;

        if (isLoadedFromSave == true)
        {
            return;
        }

		currentAmount = startingAmount;
    }

    protected override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);

        SaveManager.SaveFloat(writer, ResourceProperties.CURRENT_AMOUNT, currentAmount);
    }

    protected override bool LoadDetails(JsonReader reader, string propertyName)
    {
        // Properties must be loaded in the order they were saved for loadCompleted to work properly
        bool loadCompleted = false;

        base.LoadDetails(reader, propertyName);

        switch (propertyName)
        {
            case ResourceProperties.CURRENT_AMOUNT:
                currentAmount = LoadManager.LoadFloat(reader);
                loadCompleted = true;   // Last property to load
                break;
        }

        return loadCompleted;
    }

    public void Harvest(float amountToHarvest)
    {
        currentAmount -= amountToHarvest;
        currentAmount = Mathf.Clamp(currentAmount, 0, float.MaxValue);
    }

    protected override void UpdateHealthPercentage()
    {
        healthPercentage = currentAmount / startingAmount;
        healthStyle.normal.background = ResourceManager.GetResourceBarTexture(type);
    }
}
