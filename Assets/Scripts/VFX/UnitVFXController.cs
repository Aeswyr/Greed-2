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
	private List<AfterimageData> activeImages = new();

	private void FixedUpdate()
	{
		for (int i = 0; i < activeImages.Count; i++) {
			var image = activeImages[i];
			if (Time.time > image.end) {
				activeImages.RemoveAt(i);
				i--;
			} else if (Time.time > image.next) {
				CreateAfterimage(image.duration);
				image.next = Time.time + image.delay;

				activeImages[i] = image;
			}
		}
	}

	public void StartAfterImageChain(float duration, float imageDelay, float imageDuration = 0.5f)
	{
		if (isServer) {
			RecieveAfterImage(duration, imageDelay, imageDuration);
		} else {
			SendAfterImage(duration, imageDelay, imageDuration);
		}

		[Command] void SendAfterImage(float duration, float imageDelay, float imageDuration) {
			RecieveAfterImage(duration, imageDelay, imageDuration);
		}
		
		[ClientRpc] void RecieveAfterImage(float duration, float imageDelay, float imageDuration) {
			activeImages.Add(new (){
				end = Time.time + duration,
				next = Time.time + imageDelay,
				delay = imageDelay,
				duration = imageDuration
			});
		}
	}

	public void SyncAfterimage(float duration)
	{
		if (isServer)
		{
			RecieveAfterimage(duration);
		}
		else
		{
			SendAfterimage(duration);
		}
			
		[Command] void SendAfterimage(float duration)
		{
			RecieveAfterimage(duration);
		}

		[ClientRpc] void RecieveAfterimage(float duration)
		{
			CreateAfterimage(duration);
		}
	}

	private void CreateAfterimage(float duration) {
		GameObject gameObject = Instantiate(VFXManager.Instance.GetAfterimagePrefab(), transform.position, Quaternion.identity);
		SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
		SpriteRenderer spriteRenderer = sprite;
		component.sprite = spriteRenderer.sprite;
		component.material = spriteRenderer.material;
		component.flipX = spriteRenderer.flipX;
		gameObject.GetComponent<DestroyAfterDelay>().Init(duration);
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

	private struct AfterimageData {
		public float end;
		public float next;
		public float duration;
		public float delay;
	}
}
