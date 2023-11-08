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

	
	void Start()
	{
		myRenderer = GetComponent<SpriteRenderer>();
		deposit = GetComponent<Depositable>();
		withdraw = GetComponent<Withdrawable>();
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

    public void AddAssociation()
    {
    }

    public void RemoveAssociation()
    {
        throw new System.NotImplementedException();
    }
}
