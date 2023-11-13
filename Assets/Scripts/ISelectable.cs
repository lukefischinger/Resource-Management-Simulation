using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ISelectable
{
    public void Select();
    public void Deselect();
    public Resources GetDisplayData();
    public Transform transform { get; }
    public GameObject gameObject { get; }

}

public interface ITransactionable
{
    public static Resources GetMaxTransaction(Resources fromLimits, Resources toLimits, float fromLimit, float toLimit)
    {
        List<Resource> intersection = fromLimits.ToList().Intersect(toLimits.ToList()).ToList();
        Resources result = new Resources(intersection, 0f);
        float totalLimit = Mathf.Min(fromLimit, toLimit);

        foreach (var resource in result.ToList())
        {
            result.Add(resource, Mathf.Min(fromLimits.Get(resource), toLimits.Get(resource), totalLimit));
            if (result.Weight > totalLimit)
            {
                result.Remove(resource, (result.Weight - totalLimit) / ResourceWeights.Weight(resource));
                break;
            }
        }

        return result;
    }

    public GameObject gameObject { get; }
}

public interface IDepositable : ITransactionable
{
    public Resources Deposit(Resources resources); // returns overflow
    public Resources GetAvailableDeposits(Resources attempt);
}

public interface IWithdrawable : ITransactionable
{
    public Resources Withdraw(Resources resources);
    public Resources GetAvailableWithdrawals(Resources attempt);
    public Resources GetCurrentResources();
}

public interface ITransporter
{
    public void Assign(IAssignable assignment);

    public void SetNewAssociatable(Associatable calledBy);

    public GameObject gameObject { get; }
}


public interface IAssignable : ISelectable
{
    public void BeAssigned(ITransporter transporter);
    public void BeUnassigned(ITransporter transporter);
    public int Priority { get; }

}



