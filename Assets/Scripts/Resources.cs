using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Resources
{
	Dictionary<Resource, float> package;
	public Dictionary<Resource, float> Package
	{
		get
		{
			return package;
		}
		private set
		{
			package = value;
		}
	}
	public float Weight { get; private set; }

	public int Count => Package.Count;

	public Resources(Dictionary<Resource, float> dict)
	{
		Package = new Dictionary<Resource, float>();
		InitializeResources(dict.Keys.ToList());
		
		Add(dict);
	}

	public Resources(List<Resource> resources, List<float> quantities = null)
	{
		Package = new Dictionary<Resource, float>();
		List<float> q;

		if (quantities == null)
		{
			q = Enumerable.Repeat(0f, resources.Count).ToList();
		}
		else
		{
			q = quantities;
		}

        InitializeResources(resources);

        Add(FromLists(resources, q));
    }

	public Resources(List<Resource> resources, float quantity) {
		Package = new Dictionary<Resource, float>();

		InitializeResources(resources);

        Add(FromLists(resources, Enumerable.Repeat(quantity, resources.Count).ToList()));
    }

	public Resources(Resource resource, float quantity = 0) {
		InitializeResources(new List<Resource>() { resource });
		Add(resource, quantity);
	}

    public Resources(float quantity)
	{
		Package = new Dictionary<Resource, float>();
		List<Resource> resources = AllResources();
		InitializeResources(resources);

		Add(FromLists(resources, Enumerable.Repeat(quantity, resources.Count).ToList()));
	}

	public Resources()
	{
		Package = new Dictionary<Resource, float>();
		InitializeResources(AllResources());

	}

	public void InitializeResources(List<Resource> resources) 
	{
		foreach(Resource r in resources) 
		{
			if(!Package.ContainsKey(r)) 
			{
				Package.Add(r, 0);
			}
		}
	}

	public static List<Resource> AllResources()
	{
		return Enum.
			GetValues(typeof(Resource)).
			Cast<Resource>().
			Where((Resource r) => { return r != Resource.None; }).
			ToList();
	}


	Dictionary<Resource, float> FromLists(List<Resource> resources, List<float> quantities)
	{
		var result = new Dictionary<Resource, float>();
		for (int i = 0; i < resources.Count; i++)
		{
			float amt = 0;
			if (i < quantities.Count)
			{
				amt = quantities[i];
			}

			result.Add(resources[i], amt);
		}

		return result;
	}


	public float Get(Resource resource)
	{
		return Contains(resource) ? Package[resource] : 0;
	}

	public bool Contains(Resource resource)
	{
		return Package.Keys.Contains(resource);
	}

	public void Add(Resource resource, float amount)
	{
		if (!Contains(resource))
		{
			throw new ArgumentException("cannot add a new resource", nameof(resource));
		}
		else
		{
			Package[resource] += amount;
			Weight += amount * ResourceWeights.Weight(resource);
		}

	}

	public void Add(Dictionary<Resource, float> resources)
	{
		foreach (var r in resources)
		{
			Add(r.Key, r.Value);
		}
	}

	public float Remove(Resource resource, float amount)
	{
		if (!Contains(resource))
		{
			throw new ArgumentException("resource not present", nameof(resource));
		}
		else
		{
			float actualAmount = Mathf.Min(amount, Package[resource]);
			Package[resource] -= actualAmount;
			Weight -= actualAmount * ResourceWeights.Weight(resource);
			return actualAmount;
		}
	}
	
	public List<Resource> ToList() 
	{
		return Package.Keys.ToList();
	}
	


}
