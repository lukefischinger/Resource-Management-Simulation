using UnityEngine;
using UnityEngine.UIElements;

public class Worker : MonoBehaviour, ISelectable, ITransporter
{
    const float moveSpeed = 3f;
    public enum WorkerState { Idling, ToWithdraw, ToDeposit };
    public WorkerState state = WorkerState.Idling;
    private Transform withdrawTransform, depositTransform, myTransform;
    private Rigidbody2D myRigidbody;
    private SpriteRenderer myRenderer;
    private bool hasNewAssignment;
    private const float idleTryNewAssignmentTime = 10f;
    private float idleTimer;
    private Depositable myDeposit;
    private Withdrawable myWithdraw;
    private ResourceBank myBank;
    private ResourceBankManager bankManager;
    private bool IsWithdrawing => state == WorkerState.ToWithdraw;
    private bool IsDepositing => state == WorkerState.ToDeposit;
    public IAssignable Assignment { get; private set; }
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
    }

    void FixedUpdate()
    {
        switch (state)
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
        if (IsCarrying())
        {
            SetClosestDeposit();
        }
        else if (idleTimer < 0)
        {
            SetNewAssignment();
            idleTimer = idleTryNewAssignmentTime * (Random.Range(0.2f, 1f));
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
        myRigidbody.velocity = moveSpeed * (destination.position - myTransform.position).normalized;
    }

    public void Assign(IAssignable assignable)
    {
        if (assignable == Assignment) return;

        Assignment = assignable;
        SetDepositAndWithdraw(Assignment);

        if (IsCarrying())
        {
            state = WorkerState.ToDeposit;
        }
        else
        {
            state = WorkerState.ToWithdraw;
        }
    }

    void SetDepositAndWithdraw(IAssignable assignable)
    {

        if (assignable.gameObject.TryGetComponent(out Depositable temp1))
        {
            Deposit = temp1;

            SetClosestWithdraw();

        }
        else if (assignable.gameObject.TryGetComponent(out Withdrawable temp2))
        {

            Withdraw = temp2;

            SetClosestDeposit();
        }
    }

    void SetClosestWithdraw()
    {
        ResourceBank bank;

        Withdraw = null;

        bank = bankManager.GetClosest<Withdrawable>(myTransform, GetResourcesSought(false));

        if (bank != null)
        {
            Withdraw = bank.GetComponent<Withdrawable>();


        }
        else
        {
            SetNewAssignment();
        }

    }

    void SetClosestDeposit()
    {
        ResourceBank bank;

        Deposit = null;

        bank = bankManager.GetClosest<Depositable>(myTransform, GetResourcesSought(true));

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

    public void Unassign(IAssignable assignable)
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
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
                state = WorkerState.ToDeposit;
                if (IsAssignmentWithdrawable())
                {
                    SetClosestDeposit();
                    if (Deposit == null)
                    {
                        SetNewAssignment();
                    }
                }
            }
            else
            {
                SettleTransaction(myWithdraw, collision.gameObject.GetComponent<IDepositable>(), false);
                if (IsCarrying())
                {
                    SetClosestDeposit();
                    state = WorkerState.ToDeposit;
                }
                else if (hasNewAssignment)
                {
                    SetDepositAndWithdraw(Assignment);
                    hasNewAssignment = false;
                    state = WorkerState.ToWithdraw;
                }
                else
                {
                    state = WorkerState.ToWithdraw;
                    if (IsAssignmentDepositable())
                    {
                        SetClosestWithdraw();
                        if (Withdraw == null)
                        {
                            SetNewAssignment();
                        }
                    }
                }
            }
        }
    }

    bool IsMyTarget(GameObject gameObject)
    {
        return (state == WorkerState.ToWithdraw &&
                Withdraw != null &&
                (gameObject == Withdraw.gameObject ||
                (gameObject.transform.childCount > 0 &&
                    gameObject.transform.GetChild(0).gameObject == Withdraw.gameObject))) ||
               (state == WorkerState.ToDeposit && Deposit != null && gameObject == Deposit.gameObject);
    }

    bool IsCarrying()
    {
        return myBank.myResources.Weight > 0;
    }

    // when withdrawing for an assigned depositable, make sure to only withdraw resources the depositable will accept
    // settle the worker transaction before the other transaction, in case the latter will trigger a new assignment
    void SettleTransaction(IWithdrawable from, IDepositable to, bool withdrawal)
    {
        Resources requested = from.GetCurrentResources();
        if (IsAssignmentDepositable())
        {
            requested = Deposit.GetAvailableDeposits(requested);
        }
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
        else if (Withdraw != null && calledBy.gameObject == Withdraw.gameObject)
        {
            SetClosestWithdraw();

        }
        else if (Deposit != null && calledBy.gameObject == Deposit.gameObject)
        {
            SetClosestDeposit();

        }

    }


    void SetNewAssignment()
    {
        ResourceBank bank = bankManager.GetClosest<ITransactionable>(
                myTransform,
                GetResourcesSought(true, true),
                true
        );

        if (bank != null)
        {
            Assignment = bank.GetComponent<IAssignable>();
        }
        else
        {
            Assignment = null;
            SetIdle();
            return;
        }

        if (IsCarrying())
        {
            hasNewAssignment = true;
            SetClosestDeposit();
            state = WorkerState.ToDeposit;
        }
        else
        {
            SetDepositAndWithdraw(Assignment);
            state = WorkerState.ToWithdraw;

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

    void SetIdle() {
        state = WorkerState.Idling;
        Deposit = null;
        Withdraw = null;
    }

}
