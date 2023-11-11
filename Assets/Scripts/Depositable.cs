using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(ResourceBank))]
public class Depositable : MonoBehaviour, IDepositable
{

    ResourceBank bank;
    public List<Resource> Available => bank.myResources.ToList();

    void Awake()
    {
        bank = GetComponent<ResourceBank>();
    }
    public void Initialize(List<Resource> resources, List<float> quantities)
    {
        if (bank == null)
        {
            bank = GetComponent<ResourceBank>();
        }
        bank.Initialize(resources, quantities);
    }

    public async Task<Resources> GetAvailableDeposits(Resources attempt)
    {
        return await ITransactionable.GetMaxTransaction(
            attempt,
            new Resources(bank.myResources.ToList(), Mathf.Infinity),
            Mathf.Infinity,
            bank.weightCapacity - bank.Weight
        );
    }

    public Resources Deposit(Resources resources)
    {
        var overflow = new Resources();

        foreach (var entry in resources.Package)
        {
            if (bank.Contains(entry.Key))
            {
                float unused = TryAddResource(entry.Key, entry.Value);
                if (unused > 0)
                {
                    overflow.Add(entry.Key, unused);
                }
            }
        }

        return overflow;
    }



    // returns any overflow above capacity
    float TryAddResource(Resource resource, float quantity)
    {
        if (!bank.Contains(resource)) return quantity;

        float maxUnits = (bank.weightCapacity - bank.Weight) / ResourceWeights.Weight(resource);
        float actualUnits = Mathf.Min(maxUnits, quantity);

        bank.Add(resource, actualUnits);
        return quantity - actualUnits;
    }


}
