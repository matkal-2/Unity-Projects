﻿using UnityEngine;
using System.Collections;
using BOXRTS;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public bool human;
    public string username;
    public HUD hud;
    public WorldObject SelectedObject { get; set; }

    // Resources Variables
    public int startMoney, startMoneyLimit, startPower, startPowerLimit;
    private Dictionary<ResourceType, int> resources, resourceLimits;

	// Use this for initialization
	void Start () {
        hud = GetComponentInChildren<HUD>();
        AddStartResourceLimits();
        AddStartResources();
	}
	
	// Update is called once per frame
	void Update () {
        if (human)
        {
            hud.SetResourceValues(resources, resourceLimits);
        }
	}

    void Awake()
    {
        resources = InitResourceList();
        resourceLimits = InitResourceList();
    }

    // -------      Resources       -------
    private Dictionary<ResourceType, int> InitResourceList()
    {
        Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
        list.Add(ResourceType.Money, 0);
        list.Add(ResourceType.Power, 0);
        return list;
    }

    private void AddStartResourceLimits()
    {
        IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
        IncrementResourceLimit(ResourceType.Power, startPowerLimit);
    }

    private void AddStartResources()
    {
        AddResource(ResourceType.Money, startMoney);
        AddResource(ResourceType.Power, startPower);
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
    }

    public void IncrementResourceLimit(ResourceType type, int amount)
    {
        resourceLimits[type] += amount;
    }
    // -------      End Resources       -------

    //  -------     Unit Prduction      -----
    public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation)
    {
        Units units = GetComponentInChildren<Units>();
        GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
        newUnit.transform.parent = units.transform;
    }
    //  -------     End Unit Prduction      -----
}