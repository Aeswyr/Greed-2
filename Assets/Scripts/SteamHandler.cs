using Mirror;
using Steamworks;
using UnityEngine;

public class SteamHandler : Singleton<SteamHandler>
{
	[SerializeField]
	private NetworkManager networkManager;

	private Callback<LobbyCreated_t> createCallback;

	private Callback<GameLobbyJoinRequested_t> joinCallback;

	private Callback<LobbyEnter_t> enterCallback;

	private CSteamID lobbyId;

	private bool initialized;

	public CSteamID LobbyID => lobbyId;

    public void OnInit()
	{
		createCallback = Callback<LobbyCreated_t>.Create(LobbyCreated);
		joinCallback = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoined);
		enterCallback = Callback<LobbyEnter_t>.Create(LobbyEntered);
		initialized = true;
		Debug.Log("Steam initialized successfully");
	}

	public void Host()
	{
		Debug.Log("trying lobby1");
		if (initialized)
		{
			Debug.Log("trying lobby2");
			SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
		}
	}

	private void LobbyCreated(LobbyCreated_t data)
	{
		if (data.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("hosting lobby");
			networkManager.StartHost();
			lobbyId = new CSteamID(data.m_ulSteamIDLobby);
			SteamMatchmaking.SetLobbyData(lobbyId, "CSID", SteamUser.GetSteamID().ToString());
		}
	}

	private void LobbyJoined(GameLobbyJoinRequested_t data)
	{
		SteamMatchmaking.JoinLobby(data.m_steamIDLobby);
	}

	private void LobbyEntered(LobbyEnter_t data)
	{
		if (!NetworkServer.active)
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(new CSteamID(data.m_ulSteamIDLobby), "CSID");
			networkManager.networkAddress = lobbyData;
			networkManager.StartClient();
		}
	}
}
