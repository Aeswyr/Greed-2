using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class UnitVFXController : NetworkBehaviour
{
	[SerializeField]
	private SpriteRenderer sprite;

	[SerializeField]
	private List<ParticleSystem> playerParticles;

	[SerializeField]
	private ParticleSystem[] objectPullers;

	private float afterImageTimer;

	private float afterImageDelay;

	private float nextImage;

	private void FixedUpdate()
	{
		if (Time.time < afterImageTimer && Time.time > nextImage)
		{
			SyncAfterimage(0.5f);
			nextImage = Time.time + afterImageDelay;
		}
	}

	public void StartAfterImageChain(float duration, float delay)
	{
		afterImageTimer = Time.time + duration;
		afterImageDelay = delay;
		nextImage = Time.time + delay;
	}

	public void SyncAfterimage(float duration)
	{
		if (base.isServer)
		{
			RecieveAfterimage(duration);
		}
		else
		{
			SendAfterimage(duration);
		}
	}

	[Command]
	private void SendAfterimage(float duration)
	{
		RecieveAfterimage(duration);
	}

	[ClientRpc]
	private void RecieveAfterimage(float duration)
	{
		GameObject gameObject = Object.Instantiate(VFXManager.Instance.GetAfterimagePrefab(), base.transform.position, Quaternion.identity);
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		SpriteRenderer spriteRenderer = sprite;
		component.sprite = spriteRenderer.sprite;
		component.material = spriteRenderer.material;
		component.flipX = spriteRenderer.flipX;
		gameObject.GetComponent<DestroyAfterDelay>().Init(duration);
	}

	public void SetFXState(PlayerVFX fx, bool state)
	{
		if (base.isServer)
		{
			RecieveFXState(fx, state);
		}
		else
		{
			SendFXState(fx, state);
		}
	}

	public void TriggerPickupFX(List<Vector3> positions, int type)
	{
		if (positions.Count > 0)
		{
			SendPickupTrigger(positions, type);
		}
		[Command(requiresAuthority = false)]
		void SendPickupTrigger(List<Vector3> positions, int type)
		{
			RecievePickupTrigger(positions, type);
			[ClientRpc]
			void RecievePickupTrigger(List<Vector3> positions, int type)
			{
				objectPullers[type].Clear();
				objectPullers[type].Emit(positions.Count);
				ParticleSystem.Particle[] array = new ParticleSystem.Particle[positions.Count];
				int particles = objectPullers[type].GetParticles(array);
				for (int i = 0; i < particles; i++)
				{
					array[i].position = positions[i];
				}
				objectPullers[type].SetParticles(array);
			}
		}
	}

	[Command]
	private void SendFXState(PlayerVFX fx, bool state)
	{
		RecieveFXState(fx, state);
	}

	[ClientRpc]
	private void RecieveFXState(PlayerVFX fx, bool state)
	{
		if (state)
		{
			playerParticles[(int)fx].Play();
		}
		else
		{
			playerParticles[(int)fx].Stop();
		}
	}
}
