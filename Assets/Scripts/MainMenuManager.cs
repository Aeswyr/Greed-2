using HeathenEngineering.SteamworksIntegration;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
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

	private void Start()
	{
		if (Object.FindObjectOfType<NetworkAnimator>() == null)
		{
			Object.Instantiate(networkPrefab);
		}
		GetFriendLobbies();
	}

	public void RefreshLobbies()
	{
		for (int i = 0; i < joinParent.childCount; i++)
		{
			Object.Destroy(joinParent.GetChild(i).gameObject);
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
		GameObject gameObject = Object.Instantiate(joinPrefab, joinParent);
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

	public void OnHost()
	{
		Object.FindObjectOfType<SteamManager>().Host();
	}
}
