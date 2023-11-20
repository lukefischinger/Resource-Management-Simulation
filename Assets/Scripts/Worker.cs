using System.Linq;
using UnityEngine;

public class Worker : MonoBehaviour, ISelectable, ITransporter
{

    // Debugging fields

    public string depositName, withdrawName, assignmentName;

    // end debugging


    const float moveSpeed = 6f;
    public enum WorkerState { Idling, ToWithdraw, ToDeposit, Withdrawing };
    public WorkerState state = WorkerState.Idling;
    public WorkerState State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
        }
    }

    private Transform withdrawTransform, depositTransform, myTransform;
    private Rigidbody2D myRigidbody;
    private SpriteRenderer myRenderer;
    private Animator myAnimator;
    private bool hasNewAssignment;
    private Depositable myDeposit;
    private Withdrawable myWithdraw;
    private ResourceBank myBank;
    private ResourceBankManager bankManager;
    private bool IsToWithdraw => State == WorkerState.ToWithdraw;
    private bool IsToDeposit => State == WorkerState.ToDeposit;


    public Vector2[] path;
    private int pathIndex;
    private int pathDirection = 1;

    private IAssignable assignment;
    public IAssignable Assignment
    {
        get
        {
            return assignment;
        }
        private set
        {

            assignment = value;

            assignmentName = assignment == null ? "null" : assignment.gameObject.name;
        }
    }

    private Withdrawable withdraw;
    public Withdrawable Withdraw
    {
        get
        {
            return withdraw;
        }
        set
        {
            TryRemoveAssociation(withdraw);
            withdraw = value;
            TryAddAssociation(withdraw);
            withdrawTransform = (value == null) ? null : value.transform;
            withdrawName = (withdraw == null) ? "null" : withdraw.gameObject.name;


        }
    }

    private Depositable deposit;
    public Depositable Deposit
    {
        get
        {
            return deposit;
        }
        set
        {
            TryRemoveAssociation(deposit);
            deposit = value;
            TryAddAssociation(deposit);
            depositTransform = (value == null) ? null : value.transform;

            depositName = (deposit == null) ? "null" : deposit.gameObject.name;


        }
    }


    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myTransform = transform;
        myRenderer = GetComponent<SpriteRenderer>();
        myAnimator = GetComponent<Animator>();
        myDeposit = GetComponent<Depositable>();
        myWithdraw = GetComponent<Withdrawable>();
        myBank = GetComponent<ResourceBank>();

        bankManager = GameObject.Find("Resource Bank Manager").GetComponent<ResourceBankManager>();
        GetComponent<ResourceBank>().myResources = new Resources(0);
        SetIdle();
    }

    void Update()
    {
        if (gameObject.name == "Worker 1 (8)")
        {
            //Debug.Log("here");
        }
        switch (State)
        {
            case WorkerState.ToWithdraw:
                PrepareMove(withdrawTransform);
                break;
            case WorkerState.ToDeposit:
                PrepareMove(depositTransform);
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        if (gameObject.name == "Worker 1 (8)")
        {
            //Debug.Log("here");
        }
        switch (State)
        {
            case WorkerState.ToWithdraw:
                Move(withdrawTransform);
                break;
            case WorkerState.ToDeposit:
                Move(depositTransform);
                break;
            case WorkerState.Idling:
                Idle();
                break;
            default:
                break;
        }
    }

    void Rotate()
    {
        transform.up = myRigidbody.velocity;
    }

    void Idle()
    {
        myRigidbody.velocity = Vector2.zero;
        myAnimator.speed = 0f;
    }

    void PrepareMove(Transform destination)
    {
        if (destination == null)
        {
            SetIdle();
        }
        else if (GetPathEndPoint() != (Vector2)destination.transform.position)
        {
            bankManager.SubmitPathCalculationRequest(this, myTransform.position, destination.position);
        }
    }

    void Move(Transform destination)
    {
        if (destination == null || path == null || path.Length == 0)
        {
            SetIdle();
            return;
        }

        Rotate();
        myAnimator.speed = 1f;

        SetPathIndex();
        myRigidbody.velocity = moveSpeed * ((Vector3)path[pathIndex] - myTransform.position).normalized;
    }

    public void Assign(IAssignable assignable)
    {
        SetNewAssignment(assignable);
    }



    void SetPathIndex()
    {
        if (path == null || path.Length == 0)
        {
            bankManager.SubmitPathCalculationRequest(
                this,
                myTransform.position,
                IsCarrying() ? depositTransform.position : withdrawTransform.position
            );
        }
        else if (((Vector2)myTransform.position - path[pathIndex]).magnitude < 0.5f)
        {
            pathIndex += pathDirection;
            pathIndex = Mathf.Clamp(pathIndex, 0, path.Length - 1);

        }

    }

    public void SetPath(Vector2[] path)
    {
        this.path = path;
        pathIndex = 0;
        pathDirection = 1;
    }

    Vector2 GetPathEndPoint(int direction = 1)
    {
        if (path == null || path.Length == 0) return -Vector2.one;

        if (direction == 1)
        {
            return path[^1];
        }
        else
        {
            return path[0];
        }
    }


    void TryAddAssociation(ITransactionable transactionable)
    {
        if (transactionable == null) return;

        transactionable.gameObject.GetComponent<Associatable>().AddAssociation(this);
    }

    void TryRemoveAssociation(ITransactionable transactionable)
    {
        if (transactionable == null) return;

        transactionable.gameObject.GetComponent<Associatable>().RemoveAssociation(this);
    }

    bool IsAssignmentWithdrawable()
    {
        return Assignment != null && Assignment.gameObject.TryGetComponent<Withdrawable>(out _);
    }

    bool IsAssignmentDepositable()
    {
        return Assignment != null && Assignment.gameObject.TryGetComponent<Depositable>(out _);
    }




    public void SetNewAssignment(IAssignable assignment)
    {
        Assignment = assignment;

        if (Assignment == null)
        {
            SetIdle();
        }
        else
        {
            SetDepositAndWithdraw(Assignment);
            State = WorkerState.ToWithdraw;
        }

        if (IsCarrying())
        {
            hasNewAssignment = true;
            SetClosestDeposit(transform);
            State = WorkerState.ToDeposit;
        }
    }

    void SetDepositAndWithdraw(IAssignable assignable)
    {
        if (assignable == null) return;

        if (assignable.gameObject.TryGetComponent(out Depositable temp1))
        {
            Deposit = temp1;

            SetClosestWithdraw(assignable.transform);

        }
        else if (assignable.gameObject.TryGetComponent(out Withdrawable temp2))
        {

            Withdraw = temp2;

            SetClosestDeposit(assignable.transform);
        }


    }

    void SetClosestWithdraw(Transform t)
    {
        if (IsAssignmentWithdrawable())
        {
            SetIdle();
        }
        else
        {
            Withdraw = null;
            ResourceBank bank = bankManager.GetClosest<Withdrawable>(t, GetResourcesSought(false));

            if (bank != null)
            {
                Withdraw = bank.GetComponent<Withdrawable>();
            }
            else
            {
                SetIdle();
            }
        }
    }

    void SetClosestDeposit(Transform t)
    {
        if (IsAssignmentDepositable() && !IsCarrying())
        {
            SetIdle();
        }
        else
        {
            Deposit = null;
            ResourceBank bank = bankManager.GetClosest<Depositable>(t, GetResourcesSought(true));

            if (bank != null)
            {
                Deposit = bank.GetComponent<Depositable>();
            }
            else
            {
                SetIdle();
            }
        }

    }

    void SettleDeposit()
    {
        SettleTransaction(myWithdraw, Deposit, false);

        if (IsCarrying())
        {
            SetClosestDeposit(myTransform);
            State = WorkerState.ToDeposit;
        }
        else
        {
            if (hasNewAssignment)
            {
                SetDepositAndWithdraw(Assignment);
                hasNewAssignment = false;
            }
            else if (AreWithdrawAndDepositIncompatible())
            {
                SetIdle();
            }

            State = WorkerState.ToWithdraw;
        }
    }

    void SettleWithdrawal()
    {
        SettleTransaction(Withdraw, myDeposit, true);
        if (!IsCarrying())
        {
            SetIdle();
        }
        else if (!CanDepositAcceptMyResources())
        {
            SetIdle();
        }
        else
        {
            state = WorkerState.ToDeposit;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        PerformDepositOrWithdrawal(collision.gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        PerformDepositOrWithdrawal(collision.gameObject);
    }

    void PerformDepositOrWithdrawal(GameObject obj)
    {
        if (IsMyTarget(obj))
        {
            if (IsToWithdraw)
            {
                SettleWithdrawal();
                pathDirection *= -1;

            }
            else if (IsToDeposit)
            {
                SettleDeposit();
                pathDirection *= -1;
            }
        }
        else if (Deposit == null || Withdraw == null)
        {
            return;
        }
        else if (obj != Deposit.gameObject && obj != Withdraw.gameObject)
        {
            if (path == null || (pathIndex - pathDirection >= 0 && pathIndex - pathDirection < path.Length && !bankManager.pathwayGraph.IsInPathways(path[pathIndex - pathDirection], path[pathIndex])))
            {
                bankManager.SubmitPathCalculationRequest(
                       this,
                       myTransform.position,
                       IsCarrying() ? depositTransform.position : withdrawTransform.position
                   );
            }
        }

    }

    bool IsMyTarget(GameObject gameObject)
    {
        return (State == WorkerState.ToWithdraw &&
                Withdraw != null &&
                (gameObject == Withdraw.gameObject ||
                (gameObject.transform.childCount > 0 &&
                    gameObject.transform.GetChild(0).gameObject == Withdraw.gameObject))) ||
               (State == WorkerState.ToDeposit && Deposit != null && gameObject == Deposit.gameObject);
    }

    bool IsCarrying()
    {
        return myBank.myResources.Weight > 0;
    }

    void SettleTransaction(IWithdrawable from, IDepositable to, bool withdrawal)
    {
        Resources requested = Deposit.GetAvailableDeposits(from.GetCurrentResources());
        requested = to.GetAvailableDeposits(requested);

        if (withdrawal)
        {
            to.Deposit(requested);
            from.Withdraw(requested);
        }
        else
        {
            from.Withdraw(requested);
            to.Deposit(requested);
        }

    }

    public void Select()
    {
        myRenderer.color = Color.red;
        myRenderer.sortingOrder = 1;
        if (path == null) return;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Debug.DrawLine(path.ElementAt(i), path.ElementAt(i + 1), Color.red, 1f);
        }
    }

    public void Deselect()
    {
        myRenderer.color = Color.white;
        myRenderer.sortingOrder = 0;
    }

    public Resources GetDisplayData()
    {
        return myWithdraw.GetCurrentResources();
    }

    bool AreWithdrawAndDepositIncompatible()
    {
        if (Deposit == null || Withdraw == null) return true;
        return (Deposit.GetAvailableDeposits(Withdraw.GetCurrentResources())).Weight == 0;
    }

    bool CanDepositAcceptMyResources()
    {
        if (Deposit == null) return false;
        else return Deposit.GetAvailableDeposits(myBank.myResources).Weight > 0;
    }




    Resources GetResourcesSought(bool isForDeposit, bool isForAssignment = false)
    {
        if (isForAssignment)
            return Resources.Infinity;
        else if (IsCarrying())
        {
            return myBank.myResources;
        }
        else if (isForDeposit)
        {
            return Withdraw == null ? new Resources() : Withdraw.GetCurrentResources();
        }
        else
        {
            return Deposit == null ? new Resources() : Deposit.GetAvailableDeposits(Resources.Infinity);
        }
    }

    void SetIdle()
    {
        State = WorkerState.Idling;
        Assignment = null;
        Deposit = null;
        Withdraw = null;
        path = null;
        bankManager.AddIdleWorker(this);

    }

    public void SetNewAssociatable(Associatable calledBy)
    {
        throw new System.NotImplementedException();
    }
}
