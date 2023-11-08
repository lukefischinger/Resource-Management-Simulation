using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(ResourceBank))]
public class Withdrawable : MonoBehaviour, IWithdrawable
{
	ResourceBank bank;

	void Awake()
	{
		bank = GetComponent<ResourceBank>();
	}
	public void Initialize(List<Resource> resources, List<float> quantities)
	{
		for (int i = 0; i < resources.Count; i++)
		{
			bank.Add(resources[i], quantities[i]);
		}
	}

	public Resources GetAvailableWithdrawals(Resources attempt)
	{
		return ITransactionable.GetMaxTransaction(bank.myResources, attempt, Mathf.Infinity, Mathf.Infinity);
	}
	
	public Resources GetCurrentResources()
	{
		return bank.myResources;
	}

	public Resources Withdraw(Resources resources)
	{
		var result = new Resources();

		for (int i = 0; i < resources.Count; i++)
		{
			Resource key = resources.ToList()[i];
			float amount = bank.Remove(key, resources.Get(key));
			result.Add(key, amount);
		}

		return result;
	}
}
