using Mirror;
using Mirror.RemoteCalls;
using TMPro;
using UnityEngine;

public class GameManager : NetworkSingleton<GameManager>
{
	[SerializeField]
	private GameObject hitboxPrefab;

	[SerializeField]
	private GameObject projectilePrefab;

	[SerializeField]
	private GameObject pickupPrefab;

	[Header("lobby stuff")]
	[SerializeField]
	private GameObject playerLobby;

	[SerializeField]
	private Transform playerLobbyHolder;

	[SerializeField]
	private GameObject lobbyCardPrefab;

	[Header("levels")]
	[SerializeField]
	private TextMeshProUGUI levelDisplay;

	[SerializeField]
	private LevelController currentLevel;

	[SerializeField]
	private GameObject[] levels;

	[SerializeField]
	private GameObject[] shops;

	private int levelCount = 0;

	public Transform PlayerLobbyHolder => playerLobbyHolder;

	private void Start()
	{
		LoadLevelObjects();
	}

	public int TotalPlayerCount()
	{
		return Object.FindObjectsOfType<PlayerController>().Length;
	}

	public void AddLobbyCard(PlayerController player, InputHandler input)
	{
		player.SetInputLocked(value: true);
		SyncCard(player.transform);
	}

	[Command(requiresAuthority = false)]
	private void SyncCard(Transform player)
	{
		GameObject gameObject = Object.Instantiate(lobbyCardPrefab, playerLobbyHolder);
		gameObject.GetComponent<LobbyCardController>().Init(player.GetComponent<PlayerController>());
		NetworkServer.Spawn(gameObject);
	}

	public void CheckLobbyReady()
	{
		LobbyCardController[] array = Object.FindObjectsOfType<LobbyCardController>();
		LobbyCardController[] array2 = array;
		foreach (LobbyCardController lobbyCardController in array2)
		{
			if (!lobbyCardController.IsReady())
			{
				return;
			}
		}
		LobbyCardController[] array3 = array;
		foreach (LobbyCardController lobbyCardController2 in array3)
		{
			lobbyCardController2.FinalizeReady();
		}
		SyncGameStart();
	}

	[ClientRpc]
	private void SyncGameStart()
	{
		playerLobby.SetActive(value: false);
		PlayerController[] array = Object.FindObjectsOfType<PlayerController>();
		foreach (PlayerController playerController in array)
		{
			if (playerController.isLocalPlayer)
			{
				playerController.SetInputLocked(value: false);
			}
		}
	}

	public GameObject GetHitbox(Vector3 position, Quaternion rotation, Transform parent)
	{
		GameObject gameObject = null;
		if (parent != null)
		{
			gameObject = Object.Instantiate(hitboxPrefab, parent);
			gameObject.transform.localPosition = position;
			gameObject.transform.localRotation = rotation;
		}
		else
		{
			gameObject = Object.Instantiate(hitboxPrefab, position, rotation);
		}
		return gameObject;
	}

	public void SpawnGoldBurst(Vector3 position, int amount, PickupVariant variant = PickupVariant.ALL)
	{
		while (amount > 0)
		{
			GameObject gameObject = Object.Instantiate(pickupPrefab, position + Vector3.up, Quaternion.identity);
			Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
			component.velocity = new Vector2(Random.Range(-15, 15), Random.Range(40, 60));
			if (amount > 10)
			{
				gameObject.GetComponent<PickupData>().Init(PickupType.MONEY_LARGE, variant);
				amount -= 10;
			}
			else
			{
				gameObject.GetComponent<PickupData>().Init(PickupType.MONEY_SMALL, variant);
				amount--;
			}
			NetworkServer.Spawn(gameObject);
		}
	}

	public void SpawnCrown(Vector3 position)
	{
		GameObject gameObject = Object.Instantiate(pickupPrefab, position + Vector3.up, Quaternion.identity);
		PickupData component = gameObject.GetComponent<PickupData>();
		component.Init(PickupType.ITEM_CROWN);
		component.SetFloaty(20f, new Vector2(Random.Range(-3, 3), Random.Range(8, 12)));
		NetworkServer.Spawn(gameObject);
	}

	public void GoNextLevel()
	{
		AuctionableInteractable[] array = Object.FindObjectsOfType<AuctionableInteractable>();
		foreach (AuctionableInteractable auctionableInteractable in array)
		{
			auctionableInteractable.RunAuctionComplete();
		}
		NextLevel(0);
		[ClientRpc]
		void NextLevel(int id)
		{
			Object.Destroy(currentLevel.gameObject);
			LoadLevel(id);
		}
	}

	private void LoadLevel(int id)
	{
		levelCount++;
		GameObject original = levels[id];
		if (IsLevelShop())
		{
			original = shops[id];
		}
		currentLevel = Object.Instantiate(original).GetComponent<LevelController>();
		PlayerController[] array = Object.FindObjectsOfType<PlayerController>();
		foreach (PlayerController playerController in array)
		{
			if (playerController.isLocalPlayer)
			{
				playerController.SetStasis(value: false);
				playerController.transform.position = Vector3.zero;
				playerController.EnterLevel();
				levelDisplay.text = levelCount.ToString("D2");
			}
		}
		LoadLevelObjects();
	}

	private void LoadLevelObjects()
	{
		LevelObjectSpawn[] array = Object.FindObjectsOfType<LevelObjectSpawn>();
		foreach (LevelObjectSpawn levelObjectSpawn in array)
		{
			if (base.isServer)
			{
				GameObject gameObject = Object.Instantiate(levelObjectSpawn.GetSpawn(), levelObjectSpawn.transform.position, levelObjectSpawn.transform.rotation);
				if (!string.IsNullOrEmpty(levelObjectSpawn.GetExtraData()))
				{
					string[] array2 = levelObjectSpawn.GetExtraData().Split(' ');
					string text = array2[0];
					string text2 = text;
					if (text2 == "shop")
					{
						gameObject.GetComponent<ShopInteractable>().SetupMerchandise(array2[1]);
					}
				}
				NetworkServer.Spawn(gameObject);
			}
			Object.Destroy(levelObjectSpawn.gameObject);
		}
	}

	public Transform GetLevelObjectRoot()
	{
		return currentLevel.transform.Find("LevelObjects");
	}

	public int GetLevelIndex()
	{
		return levelCount;
	}

	public bool IsLevelShop()
	{
		return levelCount % 3 == 0 && levelCount != 0;
	}

	public LevelController GetCurrentLevel()
	{
		return currentLevel;
	}

	[Command(requiresAuthority = false)]
	public void SpawnProjectile(ProjectileBuilder data)
	{
		NetworkServer.Spawn(data.Apply(projectilePrefab));
	}
}
