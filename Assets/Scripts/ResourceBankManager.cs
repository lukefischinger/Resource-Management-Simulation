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
            if(bank.gameObject.name == "Processor (2)") {
                Debug.Log("here");
            }
            curr = bank.GetComponent<ITransactionable>();
            if (curr == null || bank.GetComponent<Associatable>().AtCapacity)
            {
                continue;
            }
            else if (assignableOnly) // if no assignable, or if a depositable and no valid withdrawables exist, skip
            {
                if (!bank.TryGetComponent<IAssignable>(out _) || (bank.TryGetComponent(out Depositable d) && !IsValidAssignableDeposit(transform, d)))
                {
                    continue;
                }
            }

            if (curr is IDepositable)
            {
                currWeight = (curr as IDepositable).GetAvailableDeposits(resources).Weight;
            }
            else if (curr is IWithdrawable)
            {
                currWeight = (curr as IWithdrawable).GetAvailableWithdrawals(resources).Weight;

            }
            else continue;

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


    public bool IsValidAssignableDeposit(Transform transform, Depositable assignable)
    {
        if (assignable.GetAvailableDeposits(transform.GetComponent<Withdrawable>().GetCurrentResources()).Weight > 0) return true;

        foreach (ResourceBank bank in banks)
        {
            if (bank.TryGetComponent(out Withdrawable withdrawable) && !bank.GetComponent<Associatable>().AtCapacity && assignable.GetAvailableDeposits(withdrawable.GetCurrentResources()).Weight > 0)
            {
                return true;
            }

        }

        return false;
    }


}
