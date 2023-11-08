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
	}

	void OnDisable()
	{
		input.Click.started -= StartClick;
		input.Click.canceled -= EndClick;
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
			ISelectable selectable = hit.collider.GetComponent<ISelectable>();
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

		if (selected.Count > 0)
			CreateDisplayList(selected.Last().gameObject.name, selected.Last().GetDisplayData().Package);

	}

	void DeselectAll()
	{
		if (selected.Count == 0) return;

		foreach (ISelectable obj in selected)
		{
			obj.Deselect();
		}
		selected.Clear();
		HideDisplayList();
	}

	void Assign(IAssignable assignment)
	{
		foreach (ISelectable item in selected)
		{
			Worker w = item.gameObject.GetComponent<Worker>();
			if (w != null)
			{
				w.Assign(assignment);
			}
		}

		DeselectAll();
	}

	void CreateDisplayList(string name, Dictionary<Resource, float> dict)
	{
		displayDoc.gameObject.SetActive(true);

		VisualElement selectedObject = displayDoc.rootVisualElement.Q("selected-object");
		Label title = selectedObject.Q<Label>("object-name");
		title.text = name;

		VisualElement resourceContainer = selectedObject.Q("resources");
		resourceContainer.Clear();

		foreach (var elt in dict)
		{
			VisualElement item = listEntryTemplate.Instantiate();
			item.Q<Label>("resource").text = elt.Key.ToString();
			item.Q<Label>("quantity").text = elt.Value.ToString();
			resourceContainer.Add(item);
		}

	}


	void HideDisplayList()
	{
		displayDoc.rootVisualElement.Q("resources").Clear();
		displayDoc.rootVisualElement.Q<Label>("object-name").text = "";
	}



	Vector2 PointerPosition(bool adjusted = true)
	{
		return cam.ScreenToWorldPoint(input.Pointer) - (adjusted ? myTransform.position : Vector2.zero);
	}




}
