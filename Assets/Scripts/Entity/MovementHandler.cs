using UnityEngine;

public class MovementHandler : MonoBehaviour
{
	[SerializeField]
	private float speed;

	[SerializeField]
	private Rigidbody2D rbody;

	[SerializeField]
	private AnimationCurve accelerationCurve;

	private float accelerationTime;

	[SerializeField]
	private AnimationCurve decelerationCurve;

	private float decelerationTime;

	private AnimationCurve currentCurve;

	private float currentSpeed;

	private float curveTime;

	private float timestamp;

	private float dir;

	private bool moving = false;

	private float currSpeed;

	private float maxSpeed;

	private bool paused;

	private float pausedUntil;

	private float pauseStarted;

	private float storedVelocity;

	private float baseSpeed => speed + speedMod;
	private float speedMod;

	private void Awake()
	{
		accelerationTime = accelerationCurve[accelerationCurve.length - 1].time;
		decelerationTime = decelerationCurve[decelerationCurve.length - 1].time;
		maxSpeed = baseSpeed;
	}

	private void FixedUpdate()
	{
		if (!(Time.time < pausedUntil))
		{
			if (paused)
			{
				paused = false;
				rbody.linearVelocityX = storedVelocity;
				timestamp += pausedUntil - pauseStarted;
			}
			if (Time.time < timestamp)
			{
				rbody.linearVelocityX = currentSpeed * dir * currentCurve.Evaluate(Time.time - timestamp + curveTime);
			}
			else if (moving)
			{
				rbody.linearVelocityX = maxSpeed * dir;
			}
			else
			{
				rbody.linearVelocityX = 0f;
			}
			maxSpeed = baseSpeed;
		}
	}

	public void StartDeceleration()
	{
		moving = false;
		currentCurve = decelerationCurve;
		curveTime = decelerationTime;
		currentSpeed = baseSpeed;
		if (Mathf.Abs(rbody.linearVelocityX) < currentSpeed)
		{
			currentSpeed = Mathf.Abs(rbody.linearVelocityX);
		}
		timestamp = Time.time + curveTime;
	}

	public void StartAcceleration(float dir)
	{
		moving = true;
		currentCurve = accelerationCurve;
		curveTime = accelerationTime;
		currentSpeed = baseSpeed;
		timestamp = Time.time + curveTime;
	}

	public void UpdateMovement(float dir)
	{
		this.dir = dir;
		moving = true;
	}

	public void OverrideCurve(float speed, AnimationCurve curve, float dir)
	{
		moving = true;
		this.dir = dir;
		currentCurve = curve;
		curveTime = curve[curve.length - 1].time;
		currentSpeed = speed;
		timestamp = Time.time + curveTime;
	}

	public void ForceStop()
	{
		moving = false;
		timestamp = 0f;
		rbody.linearVelocityX = 0f;
	}

	public void OverrideSpeed(float speed)
	{
		currentSpeed = speed;
		maxSpeed = speed;
	}

	public float GetMaxSpeed()
	{
		return maxSpeed;
	}

	public void ResetCurves()
	{
		currentSpeed = baseSpeed;
	}

	public void Pause(float endPause)
	{
		if (paused)
		{
			if (endPause > pausedUntil)
			{
				pausedUntil = endPause;
			}
		}
		else
		{
			paused = true;
			pauseStarted = Time.time;
			pausedUntil = endPause;
			storedVelocity = rbody.linearVelocityX;
			rbody.linearVelocityX = 0f;
		}
	}

	public void AdjustBaseSpeed(float mod, float multiplier = 0) {
		if (multiplier != 0) {
			speedMod = (mod + speed) * multiplier - speed;
		} else {
			speedMod = mod;
		}
		maxSpeed = baseSpeed;
	}
}
