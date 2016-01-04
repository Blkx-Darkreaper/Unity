using UnityEngine;
using System.Collections;

public class RefineryStructure : StructureController {

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Harvester" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        AddUnitToBuildQueue(actionToPerform);
    }
}
