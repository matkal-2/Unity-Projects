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



    /*** Game Engine methods, all can be overridden by subclass ***/

    protected override void Start()
    {
        base.Start();
        harvestType = ResourceType.Unknown;
    }

    protected override void Update()
    {
        base.Update();
        if (!rotating && !moving)
        {
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
        base.MouseClick(hitObject, hitPoint, controller);
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

    }

    private void Collect()
    {
    }

    private void Deposit()
    {
    }


}