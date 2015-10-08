using UnityEngine;
using System.Collections;

public class Barrack : Building {

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Soldier" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }
}
