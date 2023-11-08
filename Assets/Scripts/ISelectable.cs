using System.Linq;
using UnityEngine;

public interface ISelectable
{
    public void Select();
    public void Deselect();
    public Resources GetDisplayData();
    Transform transform { get; }
    GameObject gameObject { get; }
}

public interface ITransactionable
{
    public static Resources GetMaxTransaction(Resources fromLimits, Resources toLimits, float fromLimit, float toLimit)
    {
        Resources result = new Resources();
        float totalLimit = Mathf.Min(fromLimit, toLimit);

        foreach (var e in fromLimits.Package)
        {
            if (toLimits.Contains(e.Key))
            {
                result.Add(e.Key, Mathf.Min(e.Value, toLimits.Get(e.Key)));
                if (result.Weight > totalLimit)
                {
                    result.Remove(e.Key, (result.Weight - totalLimit) / ResourceWeights.Weight(e.Key));
                    break;
                }
            }
        }

        return result;
    }
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

    public void Unassign(IAssignable assignment);

    public void SetNewDeposit();

    public void SetNewWithdraw();
}


public interface IAssignable : ISelectable
{
    public void BeAssigned(ITransporter transporter);
    public void BeUnassigned(ITransporter transporter);
}



