using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildableManager : MonoBehaviour
{

    [SerializeField] List<BuildableData> prefabs;


    Dictionary<string, ProcessorResources> processorResources = new Dictionary<string, ProcessorResources>()
    {
        {"Blast Furnce",  new ProcessorResources(Resource.IronOre, Resource.PigIron)},
        {"Steel Refinery", new ProcessorResources(Resource.PigIron, Resource.Steel) }
    };


    public struct ProcessorResources
    {
        public Resource input;
        public Resource output;

        public ProcessorResources(Resource input, Resource output)
        {
            this.input = input;
            this.output = output;
        }
    }




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
    public Sprite sprite;

    public BuildableData(GameObject prefab, string name = "", Sprite sprite = null)
    {
        this.prefab = prefab;
        this.name = (name == "") ? prefab.name : name;
        this.sprite = sprite;
    }


}
