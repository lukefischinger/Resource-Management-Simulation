using System.Collections.Generic;
using UnityEngine;

public class ResourceBank : MonoBehaviour
{
	
	public Resources myResources = new Resources();
	public float Weight => myResources.Weight;
	public float weightCapacity = 100;
	public bool AtCapacity => myResources.Weight >= weightCapacity;

	public void Initialize(List<Resource> resources, List<float> quantities = null) 
	{
		myResources = new Resources(resources, quantities);
	}

	public void Add(Resource resource, float amount) 
	{
		myResources.Add(resource, amount);
	}
	
	public float Remove(Resource resource, float amount) 
	{
		return myResources.Remove(resource, amount);
	}
	
	public bool Contains(Resource resource) 
	{
		return myResources.Contains(resource);
	}

}
