using System.Collections.Generic;
using UnityEngine;

public class Associatable : MonoBehaviour
{
    public int Count { get; private set; }
    public int MaxCount { get; private set; } = 20;


    public List<ITransporter> Associates { get; private set; } = new List<ITransporter>();

    public List<string> AssociatesNames = new List<string>();

    public int count;
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
            foreach (ITransporter t in toRemove)
            {
                if (!Associates.Contains(t)) continue;

                Associates.Remove(t);
            }
            toRemove.Clear();
        }

        if (toAdd.Count > 0)
        {
            foreach (ITransporter t in toAdd)
            {
                if (Associates.Contains(t)) continue;

                Associates.Add(t);
            }
            toAdd.Clear();
        }

        AssociatesNames.Clear();
        foreach (ITransporter assoc in Associates)
        {
            AssociatesNames.Add(assoc.gameObject.name);
        }
        count = Count;

    }

    // called only during physics step, when resources are exchanged between Banks
    public void EndAllAssociations()
    {
        string output = gameObject.name + ": ";
        foreach (var associate in Associates)
        {
            output += associate.gameObject.name + ", ";
            associate.SetNewAssociatable(this);
        }
    }

}
