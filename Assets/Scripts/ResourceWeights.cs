using System.Collections.Generic;
using UnityEngine;

public static class ResourceWeights
{
	
	public static float Weight(Resource resource) => resourceWeights[resource];
	
	public static Dictionary<Resource, float> resourceWeights = new Dictionary<Resource, float>
	{
		{Resource.Water, 1f},
		{Resource.IronOre, 2f},
		{Resource.Ice, 1f},
		{Resource.PigIron, 4f},
		{Resource.Steel, 10f},
	};
	
	public static float TotalWeight(Dictionary<Resource, float> dict) 
	{
		float result = 0;
		foreach(var item in dict) 
		{
			result += item.Value * resourceWeights[item.Key];
		}
		
		return result;
	}
	
	

}
