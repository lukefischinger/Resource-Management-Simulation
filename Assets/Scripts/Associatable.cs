using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Associatable : MonoBehaviour
{
    public int Count { get; private set; }
    public int MaxCount { get; private set; } = 24;

    bool isSelected = false;
    public List<ITransporter> Associates { get; private set; } = new List<ITransporter>();

    public List<string> associateNames = new List<string>();

    public bool AtCapacity => Count >= MaxCount;

    public void AddAssociation(ITransporter associate)
    {
        if (!Associates.Contains(associate))
        {
            Associates.Add(associate);
            if (isSelected)
            {
                SelectAssociate(associate);
            }
            Count++;
        }
    }

    public void RemoveAssociation(ITransporter associate)
    {
        if (Associates.Contains(associate))
        {
            Associates.Remove(associate);
            if (isSelected)
            {
                DeselectAssociate(associate);
            }
            Count--;
        }

    }

    private void Update()
    {
        associateNames.Clear();
        foreach (var assoc in Associates)
        {
            associateNames.Add(assoc.gameObject.name);
        }
    }

    public void SelectAssociates()
    {
        isSelected = true;
        foreach (var associate in Associates)
        {
            if (associate.gameObject.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
            }
        }
    }

    public void DeselectAssociates()
    {
        isSelected = false;
        foreach (var associate in Associates)
        {
            DeselectAssociate(associate);
        }
    }

    void SelectAssociate(ITransporter associate)
    {
        if (associate.gameObject.TryGetComponent(out ISelectable selectable))
        {
            selectable.Select();
        }
    }

    void DeselectAssociate(ITransporter associate)
    {
        if (associate.gameObject.TryGetComponent(out ISelectable selectable))
        {
            selectable.Deselect();
        }
    }

}
