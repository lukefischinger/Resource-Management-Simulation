using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System.Net;
using System.Threading.Tasks;

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

    public async Task<Resources> GetAvailableWithdrawals(Resources attempt)
    {
        return await ITransactionable.GetMaxTransaction(bank.myResources, attempt, Mathf.Infinity, Mathf.Infinity);
    }

    public async Task<Resources> GetCurrentResources()
    {
        return bank.myResources;
    }

    public Resources Withdraw(Resources resources)
    {
        var result = new Resources();

        for (int i = 0; i < resources.Count; i++)
        {
            Resource key = resources.ToList()[i];

            if (resources.Get(key) == 0) continue;

            float amount = bank.Remove(key, resources.Get(key));
            result.Add(key, amount);
        }

        return result;
    }
}
