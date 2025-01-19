using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PickupData : NetworkBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private SpriteLibrary icons;

	[SerializeField]
	private Rigidbody2D rbody;

	[SerializeField]
	private AnimationCurve motionCurve;

	[SerializeField]
	private ParticleSystem dramaticParticles;

	[SyncVar]
	private PickupType type;

	[SyncVar]
	private PickupVariant variant;

	private float lockout;

	private bool floaty;

	private float startFloat;

	private Vector2 baseVelocity;

	public void Init(PickupType type, PickupVariant variant = PickupVariant.ALL)
	{
		this.type = type;
		this.variant = variant;
	}

	public void SetFloaty(float velocity, Vector2 dir)
	{
		rbody.gravityScale = 0f;
		floaty = true;
		startFloat = Time.time;
		baseVelocity = velocity * dir.normalized;
	}

	private void Awake()
	{
		lockout = Time.time + 0.5f;
	}

	private void Start()
	{
		UpdateSprite();
		if (type == PickupType.ITEM_CROWN)
		{
			dramaticParticles.Play();
		}
	}

	private void FixedUpdate()
	{
		if (floaty && Time.time > startFloat)
		{
			float num = Time.time - startFloat;
			if (num < motionCurve[motionCurve.length - 1].time)
			{
				rbody.velocity = motionCurve.Evaluate(num) * baseVelocity;
				return;
			}
			floaty = false;
			rbody.velocity = Vector2.zero;
			rbody.gravityScale = 0.1f;
		}
	}

	private void UpdateSprite()
	{
		if (type == PickupType.MONEY_SMALL || type == PickupType.MONEY_LARGE)
		{
			if (variant == PickupVariant.ALL)
			{
				this.variant = (PickupVariant)Random.Range(0, 3);
			}
			int i = (int)(3 * (int)type + variant);
			sprite.sprite = icons[i];
		}
		else
		{
			sprite.sprite = icons[(int)(type + 4)];
		}
	}

	public bool CanPickup()
	{
		return Time.time > lockout;
	}

	public void OnPickup()
	{
		Cleanup();
		[Command(requiresAuthority = false)]
		void Cleanup()
		{
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public PickupType GetPickupType()
	{
		return type;
	}

	public PickupVariant GetPickupVariant()
	{
		return variant;
	}

}
