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

	[SerializeField] private Animator[] pickupFX;

	private float afterImageTimer;

	private float afterImageDelay;

	private float nextImage;
	private List<AfterimageData> activeImages = new();

	private void FixedUpdate()
	{
		for (int i = 0; i < activeImages.Count; i++)
		{
			var image = activeImages[i];
			if (Time.time > image.end)
			{
				activeImages.RemoveAt(i);
				i--;
			}
			else if (Time.time > image.next)
			{
				CreateAfterimage(image);
				image.next = Time.time + image.delay;

				activeImages[i] = image;
			}
		}
	}

	public void StartAfterImageChain(float duration, float imageDelay, float imageDuration = 0.5f, bool overrideMaterial = true, Color color = default, string tag = null)
	{
		if (color == default)
		{
			color = Color.white;
		}

		AfterimageData data = new()
		{
			end = Time.time + duration,
			next = Time.time + imageDelay,
			delay = imageDelay,
			duration = imageDuration,
			materialOverride = overrideMaterial,
			color = color,
			tag = tag
		};

		if (isServer)
		{
			RecieveAfterImage(data);
		}
		else
		{
			SendAfterImage(data);
		}

		[Command] void SendAfterImage(AfterimageData data)
		{
			RecieveAfterImage(data);
		}

		[ClientRpc] void RecieveAfterImage(AfterimageData data)
		{
			activeImages.Add(data);
		}
	}

	public void SyncAfterimage(float duration, float imageDelay, float imageDuration = 0.5f, bool overrideMaterial = true, Color color = default)
	{
		if (color == default)
		{
			color = Color.white;
		}

		AfterimageData data = new()
		{
			end = Time.time + duration,
			next = Time.time + imageDelay,
			delay = imageDelay,
			duration = imageDuration,
			materialOverride = overrideMaterial,
			color = color,
			tag = null
		};

		if (isServer)
		{
			RecieveAfterimage(data);
		}
		else
		{
			SendAfterimage(data);
		}

		[Command] void SendAfterimage(AfterimageData data)
		{
			RecieveAfterimage(data);
		}

		[ClientRpc] void RecieveAfterimage(AfterimageData data)
		{
			CreateAfterimage(data);
		}
	}

	private void CreateAfterimage(AfterimageData data)
	{
		GameObject gameObject = Instantiate(VFXManager.Instance.GetAfterimagePrefab(), transform.position, Quaternion.identity);
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		SpriteRenderer spriteRenderer = sprite;
		component.sprite = spriteRenderer.sprite;
		if (data.materialOverride)
			component.material = spriteRenderer.material;
		else
			component.color = data.color;
		component.flipX = spriteRenderer.flipX;
		gameObject.GetComponent<DestroyAfterDelay>().Init(data.duration);
		gameObject.GetComponent<AlphaDecay>().SetDuration(data.duration);
	}

	public void EndChain(string tag)
	{
		if (isServer)
		{
			RecieveTag(tag);
		}
		else
		{
			SendTag(tag);
		}
		[Command] void SendTag(string tag)
		{
			RecieveTag(tag);
		}

		[ClientRpc] void RecieveTag(string tag)
		{
			for (int i = 0; i < activeImages.Count; i++)
			{
				if (activeImages[i].tag == tag)
				{
					activeImages.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public void SetFXState(PlayerVFX fx, bool state)
	{
		if (isServer)
		{
			RecieveFXState(fx, state);
		}
		else
		{
			SendFXState(fx, state);
		}
	}

	public void PlayAnimationFX(AnimationVFX fx)
	{
		if (isServer)
			RecieveFX(fx);
		else
			SendFX(fx);

		[Command] void SendFX(AnimationVFX fx)
		{
			RecieveFX(fx);
		}
		[ClientRpc] void RecieveFX(AnimationVFX fx)
		{
			switch (fx)
			{
				case AnimationVFX.PICKUP_BLOODLUST:
					pickupFX[0].GetComponent<SpriteRenderer>().color = Color.red;
					pickupFX[1].GetComponent<SpriteRenderer>().color = Color.red;
					pickupFX[1].Play(pickupFX[1].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					pickupFX[0].Play(pickupFX[0].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					break;
				case AnimationVFX.PICKUP_SWIFT:
					pickupFX[0].GetComponent<SpriteRenderer>().color = Color.yellow;
					pickupFX[1].GetComponent<SpriteRenderer>().color = Color.yellow;
					pickupFX[1].Play(pickupFX[1].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					pickupFX[0].Play(pickupFX[0].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					break;
				case AnimationVFX.PICKUP_GHOSTFORM:
					pickupFX[0].GetComponent<SpriteRenderer>().color = Color.cyan;
					pickupFX[1].GetComponent<SpriteRenderer>().color = Color.cyan;
					pickupFX[1].Play(pickupFX[1].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					pickupFX[0].Play(pickupFX[0].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					break;
				case AnimationVFX.PICKUP_GREED:
					pickupFX[0].GetComponent<SpriteRenderer>().color = Color.green;
					pickupFX[1].GetComponent<SpriteRenderer>().color = Color.green;
					pickupFX[1].Play(pickupFX[1].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					pickupFX[0].Play(pickupFX[0].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					break;
				case AnimationVFX.PICKUP_BARRIER:
					pickupFX[0].GetComponent<SpriteRenderer>().color = Color.gray;
					pickupFX[1].GetComponent<SpriteRenderer>().color = Color.gray;
					pickupFX[1].Play(pickupFX[1].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					pickupFX[0].Play(pickupFX[0].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					break;
				case AnimationVFX.PICKUP_GENERIC:
					pickupFX[0].GetComponent<SpriteRenderer>().color = Color.white;
					pickupFX[1].GetComponent<SpriteRenderer>().color = Color.white;
					pickupFX[1].Play(pickupFX[1].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					pickupFX[0].Play(pickupFX[0].GetCurrentAnimatorStateInfo(-1).fullPathHash);
					break;
			}
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

	public void ClearParticles()
	{
		SendClear();

		[Command(requiresAuthority = false)] void SendClear()
		{
			RecieveClear();
		}

		[ClientRpc] void RecieveClear()
		{
			foreach (var particle in playerParticles)
			{
				particle.Clear();
			}
		}
	}
}
	public struct AfterimageData {
		public float end;
		public float next;
		public float duration;
		public float delay;
		public bool materialOverride;
		public Color color;
		public string tag;

		public void Write(NetworkWriter writer)
		{
			writer.Write(end);
			writer.Write(next);
			writer.Write(duration);
			writer.Write(delay);
			writer.Write(materialOverride);
			writer.Write(color);
			writer.Write(tag);
		}

		public AfterimageData Read(NetworkReader reader)
		{
			end = reader.Read<float>();
			next = reader.Read<float>();
			duration = reader.Read<float>();
			delay = reader.Read<float>();
			materialOverride = reader.Read<bool>();
			color = reader.Read<Color>();
			tag = reader.Read<string>();
			return this;
		}
	}