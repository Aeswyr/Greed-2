using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

public class ExitInteractable : NetworkBehaviour
{
	[SerializeField]
	private TextMeshPro playerCounter;

	[SyncVar(hook = nameof(UpdatePlayerCount))]
	private int playerCount;

	private void Start()
	{
		playerCounter.gameObject.SetActive(value: false);
	}

	public void OnInteract(PlayerController owner)
	{
		owner.SetStasis(value: true);
		owner.LeaveLevel(playerCount == 0);
		owner.transform.position = 200f * Vector3.down;
		NextLevelReady();
	}

	[Command(requiresAuthority = false)]
	private void NextLevelReady()
	{
		playerCount = playerCount + 1;
		if (playerCount >= GameManager.Instance.TotalPlayerCount())
		{
			SFXManager.Instance.PlaySound("descend");
			GameManager.Instance.GoNextLevel();
		}
	}

	private void UpdatePlayerCount(int oldValue, int newValue)
	{
		playerCounter.gameObject.SetActive(value: true);
		playerCounter.text = $"{newValue}/{GameManager.Instance.TotalPlayerCount()}";
	}
}
