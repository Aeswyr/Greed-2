using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitInteractable : NetworkBehaviour
{
	[SerializeField]
	private TextMeshPro playerCounter;
	[SerializeField] private Image leaveTimer;
	private float remainingTime;
	private bool timerEnabled;
	private int baseTime = 15;

	[SyncVar(hook = nameof(UpdatePlayerCount))]
	private int playerCount;

	private void Start()
	{
		playerCounter.gameObject.SetActive(value: false);
		leaveTimer.transform.parent.parent.gameObject.SetActive(false);
		remainingTime = baseTime;
	}

    private void FixedUpdate()
    {
        if (timerEnabled)
		{
			remainingTime -= Time.fixedDeltaTime;
			leaveTimer.fillAmount = remainingTime / baseTime;

			if (timerEnabled && remainingTime <= 0 && isServer && playerCount < GameManager.Instance.TotalPlayerCount())
			{
				EvictPlayers();
				timerEnabled = false;
			}

			[ClientRpc] void EvictPlayers() {
				foreach (var player in GameManager.Instance.GetPlayers())
				{
					if (player.isLocalPlayer && !player.NextLevelReady)
						OnInteract(player);
				}
			}
		}
    }

    public void OnInteract(PlayerController owner)
	{
		owner.SetStasis(value: true);
		owner.LeaveLevel(playerCount == 0);
		Vector3 pos = new Vector3(transform.position.x, -200, 0);
		owner.transform.position = pos;
		NextLevelReady();
	}

	[Command(requiresAuthority = false)]
	private void NextLevelReady()
	{
		playerCount = playerCount + 1;
		EnableCountdown(Mathf.Min(baseTime / playerCount, remainingTime));
		
		if (playerCount >= GameManager.Instance.TotalPlayerCount())
		{
			SFXManager.Instance.PlaySound("descend");
			GameManager.Instance.GoNextLevel();
		}
	}

	[ClientRpc] private void EnableCountdown(float remainingTime)
	{
		this.remainingTime = remainingTime;
		timerEnabled = true;
		leaveTimer.transform.parent.parent.gameObject.SetActive(true);
	}

	private void UpdatePlayerCount(int oldValue, int newValue)
	{
		playerCounter.gameObject.SetActive(value: true);
		playerCounter.text = $"{newValue}/{GameManager.Instance.TotalPlayerCount()}";
	}
}
