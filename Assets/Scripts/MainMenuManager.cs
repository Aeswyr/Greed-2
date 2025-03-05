using HeathenEngineering.SteamworksIntegration;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
	[SerializeField]
	private GameObject joinMenu;
	[SerializeField]
	private GameObject mainMenu;

	[SerializeField]
	private GameObject joinPrefab;

	[SerializeField]
	private Transform joinParent;

	[SerializeField]
	private GameObject noLobbies;

	[SerializeField]
	private Button playButton;

	[SerializeField]
	private GameObject networkPrefab;
	[SerializeField]
	private NetworkManager networkManager;
	[SerializeField]
	private PlayerInputManager playerInput;

	private void Start()
	{
		playerInput.onPlayerJoined += FirstPlayerJoined;

		if (FindObjectOfType<NetworkAnimator>() == null)
		{
			Instantiate(networkPrefab);
		}
		GetFriendLobbies();

		mainMenu.SetActive(true);
		joinMenu.SetActive(false);
	}

	private void FirstPlayerJoined(PlayerInput player) {
		playerInput.DisableJoining();
		playerInput.onPlayerJoined -= FirstPlayerJoined;
	}

	public void RefreshLobbies()
	{
		for (int i = 0; i < joinParent.childCount; i++)
		{
			Destroy(joinParent.GetChild(i).gameObject);
		}
		GetFriendLobbies();
	}

	private void GetFriendLobbies()
	{
		int num = 0;
		Debug.Log($"friends {SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate)}");
		for (int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++)
		{
			CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			if (SteamFriends.GetFriendGamePlayed(friendByIndex, out var pFriendGameInfo) && pFriendGameInfo.m_steamIDLobby.IsValid() && pFriendGameInfo.m_gameID.AppID() == SteamSettings.ApplicationId)
			{
				Debug.Log("attempting to add lobby");
				AddJoinLobby(pFriendGameInfo.m_steamIDLobby, SteamFriends.GetFriendPersonaName(friendByIndex) + "'s Lobby");
				num++;
			}
		}
		if (num == 0)
		{
			noLobbies.SetActive(value: true);
		}
		else
		{
			noLobbies.SetActive(value: false);
		}
	}

	private void AddJoinLobby(CSteamID lobbyID, string nameOverride = null)
	{
		string text = SteamMatchmaking.GetLobbyData(lobbyID, "name");
		if (nameOverride != null)
		{
			text = nameOverride;
		}
		GameObject gameObject = Instantiate(joinPrefab, joinParent);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
		gameObject.GetComponent<Button>().onClick.AddListener(JoinLobby);
		void JoinLobby()
		{
			SteamMatchmaking.JoinLobby(lobbyID);
		}
	}

	public void OnQuit()
	{
		Application.Quit();
	}

	public void OnLocal() {
		playerInput.EnableJoining();

		networkManager.maxConnections = 0;
		networkManager.StartHost();
	}
	public void OnHost()
	{
		FindObjectOfType<SteamManager>().Host();
	}

	public void OnJoin() {
		mainMenu.SetActive(false);
		joinMenu.SetActive(true);
	}

	public void OnBack() {
		mainMenu.SetActive(true);
		joinMenu.SetActive(false);
	}
}
