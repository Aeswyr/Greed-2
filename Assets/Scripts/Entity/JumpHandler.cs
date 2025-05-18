using UnityEngine;

public class JumpHandler : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D rbody;

	[Header("Jump Info")]
	[SerializeField]
	private float velocityScalar;

	[SerializeField]
	private float terminalVelocity;

	[SerializeField]
	private float minJumpTime;

	[SerializeField]
	private AnimationCurve risingCurve;

	[SerializeField]
	private AnimationCurve fallingCurve;

	private float currentTerminalVelocity;

	private InputHandler input;

	private float timeStamp = -100f;

	private bool jumping;

	private bool starting;

	private AnimationCurve curve;

	private float risingTime;

	private float fallingTime;

	private float maxTime;

	private bool paused;

	private float pausedUntil;

	private float pauseStarted;

	private float storedVelocity;

	[SerializeField]
	private float gravity;

	private void Start()
	{
		risingTime = risingCurve[risingCurve.length - 1].time;
		fallingTime = fallingCurve[fallingCurve.length - 1].time;
		currentTerminalVelocity = terminalVelocity;
	}

	private void FixedUpdate()
	{
		if (!(Time.time < pausedUntil))
		{
			if (paused)
			{
				paused = false;
				rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, storedVelocity);
				timeStamp += pausedUntil - pauseStarted;
				rbody.gravityScale = gravity;
			}
			if (starting && (((input.jump.released || !input.jump.down) && Time.time - timeStamp > minJumpTime) || Time.time - timeStamp > risingTime))
			{
				EndJump();
			}
			if (Time.time - timeStamp <= maxTime)
			{
				rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, velocityScalar * curve.Evaluate(Time.time - timeStamp));
			}
			if (rbody.linearVelocity.y < 0f - currentTerminalVelocity)
			{
				rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, 0f - currentTerminalVelocity);
			}
		}
	}

	public void StartJump()
	{
		timeStamp = Time.time;
		starting = true;
		curve = risingCurve;
		maxTime = risingTime;
	}

	private void EndJump()
	{
		timeStamp = Time.time;
		starting = false;
		curve = fallingCurve;
		maxTime = fallingTime;
	}

	public void ForceLanding()
	{
		starting = false;
		timeStamp = -100f;
	}

	public void Pause(float endPause)
	{
		if (paused)
		{
			if (endPause > pausedUntil)
			{
				pausedUntil = endPause;
			}
			return;
		}
		paused = true;
		pauseStarted = Time.time;
		pausedUntil = endPause;
		storedVelocity = rbody.linearVelocity.y;
		rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, 0f);
		rbody.gravityScale = 0f;
	}

	public void DisableGravity()
	{
		rbody.gravityScale = 0f;
	}

	public void SetGravity(float gravity)
	{
		rbody.gravityScale = gravity;
	}

	public void ResetGravity()
	{
		rbody.gravityScale = gravity;
	}

	public void ForceVelocity(float velocity)
	{
		rbody.linearVelocity = new Vector2(rbody.linearVelocity.x, velocity);
	}

	public void SetTerminalVelocity(float val)
	{
		currentTerminalVelocity = val;
	}

	public void ResetTerminalVelocity()
	{
		currentTerminalVelocity = terminalVelocity;
	}

	public void SetInput(InputHandler input)
	{
		this.input = input;
	}
}
