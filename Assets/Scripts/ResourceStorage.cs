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
        transform.position = Vector2Int.FloorToInt(transform.position) + Vector2.one * 0.5f;

        deposit.Initialize(startingResources, startingQuantities);
    }

    public void Deselect()
    {
        myRenderer.color = Color.white;
        GetComponent<Associatable>().DeselectAssociates();

    }

    public Resources GetDisplayData()
    {
        return withdraw.GetCurrentResources();
    }

    public void Select()
    {
        myRenderer.color = Color.green;
        GetComponent<Associatable>().SelectAssociates();

    }

}
