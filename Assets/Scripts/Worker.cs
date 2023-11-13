using QuikGraph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Worker : MonoBehaviour, ISelectable, ITransporter
{

    // Debugging fields

    public string depositName, withdrawName, assignmentName;

    // end debugging


    const float moveSpeed = 6f;
    public enum WorkerState { Idling, ToWithdraw, ToDeposit };
    private WorkerState state = WorkerState.Idling;
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
    private bool hasNewAssignment;
    private const float idleTryNewAssignmentTime = 2f;
    private float idleTimer;
    private Depositable myDeposit;
    private Withdrawable myWithdraw;
    private ResourceBank myBank;
    private ResourceBankManager bankManager;
    private bool IsWithdrawing => State == WorkerState.ToWithdraw;
    private bool IsDepositing => State == WorkerState.ToDeposit;
    private IAssignable assignment;


    private Vector2[] path;
    private int pathIndex;
    private int pathDirection = 1;

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
            TryAddAssociation(value);
            withdrawTransform = (value == null) ? null : value.transform;
            withdrawName = (withdraw == null) ? "null" : withdraw.gameObject.name;
            if(Withdraw != null) {
                SetPath(withdrawTransform);
            }

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
            TryAddAssociation(value);
            depositTransform = (value == null) ? null : value.transform;

            depositName = (deposit == null) ? "null" : deposit.gameObject.name;
            if (Deposit != null)
            {
                SetPath(depositTransform);
            }

        }
    }


    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myTransform = transform;
        myRenderer = GetComponent<SpriteRenderer>();
        myDeposit = GetComponent<Depositable>();
        myWithdraw = GetComponent<Withdrawable>();
        myBank = GetComponent<ResourceBank>();

        bankManager = GameObject.Find("Resource Bank Manager").GetComponent<ResourceBankManager>();
        GetComponent<ResourceBank>().myResources = new Resources(0);
        SetIdle();
    }

    void FixedUpdate()
    {
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

    void Idle()
    {

        myRigidbody.velocity = Vector2.zero;
        if (idleTimer < 0)
        {
            SetNewAssignment();
            idleTimer = idleTryNewAssignmentTime * Random.value * 3;
        }
        else
        {
            idleTimer -= Time.deltaTime;
        }

    }

    void Move(Transform destination)
    {
        if (destination == null)
        {
            SetIdle();
            return;
        }
        else if (idleTimer < 0)
        {
            if (IsWithdrawing && AreWithdrawAndDepositIncompatible())
            {
                SetNewAssignment();
            }
            idleTimer = idleTryNewAssignmentTime * Random.value * 0.125f;
        }
        else
        {
            idleTimer -= Time.deltaTime;
        }
        SetPathIndex();
        Vector2 dest = (pathIndex + pathDirection < path.Length - 1 && pathIndex + pathDirection >= 0) ? (path[pathIndex] + path[pathIndex + pathDirection]) / 2 : path[pathIndex];
        myRigidbody.velocity = moveSpeed * ((Vector3)dest - myTransform.position).normalized;
    }

    public void Assign(IAssignable assignable)
    {
        if (assignable == Assignment) return;

        Assignment = assignable;
        SetDepositAndWithdraw(Assignment);

        if (IsCarrying())
        {
            State = WorkerState.ToDeposit;
        }
        else
        {
            State = WorkerState.ToWithdraw;
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

    void SetPathIndex()
    {
        if (pathIndex < path.Length - 1 && pathIndex > 0 && !bankManager.buildingGraph.Contains(path[pathIndex]))
        {
            SetPath(IsCarrying() ? depositTransform : withdrawTransform);
            return;
        }
        if (((Vector2)myTransform.position - path[pathIndex]).magnitude < 0.5f)
        {
            pathIndex += pathDirection;
            pathIndex = Mathf.Clamp(pathIndex, 0, path.Length - 1);
            
        }

    }

    void SetPath(Transform destination)
    {

        pathDirection = 1;
        path = bankManager.buildingGraph.ConvertToArray(bankManager.buildingGraph.CalculatePath(myTransform.position, destination.position));
        pathIndex = 0;

        for(int i = 0; i < path.Length - 1; i++) {
            Debug.DrawLine(path.ElementAt(i), path.ElementAt(i + 1), Color.red, 1f);
        }
        
    }


    void SetClosestWithdraw(Transform t)
    {
        ResourceBank bank;

        Withdraw = null;

        bank = bankManager.GetClosest<Withdrawable>(t, GetResourcesSought(false));

        if (bank != null)
        {
            Withdraw = bank.GetComponent<Withdrawable>();


        }
        else
        {
            SetNewAssignment();
        }

    }

    void SetClosestDeposit(Transform t)
    {
        ResourceBank bank;

        Deposit = null;

        bank = bankManager.GetClosest<Depositable>(t, GetResourcesSought(true));

        if (bank != null)
        {
            Deposit = bank.GetComponent<Depositable>();
        }
        else
        {
            SetIdle();
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


    void OnCollisionEnter2D(Collision2D collision)
    {
        PerformDepositOrWithdrawal(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        PerformDepositOrWithdrawal(collision);
    }

    void PerformDepositOrWithdrawal(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        if (IsMyTarget(collidedObject))
        {
            if (IsWithdrawing)
            {
                SettleTransaction(collision.gameObject.GetComponentInChildren<IWithdrawable>(), myDeposit, true);
                State = WorkerState.ToDeposit;
                pathDirection = -pathDirection;
                pathIndex += pathDirection;

                if (IsAssignmentWithdrawable())
                {
                    Withdraw = Assignment.gameObject.GetComponent<Withdrawable>();

                    if (AreWithdrawAndDepositIncompatible())
                    {

                        idleTimer = idleTryNewAssignmentTime * Random.value * 0.5f;
                        SetIdle();

                    }
                }
            }
            else if(IsDepositing)
            {
                SettleTransaction(myWithdraw, collision.gameObject.GetComponent<IDepositable>(), false);
                pathDirection = -pathDirection;
                pathIndex += pathDirection;

                if (IsCarrying())
                {
                    SetClosestDeposit(transform);
                    State = WorkerState.ToDeposit;
                }
                else if (hasNewAssignment)
                {
                    SetDepositAndWithdraw(Assignment);
                    hasNewAssignment = false;
                    State = WorkerState.ToWithdraw;


                }
                else
                {
                    if (IsAssignmentDepositable())
                    {
                        Deposit = Assignment.gameObject.GetComponent<Depositable>();
                        if (AreWithdrawAndDepositIncompatible())
                        {

                            idleTimer = idleTryNewAssignmentTime * Random.value * 0.5f;

                            SetIdle();

                        }
                    }

                    State = WorkerState.ToWithdraw;
                }
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

    // when withdrawing, make sure to only withdraw resources the depositable will accept
    // settle the worker transaction before the other transaction, in case the latter will trigger a new assignment
    void SettleTransaction(IWithdrawable from, IDepositable to, bool withdrawal)
    {
        Resources requested = Deposit.GetAvailableDeposits(from.GetCurrentResources());
        requested = to.GetAvailableDeposits(requested);

        if (withdrawal)
        {

            to.Deposit(requested); // to is the worker depositable
            from.Withdraw(requested);                    // from is where the withdrawal is being made
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

    public void SetNewAssociatable(Associatable calledBy)
    {
        if (calledBy.gameObject == Assignment.gameObject)
        {
            SetNewAssignment();
        }
    }

    bool AreWithdrawAndDepositIncompatible()
    {
        if (Deposit == null || Withdraw == null) return true;
        return (Deposit.GetAvailableDeposits(Withdraw.GetCurrentResources())).Weight == 0;
    }

    void SetNewAssignment()
    {
        Assignment = bankManager.GetAssignment(this);

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

    }

}
