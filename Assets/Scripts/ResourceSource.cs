using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Withdrawable), typeof(Associatable))]
public class ResourceSource : MonoBehaviour, IAssignable
{

    [SerializeField] List<Resource> startingResources;
    [SerializeField] List<float> startingQuantities;
    Withdrawable withdraw;
    SpriteRenderer myRenderer;
    Associatable associatable;
    ResourceBank resourceBank;

    public int Priority { get; private set; } = 0;

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

    public async Task<Resources> GetDisplayData()
    {
        return await withdraw.GetCurrentResources();
    }

    public void Select()
    {
        myRenderer.color = Color.blue;
    }

    void Awake()
    {
        withdraw = GetComponent<Withdrawable>();
        myRenderer = GetComponent<SpriteRenderer>();
        associatable = GetComponent<Associatable>();
        resourceBank = GetComponent<ResourceBank>();
        resourceBank.Initialize(startingResources, startingQuantities);

        foreach (Resource r in resourceBank.myResources.ToList())
        {
            if ((int)r > Priority)
            {
                Priority = (int)r;
            }
        }
    }

    void OnEnable()
    {
        resourceBank.Empty += associatable.EndAllAssociations;
    }

    void OnDisable()
    {
        resourceBank.Empty -= associatable.EndAllAssociations;
    }

   
}
