using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCardController : NetworkBehaviour
{
	[SerializeField]
	private TextMeshProUGUI playerName;

	[SerializeField]
	private GameObject readyText;

	[SerializeField]
	private Image characterDisplay;

	[SerializeField]
	private MaterialLibrary colors;

	[SyncVar(hook = nameof(UpdateDisplay))]
	private int currentColor;

	[SyncVar(hook = nameof(UpdateReady))]
	private bool ready;

	[SyncVar]
	private PlayerController player;

	[SyncVar]
	private ulong? friendId = null;

	private InputHandler input;

	public void Init(PlayerController player)
	{
		this.player = player;
	}

	private void Start()
	{
		if (transform.parent != GameManager.Instance.PlayerLobbyHolder)
		{
			transform.SetParent(GameManager.Instance.PlayerLobbyHolder);
		}
		if (input == null) {
			input = FindAnyObjectByType<InputHandler>();
		}
		if (player != null && player.isLocalPlayer)
		{
			SyncCardName(SteamUser.GetSteamID().m_SteamID);
		}
		else if (friendId.HasValue)
		{
			playerName.text = Utils.GetSteamName(friendId.Value);
		}
	}

	[Command(requiresAuthority = false)]
	private void SyncCardName(ulong id)
	{
		SendCardName(id);
		friendId = id;
		[ClientRpc]
		void SendCardName(ulong id)
		{
			playerName.text = Utils.GetSteamName(id);
		}
	}

	private void FixedUpdate()
	{
		if (!(player == null) && player.isLocalPlayer)
		{
			if (input.move.pressed && input.dir != 0f)
			{
				int color = (currentColor + (int)input.dir + ((currentColor == 0) ? colors.Length : 0)) % colors.Length;
				SetColor(color);
			}
			if (input.jump.pressed)
			{
				ToggleReady();
			}
		}
	}

	[Command(requiresAuthority = false)]
	private void SetColor(int color)
	{
		currentColor = color;
	}

	private void UpdateDisplay(int oldIndex, int newIndex)
	{
		Material uIColor = colors.GetUIColor(newIndex);
		characterDisplay.material = ((uIColor == null) ? colors[newIndex] : uIColor);
	}

	[Command(requiresAuthority = false)]
	private void ToggleReady()
	{
		ready = !ready;
		if (ready)
		{
			GameManager.Instance.CheckLobbyReady();
		}
	}

	private void UpdateReady(bool oldValue, bool newValue)
	{
		readyText.SetActive(newValue);
	}

	public bool IsReady()
	{
		return ready;
	}

	[ClientRpc]
	public void FinalizeReady()
	{
		if (!(player == null) && player.isLocalPlayer)
		{
			player.SetColor(currentColor);
		}
	}
}
