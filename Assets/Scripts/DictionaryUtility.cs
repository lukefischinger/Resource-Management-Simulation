using System.Collections.Generic;
using UnityEngine;

public static class DictionaryUtility 
{
	public static Dictionary<TOne, TTwo> RemoveKeys<TOne, TTwo>(Dictionary<TOne, TTwo> dict, List<TOne> keys) 
	{
		foreach(TOne key in keys) 
		{
			dict.Remove(key);
		}
		return dict;
	} 
	
	public static Dictionary<Resource, float> Infinity(List<Resource> list) 
	{
		Dictionary<Resource, float> dict = new Dictionary<Resource, float>();
		
		foreach(Resource r in list) 
		{
			dict.Add(r, Mathf.Infinity);
		}
		
		return dict;
	}
	
	
	
	
}
