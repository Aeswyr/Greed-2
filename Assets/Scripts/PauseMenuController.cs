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
		input = Object.FindObjectOfType<InputHandler>();
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
			Object.FindObjectOfType<NetworkManager>().StopHost();
		}
		else
		{
			Object.FindObjectOfType<NetworkManager>().StopClient();
		}
	}

	public void Quit()
	{
		Application.Quit();
	}
}
