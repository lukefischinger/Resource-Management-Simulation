using System.Collections.Generic;
using UnityEngine;

public class ResourceBankManager : MonoBehaviour, IComparer<IAssignable>
{
    List<ResourceBank> nonAssignments = new List<ResourceBank>();
    List<IAssignable> assignments = new List<IAssignable>();
    List<Worker> workers = new List<Worker>();

    List<IAssignable> assignmentQueue = new List<IAssignable>();
    public List<string> assignmentQueueNames = new List<string>();

    List<Worker> idleWorkerQueue = new List<Worker>();
    public List<string> idleWorkerQueueNames = new List<string>();

    public PathwayGraph pathwayGraph { get; private set; }
    int dimension = 100;
    bool removeIntersectingEdges = false;

    const float oldPathRemovalTime = 15f;
    float oldPathRemovalTimer;

    public const int maxAssignmentsPerFrame = 1;
    public int assignmentsThisFrame = 0;


    private List<PathRequest> pathsToCalculate = new List<PathRequest>();



    

    void Start()
    {
        pathwayGraph = new PathwayGraph(dimension);

        foreach (var bank in FindObjectsOfType<ResourceBank>())
        {
            Add(bank);
        }

        assignments.Sort(Compare);
    }

    public void SubmitPathCalculationRequest(Worker worker, Vector2 start, Vector2 target)
    {
        pathsToCalculate.Add(new PathRequest(
            worker, 
            GraphFunctions.VectorToInt(start, dimension), 
            GraphFunctions.VectorToInt(target, dimension)
        ));
    }


    void FixedUpdate() {
        if (removeIntersectingEdges)
        {

            StartCoroutine(pathwayGraph.RemoveIntersectingEdges(pathwayGraph.pathways));

            removeIntersectingEdges = false;
            pathwayGraph.DrawGraph();

        }

    }



    void Update()
    {
        
        if (oldPathRemovalTimer < 0)
        {
            pathwayGraph.RemoveOldLookupTimes(Time.time);
            oldPathRemovalTimer = oldPathRemovalTime;
        }
        else
        {
            oldPathRemovalTimer -= Time.deltaTime;
        }

        GiveIdleWorkersAssignments();
        idleWorkerQueueNames.Clear();
        foreach (var idleWorker in idleWorkerQueue)
        {
            idleWorkerQueueNames.Add(idleWorker.name);
        }


    }

    private void LateUpdate()
    {
       

        pathwayGraph.CalculatePaths(pathsToCalculate);
        pathsToCalculate.Clear();

    }


    public void AddIdleWorker(Worker worker)
    {
        if (!idleWorkerQueue.Contains(worker))
        {
            idleWorkerQueue.Add(worker);
        }
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

            pathwayGraph.AddBuildingNode(GraphFunctions.VectorToInt(bank.transform.position, dimension));
            removeIntersectingEdges = true;
        }
        else if (!nonAssignments.Contains(bank))
        {
            nonAssignments.Add(bank);
            pathwayGraph.AddBuildingNode(GraphFunctions.VectorToInt(bank.transform.position, dimension));
            removeIntersectingEdges = true;
        }
    }

    bool IsValidAssignment(IAssignable assignment)
    {
        GameObject assignmentObj = assignment.gameObject;
        if (assignmentObj.GetComponent<Associatable>().AtCapacity)
        {
            return false;
        }
        else if (assignmentObj.TryGetComponent(out Depositable d) && !(IsValidAssignableDeposit(d)))
        {
            return false;
        }
        else if (assignmentObj.TryGetComponent(out Withdrawable w) && GetClosest<Depositable>(w.transform, w.GetCurrentResources()) == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    private void GiveIdleWorkersAssignments()
    {

        assignmentQueue = new List<IAssignable>(assignments);
        int count = 0;

        while (assignmentQueue.Count > 0 && !IsValidAssignment(assignmentQueue[0]))
        {
            assignmentQueue.RemoveAt(0);
        }

        while (idleWorkerQueue.Count > 0 && assignmentQueue.Count > 0 && count < maxAssignmentsPerFrame)
        {

            int closest = 0; //ClosestWorker(assignmentQueue[0].transform.position);
            idleWorkerQueue[closest].SetNewAssignment(assignmentQueue[0]);
            idleWorkerQueue.RemoveAt(closest);
            count++;

            while (count < maxAssignmentsPerFrame && assignmentQueue.Count > 0 && !IsValidAssignment(assignmentQueue[0]))
            {
                assignmentQueue.RemoveAt(0);
            }
        }
        assignmentQueueNames.Clear();
        foreach (var assignment in assignmentQueue)
        {
            assignmentQueueNames.Add(assignment.gameObject.name);
        }
    }

    int ClosestWorker(Vector3 position)
    {
        int closestIndex = 0;
        float closestDistance = (position - idleWorkerQueue[0].transform.position).sqrMagnitude;
        float currDistance;
        for (int i = 1; i < idleWorkerQueue.Count; i++)
        {
            currDistance = (idleWorkerQueue[i].transform.position - position).sqrMagnitude;
            if (currDistance < closestDistance)
            {
                closestIndex = i;
                closestDistance = currDistance;
            }
        }

        return closestIndex;

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
                !nonAssignments[i].GetComponent<Associatable>().AtCapacity &&
                nonAssignments[i].TryGetComponent(out Withdrawable withdrawable) &&
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

    private void OnDisable()
    {
        pathwayGraph.pathways.Dispose();
        pathwayGraph.buildings.Dispose();
    }
}
