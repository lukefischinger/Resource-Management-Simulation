using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
	MyActions actions;

	public InputAction Move => actions.Player.Move;
	public InputAction Click => actions.Player.Click;

	public InputAction Cancel => actions.Player.Cancel;
	public InputAction Scroll => actions.Player.Scroll;
	public Vector2 Pointer => actions.Player.Look.ReadValue<Vector2>();

	void OnEnable()
	{
		if (actions == null)
		{
			actions = new MyActions();
		}

		actions.Enable();
	}

	void OnDisable()
	{
		actions.Disable();
	}



}
