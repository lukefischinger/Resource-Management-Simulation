using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceBankManager : MonoBehaviour, IComparer<IAssignable>
{
    List<ResourceBank> nonAssignments = new List<ResourceBank>();
    List<IAssignable> assignments = new List<IAssignable>();

    void Start()
    {

        foreach (var bank in FindObjectsOfType<ResourceBank>())
        {
            if (bank.TryGetComponent<Worker>(out _))
            {
                continue;
            }
            else if (bank.TryGetComponent(out IAssignable assignment))
            {
                assignments.Add(assignment);
                if (assignment.gameObject.TryGetComponent(out ResourceSource source))
                {
                    nonAssignments.Add(source.GetComponent<ResourceBank>());
                }
            }
            else
            {
                nonAssignments.Add(bank);
            }

        }

        assignments.Sort(Compare);

    }


    public void Add(ResourceBank bank)
    {
        if (bank.TryGetComponent(out IAssignable assignment))
        {
            if (!assignments.Contains(assignment))
            {
                assignments.Add(assignment);
            }
        }
        else
        {
            if (!nonAssignments.Contains(bank))
            {
                nonAssignments.Add(bank);
            }
        }
    }


    public async Task<IAssignable> GetAssignment(Worker worker)
    {
        for (int i = 0; i < assignments.Count; i++)
        {
            if (assignments[i].gameObject.GetComponent<Associatable>().AtCapacity)
            {
                continue;
            }
            else if (assignments[i].gameObject.TryGetComponent(out Depositable d) && !(await IsValidAssignableDeposit(worker, d)))
            {
                continue;
            }
            else if (assignments[i].gameObject.TryGetComponent(out Withdrawable w) && (await w.GetCurrentResources()).Weight == 0)
            {
                continue;
            }
            else
            {
                return assignments[i];
            }
        }
        return null;
    }


    public async Task<ResourceBank> GetClosest<ITransactionable>(Worker worker, Resources resources)
    {
        ResourceBank closest = null;
        float closestDistance = Mathf.Infinity;

        ITransactionable curr;
        float currWeight, currDistance;
        Vector3 workerPosition = worker.transform.position;

        for (int i = 0; i < nonAssignments.Count; i++)
        {
            curr = nonAssignments[i].GetComponent<ITransactionable>();
            if (curr == null || nonAssignments[i].GetComponent<Associatable>().AtCapacity)
            {
                continue;
            }

            if (curr is IDepositable)
            {
                currWeight = (await (curr as IDepositable).GetAvailableDeposits(resources)).Weight;
            }
            else if (curr is IWithdrawable)
            {
                currWeight = (await (curr as IWithdrawable).GetAvailableWithdrawals(resources)).Weight;
            }
            else continue;

            currDistance = (nonAssignments[i].transform.position - workerPosition).magnitude;
            if (currWeight > 0 && currDistance <= closestDistance)
            {
                closest = nonAssignments[i];
                closestDistance = currDistance;
            }
        }

        return closest;
    }


    public async Task<bool> IsValidAssignableDeposit(Worker worker, Depositable assignable)
    {
        //if ((await assignable.GetAvailableDeposits(worker.GetComponent<Withdrawable>().GetCurrentResources())).Weight > 0) return true;

        for (int i = 0; i < nonAssignments.Count; i++)
        {
            if (
                nonAssignments[i].TryGetComponent(out Withdrawable withdrawable) &&
                !nonAssignments[i].GetComponent<Associatable>().AtCapacity &&
                (await assignable.GetAvailableDeposits(await withdrawable.GetCurrentResources())).Weight > 0
            )
            {
                return true;
            }
        }

        return false;
    }

    // results in decreasing priority when used in Sort method
    public int Compare(IAssignable x, IAssignable y)
    {
        return y.Priority - x.Priority;
    }
}
