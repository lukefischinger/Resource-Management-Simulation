using System.Collections.Generic;
using UnityEngine;

public class ResourceBankManager : MonoBehaviour, IComparer<IAssignable>
{
    List<ResourceBank> nonAssignments = new List<ResourceBank>();
    List<IAssignable> assignments = new List<IAssignable>();
    List<Worker> workers = new List<Worker>();

    public BuildingGraph buildingGraph { get; private set; }

    void Start()
    {
        buildingGraph = new BuildingGraph(100);


        foreach (var bank in FindObjectsOfType<ResourceBank>())
        {
            Add(bank);
            if (bank.TryGetComponent(out Worker worker))
            {
                workers.Add(worker);
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
        if (bank.TryGetComponent(out Worker worker) && !workers.Contains(worker))
        {
            workers.Add(worker);
        }
        else if (bank.TryGetComponent(out IAssignable assignment) && !assignments.Contains(assignment))
        {
            assignments.Add(assignment);

            if (assignment.gameObject.TryGetComponent(out ResourceSource source))
            {
                nonAssignments.Add(source.GetComponent<ResourceBank>());
            }
            buildingGraph.RemoveVertex(buildingGraph.Dimension * (int)bank.transform.position.x + (int)bank.transform.position.y);

        }
        else if (!nonAssignments.Contains(bank))
        {
            nonAssignments.Add(bank);
            buildingGraph.RemoveVertex(buildingGraph.Dimension * (int)bank.transform.position.x + (int)bank.transform.position.y);

        }

    }


    public IAssignable GetAssignment(Worker worker)
    {
        for (int i = 0; i < assignments.Count; i++)
        {
            if (assignments[i].gameObject.GetComponent<Associatable>().AtCapacity)
            {
                continue;
            }
            else if (assignments[i].gameObject.TryGetComponent(out Depositable d) && !(IsValidAssignableDeposit(d)))
            {
                continue;
            }
            else if (assignments[i].gameObject.TryGetComponent(out Withdrawable w) && (w.GetCurrentResources()).Weight == 0)
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


    public ResourceBank GetClosest<ITransactionable>(Transform transform, Resources resources)
    {
        ResourceBank closest = null;
        float closestDistance = Mathf.Infinity;

        ITransactionable curr;
        float currWeight, currDistance;
        Vector3 basePosition = transform.position;

        for (int i = 0; i < nonAssignments.Count; i++)
        {
            curr = nonAssignments[i].GetComponent<ITransactionable>();
            if (curr == null || nonAssignments[i].GetComponent<Associatable>().AtCapacity)
            {
                continue;
            }

            if (curr is IDepositable)
            {
                currWeight = ((curr as IDepositable).GetAvailableDeposits(resources)).Weight;
            }
            else if (curr is IWithdrawable)
            {
                currWeight = ((curr as IWithdrawable).GetAvailableWithdrawals(resources)).Weight;
            }
            else continue;

            currDistance = (nonAssignments[i].transform.position - basePosition).magnitude;
            if (currWeight > 0 && currDistance <= closestDistance)
            {
                closest = nonAssignments[i];
                closestDistance = currDistance;
            }
        }

        return closest;
    }


    public bool IsValidAssignableDeposit(Depositable assignable)
    {

        for (int i = 0; i < nonAssignments.Count; i++)
        {
            if (
                nonAssignments[i].TryGetComponent(out Withdrawable withdrawable) &&
                !nonAssignments[i].GetComponent<Associatable>().AtCapacity &&
                (assignable.GetAvailableDeposits(withdrawable.GetCurrentResources())).Weight > 0
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
