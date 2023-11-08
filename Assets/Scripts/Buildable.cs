using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class Buildable : MonoBehaviour
{

	[SerializeField] BuildableManager buildableManager;
	[SerializeField] PlayerInputController input;
	[SerializeField] ResourceBankManager bankManager;
	BuildableData buildableData;
	SpriteRenderer myRenderer;
	Transform myTransform;
	Camera cam;

	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();
		myTransform = transform;
		cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		gameObject.SetActive(false);
	}

	public void SetPrefab(string name)
	{
		buildableData = buildableManager.GetBuildable(name);
		myRenderer.sprite = buildableData.prefab.GetComponent<SpriteRenderer>().sprite;
		gameObject.SetActive(true);
	}


	void Update()
	{
		Move();

		if (input.Click.triggered)
		{
			Build();
		}
	}

	void Move()
	{
		Vector3 xyz = cam.ScreenToWorldPoint(input.Pointer);
		myTransform.position = new Vector3(xyz.x, xyz.y, 0);
	}

	void Build()
	{
		GameObject buildable = Instantiate(buildableData.prefab);
		buildable.transform.position = myTransform.position;
		gameObject.SetActive(false);
		bankManager.AddBank(buildable.GetComponent<ResourceBank>());
	}

	void OnEnable()
	{
		input.GetComponent<ObjectSelector>().enabled = false;
	}

	void OnDisable()
	{
		input.GetComponent<ObjectSelector>().enabled = true;
	}

}
