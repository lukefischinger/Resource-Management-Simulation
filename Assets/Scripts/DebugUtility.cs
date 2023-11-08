
using System.Collections.Generic;
using UnityEngine;

public static class DebugUtility
{
	public static void DebugDictionary(Dictionary<Resource, float> dict) 
	{
		string result = "";
		foreach(var entry in dict) 
		{
			result += "(" + entry.Key + ", " + entry.Value + "), ";
		}
		
		Debug.Log(result);
	}
}
