using System.Collections.Generic;
using UnityEngine;

public class ResourceBankManager : MonoBehaviour
{
	List<ResourceBank> banks = new List<ResourceBank>();

	void Awake()
	{
		foreach (var bank in FindObjectsOfType<ResourceBank>())
		{
			if (bank.GetComponent<Worker>() == null)
			{
				AddBank(bank);
			}
		}
	}


	public void AddBank(ResourceBank bank)
	{
		if (!banks.Contains(bank))
		{
			banks.Add(bank);
		}
	}

	public void RemoveBank(ResourceBank bank)
	{
		if (banks.Contains(bank))
		{
			banks.Remove(bank);
		}
	}
	
	public ResourceBank GetClosest<ITransactionable>(Transform transform, Resources resources, bool assignableOnly = false) 
	{
		ResourceBank closest = null;
		Transform closestTransform;
		float closestDistance = Mathf.Infinity;

		ITransactionable curr;
		float currWeight, currDistance;

		foreach (ResourceBank bank in banks)
		{
			curr = bank.GetComponent<ITransactionable>();
			if (curr == null || bank.GetComponent<Associatable>().AtCapacity || (assignableOnly && !bank.TryGetComponent<IAssignable>(out _)))
			{
				continue;
			}

			if(curr is IDepositable) 
			{
				currWeight = (curr as IDepositable).GetAvailableDeposits(resources).Weight;
			} else if(curr is IWithdrawable) 
			{
				currWeight = (curr as IWithdrawable).GetAvailableWithdrawals(resources).Weight;

			} else continue;

			currDistance = (bank.transform.position - transform.position).magnitude;
			if (currWeight > 0 && currDistance <= closestDistance)
			{
				closest = bank;
				closestTransform = bank.transform;
				closestDistance = currDistance;
			}
		}

		return closest;
	}
	
	
}
