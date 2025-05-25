using System;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LobbyCardController : NetworkBehaviour
{
	[SerializeField]
	private TextMeshProUGUI playerName;

	[SerializeField]
	private GameObject readyText;

	[SerializeField]
	private Image characterDisplay;
	[SerializeField] private TextMeshProUGUI profileName;
	[SerializeField] private GameObject playerProfile;

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
	private int controlScheme = 0;

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
		if (input == null)
		{
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

		if (player != null && (GameManager.Instance.IsLocalGame || player.isLocalPlayer))
		{
			NextControlScheme(0);
			playerProfile.SetActive(true);
		}
		else
		{
			playerProfile.SetActive(false);
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

	public void NextControlScheme(int dir)
	{
		
		controlScheme += dir;

		var profiles = SaveDataManager.GetProfiles();

		if (controlScheme < 0)
		{
			controlScheme += profiles.Count + 1;
		}
		else
		{
			controlScheme %= profiles.Count + 1;
		}

		if (controlScheme == 0)
		{
			input.GetComponent<PlayerInput>().actions.RemoveAllBindingOverrides();
			profileName.text = "Default Profile";
		}
		else
		{
			var profile = profiles[controlScheme - 1];
			if (string.IsNullOrEmpty(profile.settings))
			{
				input.GetComponent<PlayerInput>().actions.RemoveAllBindingOverrides();
			}
			else
			{
				input.GetComponent<PlayerInput>().actions.LoadBindingOverridesFromJson(profile.settings);
			}
			profileName.text = profile.name;
		}
	}
}
