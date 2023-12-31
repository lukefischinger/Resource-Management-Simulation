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

	bool shouldBuild = false;

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
			shouldBuild = true;
		}

		if(input.Cancel.triggered) {
			gameObject.SetActive(false);
		}
	}

    private void FixedUpdate()
    {
        if(shouldBuild) {
			Build();
			shouldBuild = false;
		}
    }

    void Move()
	{
		Vector3 xyz = cam.ScreenToWorldPoint(input.Pointer);
		myTransform.position = new Vector3(Mathf.Floor(xyz.x) + 0.5f, Mathf.Floor(xyz.y) + 0.5f, 0);
	}

	void Build()
	{
		GameObject buildable = Instantiate(buildableData.prefab);
		buildable.transform.position = myTransform.position;
		foreach (var bank in buildable.GetComponentsInChildren<ResourceBank>())
		{
			bankManager.Add(bank);
		}
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
