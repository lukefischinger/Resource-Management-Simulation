using UnityEngine;

public class Associatable : MonoBehaviour
{
    public int Associates { get; private set; }
    public int maxAssociates { get; private set; } = 10;

    public bool AtCapacity => Associates == maxAssociates;

    public void AddAssociation()
    {
        Associates++;
    }

    public void RemoveAssociation()
    {
        Associates--;
    }
}
