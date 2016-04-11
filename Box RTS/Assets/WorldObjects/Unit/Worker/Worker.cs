using UnityEngine;
using System.Collections;
using BOXRTS;

public class Worker : Unit
{
    public float capacity;

    private bool harvesting = false, emptying = false;
    private float currentLoad = 0.0f;
    private ResourceType harvestType;
    private Resource resourceDeposit;
    public Building resourceStore;
    public float collectionAmount, depositAmount;
    private float currentDeposit = 0.0f;

    public int buildSpeed;

    private Building currentProject;
    private bool building = false;
    private float amountBuilt = 0.0f;


    /*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Start()
    {
        base.Start();
        harvestType = ResourceType.Unknown;
        actions = new string[] { "Barrack", "TownHall" };
    }

    protected override void Update()
    {
        base.Update();
        if (!rotating && !moving)
        {
            if (building && currentProject && currentProject.UnderConstruction())
            {
                amountBuilt += buildSpeed * Time.deltaTime;
                int amount = Mathf.FloorToInt(amountBuilt);
                if (amount > 0)
                {
                    amountBuilt -= amount;
                    currentProject.Construct(amount);
                    if (!currentProject.UnderConstruction()) building = false;
                }
            }
            if (harvesting || emptying)
            {
                Pick[] picks = GetComponentsInChildren<Pick>();
                foreach (Pick pick in picks) pick.GetComponent<Renderer>().enabled = true;
                if (harvesting)
                {
                    Collect();
                    if (currentLoad >= capacity || resourceDeposit.isEmpty())
                    {
                        //make sure that we have a whole number to avoid bugs
                        //caused by floating point numbers
                        currentLoad = Mathf.Floor(currentLoad);
                        harvesting = false;
                        emptying = true;
                        foreach (Pick pick in picks) pick.GetComponent<Renderer>().enabled = false;
                        StartMove(resourceStore.transform.position, resourceStore.gameObject);
                    }
                }
                else
                {
                    Deposit();
                    if (currentLoad <= 0)
                    {
                        emptying = false;
                        foreach (Pick pick in picks) pick.GetComponent<Renderer>().enabled = false;
                        if (!resourceDeposit.isEmpty())
                        {
                            harvesting = true;
                            StartMove(resourceDeposit.transform.position, resourceDeposit.gameObject);
                        }
                    }
                }
            }
        }
    }


    /* Public Methods */

    public override void SetHoverState(GameObject hoverObject)
    {
        base.SetHoverState(hoverObject);
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected)
        {
            if (hoverObject.name != "WorldMap")
            {
                Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
                if (resource && !resource.isEmpty()) player.hud.SetCursorState(CursorState.Harvest);
            }
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {

        bool doBase = true;
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected && hitObject && hitObject.name != "WorldMap")
        {
            Building building = hitObject.transform.parent.GetComponent<Building>();
            if (building)
            {
                if (building.UnderConstruction())
                {
                    SetBuilding(building);
                    doBase = false;
                }
            }
        }
        if (doBase) base.MouseClick(hitObject, hitPoint, controller);
        //only handle input if owned by a human player
        if (player && player.human)
        {
            if (hitObject.name != "WorldMap")
            {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();
                if (resource && !resource.isEmpty())
                {
                    //make sure that we select harvester remains selected
                    if (player.SelectedObject) player.SelectedObject.SetSelection(false, playingArea);
                    SetSelection(true, playingArea);
                    player.SelectedObject = this;
                    StartHarvest(resource);
                }
            }
            else StopHarvest();
        }
        
    }

    /* Private Methods */

    private void StartHarvest(Resource resource)
    {
        resourceDeposit = resource;
        StartMove(resource.transform.position, resource.gameObject);
        //we can only collect one resource at a time, other resources are lost
        if (harvestType == ResourceType.Unknown || harvestType != resource.GetResourceType())
        {
            harvestType = resource.GetResourceType();
            currentLoad = 0.0f;
        }
        harvesting = true;
        emptying = false;
    }


    private void StopHarvest()
    {
        harvesting = false;
    }

    private void Collect()
    {
        float collect = collectionAmount * Time.deltaTime;
        //make sure that the harvester cannot collect more than it can carry
        if (currentLoad + collect > capacity) collect = capacity - currentLoad;
        resourceDeposit.Remove(collect);
        currentLoad += collect;
    }

    private void Deposit()
    {
        ResourceType depositType = harvestType;
        if (harvestType == ResourceType.Ore) depositType = ResourceType.Money;
        player.AddResource(depositType, Mathf.FloorToInt(currentLoad));
        currentLoad = 0;
    }

    protected override void DrawSelectionBox(Rect selectBox)
    {
        base.DrawSelectionBox(selectBox);
        float percentFull = currentLoad / capacity;
        float maxHeight = selectBox.height - 4;
        float height = maxHeight * percentFull;
        float leftPos = selectBox.x + selectBox.width - 7;
        float topPos = selectBox.y + 2 + (maxHeight - height);
        float width = 5;
        Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
        if (resourceBar) GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
    }

    private void CreateBuilding(string buildingName)
    {
        Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
        if (player) player.createBuilding(buildingName, buildPoint, this, playingArea);
    }


    public override void SetBuilding(Building project)
    {
        base.SetBuilding(project);
        currentProject = project;
        StartMove(currentProject.transform.position, currentProject.gameObject);
        building = true;
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateBuilding(actionToPerform);
    }

    public override void StartMove(Vector3 destination)
    {
        base.StartMove(destination);
        amountBuilt = 0.0f;
        building = false;
    }



}