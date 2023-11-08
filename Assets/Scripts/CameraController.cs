using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

	[SerializeField] PlayerInputController input;

	const float maxOrtho = 40, minOrtho = 5;

	Transform myTransform;

	Camera cam;
	private float cameraSpeed = 2f;
	bool moving = false;
	Vector3 movementDirection = Vector3.zero;

	void Awake()
	{
		myTransform = transform;
		cam = GetComponentInChildren<Camera>();
	}

	void OnEnable()
	{

		input.Scroll.performed += Zoom;
		input.Move.performed += StartMove;
		input.Move.canceled += StopMove;

		moving = false;
	}

	void OnDisable()
	{
		input.Scroll.performed -= Zoom;
		input.Move.performed -= StartMove;
		input.Move.canceled -= StopMove;

	}



	void Update()
	{
		if (moving)
		{
			Move();
		}
	}

	void Move()
	{
		myTransform.position = myTransform.position + cameraSpeed * cam.orthographicSize * Time.deltaTime * movementDirection;
	}

	void StartMove(InputAction.CallbackContext context)
	{
		movementDirection = context.ReadValue<Vector2>();
		moving = true;
	}

	private void StopMove(InputAction.CallbackContext context)
	{
		moving = false;
	}

	void Zoom(InputAction.CallbackContext context)
	{
		cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - context.action.ReadValue<float>() / 100f, minOrtho, maxOrtho);
	}


}
