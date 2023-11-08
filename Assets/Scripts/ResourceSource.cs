using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Withdrawable))]
public class ResourceSource : MonoBehaviour, IAssignable
{

    [SerializeField] List<Resource> startingResources;
    [SerializeField] List<float> startingQuantities;
    Withdrawable resources;
    SpriteRenderer myRenderer;
    

    public void BeAssigned(ITransporter transporter)
    {
        throw new System.NotImplementedException();
    }

    public void BeUnassigned(ITransporter transporter)
    {
        throw new System.NotImplementedException();
    }

    public void Deselect()
    {
        myRenderer.color = Color.white;
    }

    public Resources GetDisplayData()
    {
        return resources.GetCurrentResources();
    }

    public void Select()
    {
        myRenderer.color = Color.blue;
    }

    void Start()
    {
        resources = GetComponent<Withdrawable>();
        resources.Initialize(startingResources, startingQuantities);
        myRenderer = GetComponent<SpriteRenderer>();
    }
}
