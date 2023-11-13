using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Depositable), typeof(Withdrawable))]
public class ResourceStorage : MonoBehaviour, ISelectable
{

    [SerializeField] List<Resource> startingResources;
    [SerializeField] List<float> startingQuantities;
    Depositable deposit;
    Withdrawable withdraw;
    SpriteRenderer myRenderer;
    ResourceBank resourceBank;
    Associatable associatable;


    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        deposit = GetComponent<Depositable>();
        withdraw = GetComponent<Withdrawable>();
        resourceBank = GetComponent<ResourceBank>();
        associatable = GetComponent<Associatable>();

        deposit.Initialize(startingResources, startingQuantities);
    }

    public void Deselect()
    {
        myRenderer.color = Color.white;
    }

    public Resources GetDisplayData()
    {
        return withdraw.GetCurrentResources();
    }

    public void Select()
    {
        myRenderer.color = Color.green;
    }



    void OnEnable()
    {
        resourceBank.Full += associatable.EndAllAssociations;
    }

    void OnDisable()
    {
        resourceBank.Full -= associatable.EndAllAssociations;
    }
}
