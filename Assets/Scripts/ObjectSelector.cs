using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class ObjectSelector : MonoBehaviour
{

    [SerializeField] UIDocument displayDoc;
    [SerializeField] VisualTreeAsset listEntryTemplate;


    List<ISelectable> selected = new List<ISelectable>(),
                      tempSelected = new List<ISelectable>();

    public List<string> selectedNames = new List<string>(),
                        tempSelectedNames = new List<string>();

    PlayerInputController input;
    PolygonCollider2D box;
    LineRenderer line;
    Transform myTransform;
    Camera cam;
    VisualElement root, resourceContainer;
    ISelectable displayedObject;

    void Awake()
    {
        input = GetComponent<PlayerInputController>();
        myTransform = transform;
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        box = GetComponent<PolygonCollider2D>();
        line = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        input.Click.started += StartClick;
        input.Click.canceled += EndClick;
        box.enabled = false;
        line.enabled = false;
        SubscribeToDisplayAction();
    }

    void OnDisable()
    {
        input.Click.started -= StartClick;
        input.Click.canceled -= EndClick;
        UnsubscribeFromDisplayAction();
    }

    void StartClick(InputAction.CallbackContext context)
    {
        Vector2 pt = PointerPosition();
        box.enabled = true;
        line.enabled = true;

        box.SetPath(0, new Vector2[] { pt, pt, pt, pt });
        line.SetPositions(new Vector3[] { pt, pt, pt, pt });

        RaycastHit2D hit = Physics2D.Raycast(PointerPosition(false), Vector3.back);


        if (hit.collider != null)
        {
            ISelectable selectable = hit.collider.GetComponentInParent<ISelectable>();
            if (selectable != null)
            {
                if (!AreWorkersSelected())
                {
                    DeselectAll();
                }

                tempSelected.Add(selectable);
                selectable.Select();
            }
        }
    }

    void DragClick()
    {
        float x = box.points[0].x,
              y = box.points[0].y;
        Vector2 pt = PointerPosition();

        box.SetPath(0, new Vector2[]
        {
            box.points[0],
            new Vector2(pt.x, y),
            pt,
            new Vector2(x, pt.y)
        });
        line.SetPositions(new Vector3[]
        {
            box.points[0],
            new Vector2(pt.x, y),
            pt,
            new Vector2(x, pt.y)
        });

    }

    void EndClick(InputAction.CallbackContext context)
    {
        if (AreWorkersSelected() && IsTempSelectionOneAssignable())
        {
            Assign(tempSelected[0] as IAssignable);
        }

        DeselectAll();
        selected = new List<ISelectable>(tempSelected);
        tempSelected.Clear();
        box.enabled = false;
        line.enabled = false;
        if (selected.Count > 0)
        {
            displayedObject = selected.First();
            CreateDisplayList(displayedObject.gameObject, (displayedObject.GetDisplayData()).Package);
        }
    }

    bool AreWorkersSelected()
    {
        return selected.Count > 0 && !IsNotWorker(selected[0]);
    }

    bool IsTempSelectionOneAssignable()
    {
        return tempSelected.Count == 1 && tempSelected[0] is IAssignable;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        ISelectable selectable = collider.GetComponent<ISelectable>();
        if (selectable == null || tempSelected.Contains(selectable)) return;

        if (selectable is Worker || tempSelected.Count == 0)
        {
            if (tempSelected.Count > 0 && IsNotWorker(tempSelected[0]))
            {
                RemoveNotWorkers(tempSelected);
            }

            tempSelected.Add(selectable);

            if (!IsTempSelectionOneAssignable())
            {
                DeselectAll();
            }

            selectable.Select();
        }
    }

    bool IsNotWorker(ISelectable selectable)
    {
        return selectable.gameObject.GetComponent<Worker>() == null;
    }

    void RemoveNotWorkers(List<ISelectable> list)
    {
        foreach (var entry in list)
        {
            if (IsNotWorker(entry))
            {
                entry.Deselect();
            }
        }

        list.RemoveAll(IsNotWorker);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        ISelectable selectable = collider.GetComponent<ISelectable>();
        if (selectable != null && tempSelected.Contains(selectable))
        {
            tempSelected.Remove(selectable);
            selectable.Deselect();
        }
    }


    void FixedUpdate()
    {
        if (box.enabled)
        {
            DragClick();
        }
    }

    void DeselectAll()
    {
        if (selected.Count == 0) return;

        foreach (ISelectable obj in selected)
        {
            obj.Deselect();
        }
        HideDisplayList();
        selected.Clear();
    }

    void Assign(IAssignable assignment)
    {
        foreach (ISelectable item in selected)
        {
            Worker w = item.gameObject.GetComponent<Worker>();
            if (w != null && !assignment.gameObject.GetComponent<Associatable>().AtCapacity)
            {
                w.Assign(assignment);
            }
        }

        DeselectAll();
    }

    void CreateDisplayList(GameObject selectedObj, Dictionary<Resource, float> dict)
    {
        displayDoc.gameObject.SetActive(true);
        SubscribeToDisplayAction();


        root = displayDoc.rootVisualElement;
        VisualElement selectedObject = root.Q("selected-object");
        Label title = selectedObject.Q<Label>("object-name");
        title.text = selectedObj.name;


        resourceContainer = selectedObject.Q("resources");
        resourceContainer.Clear();

        foreach (var elt in dict)
        {
            VisualElement item = listEntryTemplate.Instantiate();
            item.name = elt.Key.ToString();
            item.Q<Label>("resource").text = elt.Key.ToString();
            item.Q<Label>("quantity").text = elt.Value.ToString();
            resourceContainer.Add(item);
        }

    }

    void UpdateDisplayList()
    {
        VisualElement item;
        foreach (var kvp in selected.Last().GetDisplayData().Package)
        {
            item = resourceContainer.Q<VisualElement>(kvp.Key.ToString());
            item.Q<Label>("quantity").text = kvp.Value.ToString();
        }
    }


    void HideDisplayList()
    {
        UnsubscribeFromDisplayAction();

        displayDoc.rootVisualElement.Q("resources").Clear();
        displayDoc.rootVisualElement.Q<Label>("object-name").text = "";

        displayedObject = null;
    }



    Vector2 PointerPosition(bool adjusted = true)
    {
        return cam.ScreenToWorldPoint(input.Pointer) - (adjusted ? myTransform.position : Vector2.zero);
    }


    void UnsubscribeFromDisplayAction()
    {
        if (displayedObject == null) return;

        foreach (var bank in displayedObject.gameObject.GetComponentsInChildren<ResourceBank>())
        {
            bank.Changed -= UpdateDisplayList;
        }
    }

    void SubscribeToDisplayAction()
    {
        if (displayedObject == null) return;

        foreach (var bank in displayedObject.gameObject.GetComponentsInChildren<ResourceBank>())
        {
            bank.Changed += UpdateDisplayList;
        }
    }




}
