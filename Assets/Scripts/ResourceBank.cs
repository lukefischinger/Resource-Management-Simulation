using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBank : MonoBehaviour
{

    public Resources myResources = new Resources();
    public float Weight => myResources.Weight;
    public float weightCapacity = 100;
    public bool AtCapacity => myResources.Weight >= weightCapacity;

    public Action Full = new(() => { });
    public Action Empty = new(() => { });

    public void Initialize(List<Resource> resources, List<float> quantities = null)
    {
        myResources = new Resources(resources, quantities);
    }

    public void Add(Resource resource, float amount)
    {
        myResources.Add(resource, amount);
        if (AtCapacity)
        {
            Full();
        }
    }

    public float Remove(Resource resource, float amount)
    {
        float removeAmount = myResources.Remove(resource, amount);
        if (myResources.Weight == 0)
        {
            Empty.Invoke();
        }
        return removeAmount;
    }

    public bool Contains(Resource resource)
    {
        return myResources.Contains(resource);
    }

}

