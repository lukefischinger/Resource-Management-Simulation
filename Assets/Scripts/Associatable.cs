using System.Collections.Generic;
using UnityEngine;

public class Associatable : MonoBehaviour
{
    public int Count { get; private set; }
    public int MaxCount { get; private set; } = 10;


    public List<ITransporter> Associates { get; private set; } = new List<ITransporter>();

    public bool AtCapacity => Count >= MaxCount;

    List<ITransporter> toRemove = new List<ITransporter>(),
                    toAdd = new List<ITransporter>();

    public void AddAssociation(ITransporter associate)
    {

        toAdd.Add(associate);
        Count++;
    }

    public void RemoveAssociation(ITransporter associate)
    {
        toRemove.Add(associate);
        Count--;

    }

    private void Update()
    {
        if (toRemove.Count > 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                if (!Associates.Contains(toRemove[i])) continue;

                Associates.Remove(toRemove[i]);
            }
            toRemove.Clear();
        }

        if (toAdd.Count > 0)
        {
            for (int i = 0; i < toAdd.Count; i++)
            {
                if (Associates.Contains(toAdd[i])) continue;

                Associates.Add(toAdd[i]);
            }
            toAdd.Clear();
        }
    }

    // called only during physics step, when resources are exchanged between Banks
    public void EndAllAssociations()
    {
        string output = gameObject.name + ": ";
        foreach (var associate in Associates)
        {
            output += associate.gameObject.name + ", ";
            //associate.SetNewAssociatable(this);
        }
    }

}
