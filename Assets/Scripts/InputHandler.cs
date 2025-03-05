using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
	public struct ButtonState
	{
		private short id;

		private static short STATE_PRESSED = 0;

		private static short STATE_RELEASED = 1;

		private InputHandler handler;

		private bool firstFrame;

		public bool down { get; private set; }

		public bool pressed
		{
			get
			{
				if (handler.bufferEnabled && handler.inputBuffer != null)
				{
					foreach (Dictionary<short, short> item in handler.inputBuffer)
					{
						if (item.ContainsKey(id) && item[id] == STATE_PRESSED)
						{
							return item.Remove(id);
						}
					}
					return false;
				}
				return down && firstFrame;
			}
		}

		public bool released
		{
			get
			{
				if (handler.bufferEnabled && handler.inputBuffer != null)
				{
					foreach (Dictionary<short, short> item in handler.inputBuffer)
					{
						if (item.ContainsKey(id) && item[id] == STATE_RELEASED)
						{
							return item.Remove(id);
						}
					}
					return false;
				}
				return !down && firstFrame;
			}
		}

		public void Set(InputAction.CallbackContext ctx)
		{
			down = !ctx.canceled;
			firstFrame = true;
			if (handler.bufferEnabled && handler.currentFrame != null)
			{
				handler.currentFrame.TryAdd(id, down ? STATE_PRESSED : STATE_RELEASED);
			}
		}

		public void Reset()
		{
			firstFrame = false;
		}

		public void Init(ref short IDSRC, InputHandler handler)
		{
			id = IDSRC++;
			this.handler = handler;
		}
	}

	public DeviceType activeDevice;

	[SerializeField]
	private int buttonCount = 1;

	[SerializeField]
	private short bufferFrames = 5;

	[SerializeField]
	private bool bufferEnabled = false;

	private short IDSRC = 0;

	private ButtonState[] buttons;

	private Queue<Dictionary<short, short>> inputBuffer = new Queue<Dictionary<short, short>>();

	private Dictionary<short, short> currentFrame;

	public float dir { get; private set; }

	public Vector2 aim { get; private set; }

	public ButtonState move => buttons[0];

	public ButtonState jump => buttons[1];

	public ButtonState attack => buttons[2];

	public ButtonState dodge => buttons[3];

	public ButtonState item => buttons[4];

	public ButtonState interact => buttons[5];

	public ButtonState menu => buttons[6];

	public ButtonState any => buttons[7];


	public void Start()
	{
		buttons = new ButtonState[buttonCount];
		for (int i = 0; i < buttonCount; i++)
		{
			buttons[i].Init(ref IDSRC, this);
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < buttonCount; i++)
		{
			buttons[i].Reset();
		}
		if (bufferEnabled)
		{
			UpdateBuffer();
		}
	}

	public void Move(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		Debug.Log("moving!!");

		UpdateActiveController(ctx);
		dir = ctx.ReadValue<float>();
		buttons[0].Set(ctx);
		Vector2 vector = aim;
		vector.x = dir;
		aim = vector;
	}

	public void Aim(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		Vector2 vector = aim;
		vector.y = ctx.ReadValue<float>();
		aim = vector;
	}

	public void Jump(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[1].Set(ctx);
	}

	public void Primary(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[2].Set(ctx);
	}

	public void Secondary(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[3].Set(ctx);
	}

	public void Item(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[4].Set(ctx);
	}

	public void Interact(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[5].Set(ctx);
	}

	public void Menu(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[6].Set(ctx);
	}

	public void Any(InputAction.CallbackContext ctx)
	{
		if (buttons == null) {
			return;
		}

		UpdateActiveController(ctx);
		buttons[7].Set(ctx);
	}

	public void FlushBuffer()
	{
		inputBuffer.Clear();
	}

	public void UpdateBuffer()
	{
		if (inputBuffer.Count >= bufferFrames)
		{
			inputBuffer.Dequeue();
		}
		currentFrame = new Dictionary<short, short>();
		inputBuffer.Enqueue(currentFrame);
	}

	public void PrintBuffer()
	{
		string text = $"InputBuffer: count-{inputBuffer.Count}";
		foreach (Dictionary<short, short> item in inputBuffer)
		{
			if (item.Count > 0)
			{
				text += $"\n{item.Count}";
			}
		}
		Debug.Log(text);
	}

	public void UpdateActiveController(InputAction.CallbackContext ctx)
	{
		if (ctx.action.activeControl.device is Gamepad)
		{
			activeDevice = DeviceType.GAMEPAD;
		}
		else
		{
			activeDevice = DeviceType.KEYBOARD;
		}
	}
}
