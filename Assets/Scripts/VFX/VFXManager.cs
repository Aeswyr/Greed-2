using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class VFXManager : NetworkSingleton<VFXManager>
{
	[SerializeField] private Camera camera;
	[Header("Simple Particles")]
	[SerializeField]
	private GameObject template;

	[SerializeField]
	private AnimationClip[] anims;

	[Header("Complex Particles")]
	[SerializeField]
	private List<GameObject> particles;

	[Header("Floating Text")]
	[SerializeField]
	private GameObject textPrefab;

	[Header("Afterimage")]
	[SerializeField]
	private GameObject afterimagePrefab;

	private Vector3 cameraPos;

	void Start()
	{
		cameraPos = camera.transform.position;
	}

	public void SyncVFX(ParticleType type, Vector3 pos, bool flip, bool renderBehind = false)
	{
		if (isServer)
		{
			RecieveVFX(type, pos, flip, renderBehind);
		}
		else
		{
			SendVFX(type, pos, flip, renderBehind);
		}
	}

	[Command(requiresAuthority = false)]
	private void SendVFX(ParticleType type, Vector3 pos, bool flip, bool renderBehind)
	{
		RecieveVFX(type, pos, flip, renderBehind);
	}

	[ClientRpc]
	private void RecieveVFX(ParticleType type, Vector3 pos, bool flip, bool renderBehind)
	{
		CreateVFX(type, pos, flip, null, renderBehind);
	}

	public void CreateVFX(ParticleType type, Vector3 pos, bool flip, Transform parent = null, bool renderBehind = false)
	{
		if (type == ParticleType.NONE)
			return;

		GameObject gameObject;
		if (parent != null)
		{
			gameObject = Instantiate(template, parent);
			gameObject.transform.localPosition = pos;
		}
		else
		{
			gameObject = Instantiate(template, pos, Quaternion.identity);
		}
		Animator component = gameObject.GetComponent<Animator>();
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(component.runtimeAnimatorController);
		animatorOverrideController["particle"] = anims[(int)type];
		component.runtimeAnimatorController = animatorOverrideController;
		gameObject.GetComponent<DestroyAfterDelay>().Init(anims[(int)type].length - 0.1f);
		SpriteRenderer component2 = gameObject.GetComponent<SpriteRenderer>();
		component2.flipX = flip;
		if (renderBehind)
		{
			component2.sortingLayerName = "VFX_Back";
		}
	}

	public void SyncPrefabVFX(ParticlePrefabType type, Vector3 pos)
	{
		if (isServer)
		{
			RecievePrefabVFX(type, pos);
		}
		else
		{
			SendPrefabVFX(type, pos);
		}
	}

	[Command(requiresAuthority = false)]
	private void SendPrefabVFX(ParticlePrefabType type, Vector3 pos)
	{
		RecievePrefabVFX(type, pos);
	}

	[ClientRpc]
	private void RecievePrefabVFX(ParticlePrefabType type, Vector3 pos)
	{
		CreatePrefabVFX(type, pos);
	}

	public void CreatePrefabVFX(ParticlePrefabType type, Vector3 pos, Transform parent = null)
	{
		if (parent != null)
		{
			GameObject gameObject = Instantiate(particles[(int)type], parent);
			gameObject.transform.localPosition = pos;
		}
		else
		{
			GameObject gameObject = Instantiate(particles[(int)type], pos, Quaternion.identity);
			gameObject.transform.SetParent(GameManager.Instance.GetLevelObjectRoot());
		}
	}

	public void SyncFloatingText(string text, Vector3 pos, Color color)
	{
		if (isServer)
		{
			RecieveFloatingText(text, pos, color);
		}
		else
		{
			SendFloatingText(text, pos, color);
		}
	}

	[Command(requiresAuthority = false)]
	public void SendFloatingText(string text, Vector3 pos, Color color)
	{
		RecieveFloatingText(text, pos, color);
	}

	[ClientRpc]
	private void RecieveFloatingText(string text, Vector3 pos, Color color)
	{
		CreateFloatingText(text, pos, color);
	}

	public void CreateFloatingText(string text, Vector3 pos, Color color)
	{
		GameObject gameObject = Instantiate(textPrefab, pos, Quaternion.identity);
		FloatingTextController component = gameObject.GetComponent<FloatingTextController>();
		component.SetText(text, color);
	}

	public GameObject GetAfterimagePrefab()
	{
		return afterimagePrefab;
	}

	public void Screenshake(float intensity, float duration)
	{
		camera.transform.DOShakePosition(duration, intensity).onComplete += () =>
		{
			camera.transform.position = cameraPos;
		};
	}

	public void SyncScreenshake(float intensity, float duration)
	{
		if (isServer)
			RecieveScreenShake(intensity, duration);
		else
			SendScreenshake(intensity, duration);

		[Command(requiresAuthority = false)] void SendScreenshake(float intensity, float duration)
		{
			RecieveScreenShake(intensity, duration);
		}
		
		[ClientRpc] void RecieveScreenShake(float intensity, float duration)
		{
			Screenshake(intensity, duration);
		}
	}
}
