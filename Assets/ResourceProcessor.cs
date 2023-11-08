using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceProcessor : MonoBehaviour, IAssignable
{
    ResourceBank input, output;

    [SerializeField] Resource inResource, outResource;
    SpriteRenderer myRenderer;
    Color baseColor;

    public void Select()
    {
        myRenderer.color = Color.cyan;
    }

    public void Deselect()
    {
        myRenderer.color = baseColor;
    }

    public Resources GetDisplayData()
    {
        List<Resource> list = input.myResources.ToList();
        list.AddRange(output.myResources.ToList());

        Resources result = new Resources(list);

        result.Add(input.myResources.Package);
        result.Add(output.myResources.Package);

        return result;
    }

   
    private void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        baseColor = myRenderer.color;

        input = GetComponent<ResourceBank>();
        output = transform.GetChild(0).GetComponent<ResourceBank>();

        input.Initialize(new List<Resource>() { inResource });
        output.Initialize(new List<Resource>() { outResource });
    }



    void Convert() {
        if(input.Weight > 0) {
            float conversionAmount = input.Remove(inResource, input.myResources.Get(inResource));
            output.Add(outResource, conversionAmount);
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
