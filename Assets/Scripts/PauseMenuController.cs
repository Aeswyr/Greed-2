using Mirror;
using Steamworks;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
	private InputHandler input;

	[SerializeField]
	private GameObject menuParent;

	private void Start()
	{
		input = FindAnyObjectByType<InputHandler>();
		menuParent.SetActive(value: false);
	}

	private void FixedUpdate()
	{
		if (input.menu.pressed)
		{
			menuParent.SetActive(!menuParent.activeSelf);
		}
	}

	public void ReturnToMenu()
	{
		SteamMatchmaking.LeaveLobby(Singleton<SteamManager>.Instance.LobbyID);
		if (NetworkServer.activeHost)
		{
			FindAnyObjectByType<NetworkManager>().StopHost();
		}
		else
		{
			FindAnyObjectByType<NetworkManager>().StopClient();
		}
	}

	public void Quit()
	{
		Application.Quit();
	}
}
