using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class Worker : MonoBehaviour, ISelectable, ITransporter
{

    // Debugging fields

    public string depositName, withdrawName, assignmentName;

    // end debugging


    const float moveSpeed = 3f;
    public enum WorkerState { Idling, ToWithdraw, ToDeposit };
    public WorkerState state = WorkerState.Idling;
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
    private bool IsWithdrawing => state == WorkerState.ToWithdraw;
    private bool IsDepositing => state == WorkerState.ToDeposit;
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
            TryAddAssociation(value);
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
            TryAddAssociation(value);
            depositTransform = (value == null) ? null : value.transform;

            depositName = (deposit == null) ? "null" : deposit.gameObject.name;


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

    async void Update()
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
                await Idle();
                break;
            default:
                break;
        }
    }

    async Task Idle()
    {

        myRigidbody.velocity = Vector2.zero;
        if (idleTimer < 0)
        {
            await SetNewAssignment();
            idleTimer = idleTryNewAssignmentTime;
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

    public async Task Assign(IAssignable assignable)
    {
        if (assignable == Assignment) return;

        Assignment = assignable;
        await SetDepositAndWithdraw(Assignment);

        if (IsCarrying())
        {
            state = WorkerState.ToDeposit;
        }
        else
        {
            state = WorkerState.ToWithdraw;
        }
    }

    async Task SetDepositAndWithdraw(IAssignable assignable)
    {

        if (assignable.gameObject.TryGetComponent(out Depositable temp1))
        {
            Deposit = temp1;

            await SetClosestWithdraw();

        }
        else if (assignable.gameObject.TryGetComponent(out Withdrawable temp2))
        {

            Withdraw = temp2;

            await SetClosestDeposit();
        }
    }

    async Task SetClosestWithdraw()
    {
        ResourceBank bank;

        Withdraw = null;

        bank = await bankManager.GetClosest<Withdrawable>(this, await GetResourcesSought(false));

        if (bank != null)
        {
            Withdraw = bank.GetComponent<Withdrawable>();


        }
        else
        {
            await SetNewAssignment();
        }

    }

    async Task SetClosestDeposit()
    {
        ResourceBank bank;

        Deposit = null;

        bank = await bankManager.GetClosest<Depositable>(this, await GetResourcesSought(true));

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


    async void OnCollisionEnter2D(Collision2D collision)
    {
        await PerformDepositOrWithdrawal(collision);
    }

    async Task PerformDepositOrWithdrawal(Collision2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        if (IsMyTarget(collidedObject))
        {
            if (IsWithdrawing)
            {
                await SettleTransaction(collision.gameObject.GetComponentInChildren<IWithdrawable>(), myDeposit, true);
                state = WorkerState.ToDeposit;
                if (IsAssignmentWithdrawable())
                {
                    if (await AreWithdrawAndDepositIncompatible())
                    {
                        await SetClosestDeposit();
                    }
                    if (Deposit == null)
                    {
                        await SetNewAssignment();
                    }
                }
            }
            else
            {
                await SettleTransaction(myWithdraw, collision.gameObject.GetComponent<IDepositable>(), false);
                if (IsCarrying())
                {
                    await SetClosestDeposit();
                    state = WorkerState.ToDeposit;
                }
                else if (hasNewAssignment)
                {
                    await SetDepositAndWithdraw(Assignment);
                    hasNewAssignment = false;
                    state = WorkerState.ToWithdraw;
                }
                else
                {
                    state = WorkerState.ToWithdraw;
                    if (IsAssignmentDepositable())
                    {
                        if (await AreWithdrawAndDepositIncompatible())
                        {
                            await SetClosestWithdraw();
                        }
                        if (Withdraw == null)
                        {
                            await SetNewAssignment();
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

    // when withdrawing, make sure to only withdraw resources the depositable will accept
    // settle the worker transaction before the other transaction, in case the latter will trigger a new assignment
    async Task SettleTransaction(IWithdrawable from, IDepositable to, bool withdrawal)
    {
        Resources requested = await Deposit.GetAvailableDeposits(await from.GetCurrentResources());
        requested = await to.GetAvailableDeposits(requested);

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

        Debug.Log(gameObject.name + (withdrawal ? " withdrew from " + from.gameObject.name : "deposited to " + to.gameObject.name));
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

    public async Task<Resources> GetDisplayData()
    {
        return await myWithdraw.GetCurrentResources();
    }

    public async Task SetNewAssociatable(Associatable calledBy)
    {
        if (calledBy.gameObject == Assignment.gameObject)
        {
            await SetNewAssignment();
        }
        else if (Withdraw != null && calledBy.gameObject == Withdraw.gameObject)
        {
            await SetClosestWithdraw();
        }
        else if (Deposit != null && calledBy.gameObject == Deposit.gameObject)
        {
            await SetClosestDeposit();
        }
    }

    async Task<bool> AreWithdrawAndDepositIncompatible()
    {
        if (Deposit == null || Withdraw == null) return true;
        return (await Deposit.GetAvailableDeposits(await Withdraw.GetCurrentResources())).Weight == 0;
    }

    async Task SetNewAssignment()
    {
        Assignment = await bankManager.GetAssignment(this);

        if (Assignment == null)
        {
            SetIdle();
        }
        else if (IsCarrying())
        {
            hasNewAssignment = true;
            await SetClosestDeposit();
            state = WorkerState.ToDeposit;
        }
        else
        {
            await SetDepositAndWithdraw(Assignment);
            state = WorkerState.ToWithdraw;
        }
    }


    async Task<Resources> GetResourcesSought(bool isForDeposit, bool isForAssignment = false)
    {
        if (isForAssignment)
            return Resources.Infinity;
        else if (IsCarrying())
        {
            return myBank.myResources;
        }
        else if (isForDeposit)
        {
            return Withdraw == null ? new Resources() : await Withdraw.GetCurrentResources();
        }
        else
        {
            return Deposit == null ? new Resources() : await Deposit.GetAvailableDeposits(Resources.Infinity);
        }
    }

    void SetIdle()
    {
        state = WorkerState.Idling;
        Deposit = null;
        Withdraw = null;
    }

}
