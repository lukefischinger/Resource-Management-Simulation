using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceBank : MonoBehaviour
{

    public Resources myResources = new Resources();
    public float Weight => myResources.Weight;
    public float weightCapacity = 100;
    public bool AtCapacity => myResources.Weight >= weightCapacity;

    public Action Full = new(() => { });
    public Action Empty = new(() => { });
    public Action Changed = new(() => { });

    public void Initialize(List<Resource> resources, List<float> quantities = null)
    {
        myResources = new Resources(resources, quantities);
        
    }

    public void Add(Resource resource, float amount)
    {
        myResources.Add(resource, amount);
        Changed();

    }

    public float Remove(Resource resource, float amount)
    {
        float removeAmount = myResources.Remove(resource, amount);     
        Changed();

        return removeAmount;
    }

    public bool Contains(Resource resource)
    {
        return myResources.Contains(resource);
    }

    

}

