using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceProcessor : MonoBehaviour, IAssignable
{
    ResourceBank inBank, outBank;

    [SerializeField] List<Resource> inResources, outResources;
    SpriteRenderer myRenderer;
    Associatable inAssociatable, outAssociatable;

    Color baseColor;

    public int Priority { get; private set; } = 0;
    public void Select()
    {
        myRenderer.color = Color.cyan;
        inAssociatable.SelectAssociates();
        outAssociatable.SelectAssociates();

    }

    public void Deselect()
    {
        myRenderer.color = baseColor;
        inAssociatable.DeselectAssociates();
        outAssociatable.DeselectAssociates();
    }

    public Resources GetDisplayData()
    {
        List<Resource> list = inBank.myResources.ToList();
        list.AddRange(outBank.myResources.ToList());

        Resources result = new Resources(list);

        result.Add(inBank.myResources.Package);
        result.Add(outBank.myResources.Package);

        return result;
    }


    private void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        baseColor = myRenderer.color;

        inBank = GetComponent<ResourceBank>();
        outBank = transform.GetChild(0).GetComponent<ResourceBank>();

        inBank.Initialize(inResources);
        outBank.Initialize(outResources);

        inAssociatable = GetComponent<Associatable>();
        outAssociatable = transform.GetChild(0).GetComponent<Associatable>();

        transform.position = Vector2Int.FloorToInt(transform.position) + Vector2.one * 0.5f;

        foreach (Resource r in outResources)
        {
            if ((int)r > Priority)
            {
                Priority = (int)r;
            }
        }

        Priority += System.Enum.GetValues(typeof(Resource)).Cast<int>().Max() + 1;

    }



    void Convert()
    {
        if (inBank.Weight > 0)
        {
            float conversionAmount = inBank.Remove(inResources[0], inBank.myResources.Get(inResources[0]));
            outBank.Add(outResources[0], conversionAmount);
        }
    }


    private void Update()
    {
        Convert();
    }

    public void BeAssigned(ITransporter transporter)
    {
        throw new System.NotImplementedException();
    }

    public void BeUnassigned(ITransporter transporter)
    {
        throw new System.NotImplementedException();
    }




}
