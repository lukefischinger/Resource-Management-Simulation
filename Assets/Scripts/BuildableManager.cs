using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildableManager : MonoBehaviour
{

	[SerializeField] List<BuildableData> prefabs;

	public List<BuildableData> Prefabs
	{
		get
		{
			return prefabs;
		}
		private set
		{
			prefabs = value;
		}
	}

	public BuildableData GetBuildable(string name)
	{
		foreach (var data in Prefabs)
		{
			if (data.name == name)
			{				
				return data;
			}
		}

		return null;
	}
}

[Serializable]
public class BuildableData
{
	public string name;
	public GameObject prefab;

	public BuildableData(GameObject prefab, string name = "")
	{
		this.prefab = prefab;
		this.name = (name == "") ? prefab.name : name;
	}


}
