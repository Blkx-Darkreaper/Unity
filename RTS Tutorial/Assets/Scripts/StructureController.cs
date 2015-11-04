using UnityEngine;
using System.Collections.Generic;
using RTS;

public class StructureController : EntityController {

    protected Queue<string> buildQueue;
    private float currentBuildProgress = 0f;
    public float buildComplete;
    private Vector3 spawnPoint;

    protected override void Awake()
    {
        base.Awake();
        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0f, spawnZ);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        BuildUnits();
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }

    protected void BuildUnit(string unitName)
    {
        buildQueue.Enqueue(unitName);
    }

    protected void BuildUnits()
    {
        if (buildQueue.Count <= 0)
        {
            return;
        }

        currentBuildProgress += Time.deltaTime * ResourceManager.buildSpeed;
        if (currentBuildProgress < buildComplete)
        {
            return;
        }
        if (owner == null)
        {
            return;
        }

        string unitName = buildQueue.Dequeue();
        owner.SpawnUnit(unitName, spawnPoint, transform.rotation);
        currentBuildProgress = 0f;
    }

    public string[] GetBuildQueueEntries()
    {
        string[] entries = buildQueue.ToArray();
        return entries;
    }

    public float GetBuildCompletionPercentage()
    {
        float completionPercentage = currentBuildProgress / buildComplete;
        return completionPercentage;
    }
}