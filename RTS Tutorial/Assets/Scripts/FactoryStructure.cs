using UnityEngine;
using System.Collections;

public class FactoryStructure : StructureController {

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Tank" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        AddUnitToBuildQueue(actionToPerform);
    }
}