using Mono.Cecil;
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

        Dictionary<Resource, float> result = new Dictionary<Resource, float>();

        float totalLimit = Mathf.Min(fromLimit, toLimit);
        float currentWeight = 0;
        foreach (var kv in fromLimits.Package)
        {
            if (kv.Value == 0) continue;

            if (toLimits.Package.ContainsKey(kv.Key))
            {
                result.Add(kv.Key, Mathf.Min(kv.Value, (totalLimit - currentWeight) / ResourceWeights.Weight(kv.Key)));
                currentWeight += ResourceWeights.Weight(kv.Key) * result[kv.Key];

                if (currentWeight == totalLimit)
                {
                    break;
                }
                else if (currentWeight > totalLimit)
                {
                    result[kv.Key] -= (currentWeight - totalLimit) / ResourceWeights.Weight(kv.Key);
                    break;
                }
            }
        }


        return new Resources(result);
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

    public GameObject gameObject { get; }
}


public interface IAssignable : ISelectable
{
    public void BeAssigned(ITransporter transporter);
    public void BeUnassigned(ITransporter transporter);
    public int Priority { get; }

}



