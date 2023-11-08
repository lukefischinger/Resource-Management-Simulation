using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Worker : MonoBehaviour, ISelectable, ITransporter
{
	const float moveSpeed = 3f;
	public enum WorkerState { Idling, ToWithdraw, ToDeposit };
	private WorkerState state = WorkerState.Idling;
	private Transform withdrawTransform, depositTransform, myTransform;
	private Rigidbody2D myRigidbody;
	private SpriteRenderer myRenderer;
	private bool isSelected;
	private Depositable myDeposit;
	private Withdrawable myWithdraw;
	private ResourceBank myBank;
	private ResourceBankManager bankManager;
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
			if(isSelected) 
			{
				withdraw?.gameObject.GetComponent<ISelectable>()?.Deselect();
				value?.gameObject.GetComponent<ISelectable>()?.Select();
			}
			
			withdraw = value;
			if (value != null)
			{
				withdrawTransform = value.transform;
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
			if(isSelected) 
			{
				deposit?.gameObject.GetComponent<ISelectable>()?.Deselect();
				value?.gameObject.GetComponent<ISelectable>()?.Select();
			}
			deposit = value;
			if (value != null)
			{
				depositTransform = value.transform;
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
	}

	void Move(Transform destination)
	{
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

	void SetClosestWithdraw(bool assignableOnly = false)
	{
		Withdrawable temp = null;

        Withdraw?.GetComponent<Associatable>().RemoveAssociation();

        bankManager.GetClosest<Withdrawable>(
			myTransform,
			new Resources(Mathf.Infinity),
			assignableOnly
		)?.TryGetComponent(out temp);

		Withdraw = temp;
        Withdraw?.GetComponent<Associatable>().AddAssociation();

    }

    void SetClosestDeposit(bool assignableOnly = false)
	{
		Depositable temp = null;

        Deposit?.GetComponent<Associatable>().RemoveAssociation();
        bankManager.GetClosest<Depositable>(
			myTransform,
			assignableOnly ? new Resources(Mathf.Infinity) : (IsCarrying() ? myBank.myResources : Withdraw.GetCurrentResources()),
			assignableOnly
		)?.TryGetComponent(out temp);
		Deposit = temp;
		Deposit?.GetComponent<Associatable>().AddAssociation();
	}

	bool IsAssignmentWithdrawable()
	{
		return Assignment.gameObject.TryGetComponent<Withdrawable>(out _);
	}

	public void Unassign(IAssignable assignable)
	{

	}


	void OnCollisionEnter2D(Collision2D collision)
	{
		if (state == WorkerState.ToWithdraw && Withdraw != null && collision.gameObject == Withdraw.gameObject)
		{
			SettleTransaction(collision.gameObject.GetComponent<IWithdrawable>(), myDeposit);
			AfterWithdrawal();
		}
		else if (state == WorkerState.ToDeposit && Deposit != null && collision.gameObject == Deposit.gameObject)
		{
			SettleTransaction(myWithdraw, collision.gameObject.GetComponent<IDepositable>());
			AfterDeposit();
		}
	}

	void AfterDeposit()
	{
		if (IsCarrying())
		{
			SetClosestDeposit();
			state = WorkerState.ToDeposit;
			return;
		}
		else
		{
			state = WorkerState.ToWithdraw;
		}

		if (!IsAssignmentWithdrawable())
		{
			SetClosestWithdraw();
			if (Deposit.GetAvailableDeposits(Withdraw.GetCurrentResources()).Weight == 0)
			{
				SetClosestDeposit(true);
				if (Deposit != null)
				{
					Unassign(Assignment);
					Assign(Deposit.GetComponent<IAssignable>());

				}
				else
				{
					state = WorkerState.Idling;
				}
			}
		}
	}

	void AfterWithdrawal()
	{
		state = WorkerState.ToDeposit;

		// if the worker was assigned a withdrawable, then have it choose the most convenient depositable each time
		if (IsAssignmentWithdrawable())
		{
			SetClosestDeposit();
			if (Withdraw.GetCurrentResources().Weight == 0)
			{
				SetClosestWithdraw(true);
				if (Withdraw != null)
				{
					Unassign(Assignment);
					Assign(Withdraw.GetComponent<IAssignable>());

					if (IsCarrying())
					{
						state = WorkerState.ToDeposit;
					}
					else
					{
						state = WorkerState.ToWithdraw;
					}
				}
				else if (Deposit != null && IsCarrying())
				{
					state = WorkerState.ToDeposit;
				}
				else
				{
					state = WorkerState.Idling;
				}
			}


		}
	}


	bool IsCarrying()
	{
		return myBank.myResources.Weight > 0;
	}

	void SettleTransaction(IWithdrawable from, IDepositable to)
	{
		to.Deposit(from.Withdraw(to.GetAvailableDeposits(from.GetCurrentResources())));
	}

	public void Select()
	{
		myRenderer.color = Color.red;
		myRenderer.sortingOrder = 1;
		isSelected = true;
		Deposit?.gameObject.GetComponent<ISelectable>()?.Select();
		Withdraw?.gameObject.GetComponent<ISelectable>()?.Select();

	}

	public void Deselect()
	{
		myRenderer.color = Color.white;
		myRenderer.sortingOrder = 0;
		isSelected = false;
		Deposit?.gameObject.GetComponent<ISelectable>()?.Deselect();
		Withdraw?.gameObject.GetComponent<ISelectable>()?.Deselect();
	}

	public Resources GetDisplayData()
	{
		return myWithdraw.GetCurrentResources();
	}

    public void SetNewDeposit()
    {
        throw new System.NotImplementedException();
    }

    public void SetNewWithdraw()
    {
        throw new System.NotImplementedException();
    }
}
