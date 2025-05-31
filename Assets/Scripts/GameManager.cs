using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

public class GameManager : NetworkSingleton<GameManager>
{
	[SerializeField]
	private GameObject playerPrefab;
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
	[SerializeField] private int levelCount;
	[SerializeField] private int majorLevelModulo;

	[SerializeField]
	private LevelController currentLevel;

	[SerializeField]
	private GameObject[] levels;

	[SerializeField]
	private GameObject[] shops;
	[SerializeField]
	private GameObject[] finalLevels;
	[Header("UI")]
	[SerializeField] private Animator screenWipe;
	[SerializeField] private VictoryScreenController victoryScreen;
	[SerializeField] private GameObject scoreCardPrefab;
	[SerializeField] private Transform scoreCardParent;
	[SerializeField] private GameObject levelTickPrefab;
	[SerializeField] private GameObject majorTickPrefab;
	[SerializeField] private Transform levelCounterParent;
	[SerializeField] private GameObject levelPointer;


	private int levelIndex = 0;

	private int readyPings;

	public Transform PlayerLobbyHolder => playerLobbyHolder;

	public bool IsLocalGame
	{
		get;
		private set;
	}

	private void Start()
	{
		victoryScreen.gameObject.SetActive(false);
		if (PlayerInputManager.instance != null)
		{
			IsLocalGame = true;
			PlayerInputManager.instance.onPlayerJoined += AddLocalPlayer;
		}

		LoadLevelObjects();
	}

	public int TotalPlayerCount()
	{
		return FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length;
	}

	public void AddLocalPlayer(PlayerInput input)
	{
		Debug.Log("Creating new player");

		var playerObj = Instantiate(playerPrefab);
		NetworkServer.Spawn(playerObj);
		playerObj.GetComponent<PlayerController>().SetupInput(input.GetComponent<InputHandler>());
	}

	public void AddLobbyCard(PlayerController player)
	{
		player.SetInputLocked(value: true);
		SyncCard(player.transform);
	}

	[Command(requiresAuthority = false)]
	private void SyncCard(Transform player)
	{
		GameObject gameObject = Instantiate(lobbyCardPrefab, playerLobbyHolder);
		gameObject.GetComponent<LobbyCardController>().Init(player.GetComponent<PlayerController>());
		NetworkServer.Spawn(gameObject);
	}

	public void CheckLobbyReady()
	{
		LobbyCardController[] array = FindObjectsByType<LobbyCardController>(FindObjectsSortMode.None);
		foreach (LobbyCardController lobbyCardController in array)
		{
			if (!lobbyCardController.IsReady())
			{
				return;
			}
		}
		foreach (LobbyCardController lobbyCardController in array)
		{
			lobbyCardController.FinalizeReady();
		}

		AssignPlayerIds();

		SyncGameStart();
	}

	private void AssignPlayerIds()
	{
		PlayerController[] array = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
		int id = 0;
		foreach (PlayerController playerController in array)
		{
			playerController.AssignId(id);
			id++;
		}
	}

	[ClientRpc]
	private void SyncGameStart()
	{
		if (IsLocalGame)
		{
			FindAnyObjectByType<PlayerInputManager>().DisableJoining();
		}

		HandlePlayerSpawns();

		playerLobby.SetActive(value: false);
		PlayerController[] array = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
		foreach (PlayerController playerController in array)
		{
			if (playerController.isLocalPlayer)
			{
				playerController.SetInputLocked(value: false);

			}
			var card = Instantiate(scoreCardPrefab, scoreCardParent);
			playerController.SetupScorecard(card.GetComponent<ScoreCard>());
		}

		for (int i = 0; i <= levelCount; i++)
		{
			if (i % majorLevelModulo == 0)
				Instantiate(majorTickPrefab, levelCounterParent);
			else
				Instantiate(levelTickPrefab, levelCounterParent);
		}

		CompleteLevelTransition();
	}

	public GameObject GetHitbox(Vector3 position, Quaternion rotation, Transform parent)
	{
		GameObject gameObject = null;
		if (parent != null)
		{
			gameObject = Instantiate(hitboxPrefab, parent);
			gameObject.transform.localPosition = position;
			gameObject.transform.localRotation = rotation;
		}
		else
		{
			gameObject = Instantiate(hitboxPrefab, position, rotation);
		}
		return gameObject;
	}

	public void SpawnGoldBurst(Vector3 position, int amount, PickupVariant variant = PickupVariant.ALL)
	{
		while (amount > 0)
		{
			GameObject gameObject = Instantiate(pickupPrefab, position + Vector3.up, Quaternion.identity);
			Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
			component.linearVelocity = new Vector2(Random.Range(-15, 15), Random.Range(40, 60));
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

	public void SpawnGemBurst(Vector3 position, int amount)
	{
		while (amount > 0)
		{
			GameObject gameObject = Instantiate(pickupPrefab, position + Vector3.up, Quaternion.identity);
			Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
			component.linearVelocity = new Vector2(Random.Range(-15, 15), Random.Range(40, 60));

			gameObject.GetComponent<PickupData>().Init(PickupType.MONEY_BONUS, PickupVariant.ALL);
			amount--;

			NetworkServer.Spawn(gameObject);
		}
	}

	public void SpawnCrown(Vector3 position)
	{
		GameObject gameObject = Instantiate(pickupPrefab, position + Vector3.up, Quaternion.identity);
		PickupData component = gameObject.GetComponent<PickupData>();
		component.Init(PickupType.ITEM_CROWN);
		component.SetFloaty(20f, new Vector2(Random.Range(-3, 3), Random.Range(8, 12)));
		NetworkServer.Spawn(gameObject);
	}

	public void GoNextLevel()
	{
		AuctionableInteractable[] array = FindObjectsByType<AuctionableInteractable>(FindObjectsSortMode.None);
		foreach (AuctionableInteractable auctionableInteractable in array)
		{
			auctionableInteractable.RunAuctionComplete();
		}

		NextLevel();

		[ClientRpc]
		void NextLevel()
		{
			StartCoroutine(StartLevelLoadSequence());
		}
	}



	private void LoadLevel(int id)
	{
		levelIndex++;
		GameObject original = levels[id];
		if (IsLevelShop())
		{
			original = shops[id];
		}
		if (levelIndex == levelCount)
		{
			original = finalLevels[id];
		}

		currentLevel = Instantiate(original).GetComponent<LevelController>();

		LoadLevelObjects();

		HandlePlayerSpawns();
	}

	private void HandlePlayerSpawns()
	{
		PlayerController[] array = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
		List<int> playerIds = new();

		foreach (PlayerController playerController in array)
		{
			StartCoroutine(SetupLevel(playerIds, playerController));
		}

		for (int i = 0; i < currentLevel.SpawnPoints.Count; i++)
		{
			if (!playerIds.Contains(i))
				currentLevel.SpawnPoints[i].SetActive(false);

		}
	}

	private IEnumerator SetupLevel(List<int> playerIds, PlayerController playerController)
	{
		playerIds.Add(playerController.PlayerID);
		if (playerController.isLocalPlayer)
		{
			yield return new WaitUntil(() => playerController.PlayerID != -1);

			playerController.transform.position = currentLevel.SpawnPoints[playerController.PlayerID].transform.position + 2 * Vector3.up;
			playerController.EnterLevel();
			playerController.PingNameplate();

			UpdateLevelUI();
		}
	}

	private void UpdateLevelUI()
	{
		Vector3 targetPos = levelCounterParent.GetChild(levelIndex).transform.position;
		levelPointer.transform.position = targetPos + 1.25f * Vector3.up;
	}

	public void CompleteLevelTransition()
	{
		PlayerController[] array = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

		foreach (PlayerController playerController in array)
			if (playerController.isLocalPlayer)
				playerController.SetStasis(value: false);

	}

	private void LoadLevelObjects()
	{
		LevelObjectSpawn[] array = FindObjectsByType<LevelObjectSpawn>(FindObjectsSortMode.None);
		foreach (LevelObjectSpawn levelObjectSpawn in array)
		{
			if (isServer)
			{
				if (levelObjectSpawn.ShouldCancelSpawn())
				{
					Destroy(levelObjectSpawn.gameObject);
					continue;
				}

				GameObject gameObject = Instantiate(levelObjectSpawn.GetSpawn(), levelObjectSpawn.transform.position, levelObjectSpawn.transform.rotation);
				if (!string.IsNullOrEmpty(levelObjectSpawn.GetExtraData()))
				{
					string[] commands = levelObjectSpawn.GetExtraData().Split(',');
					foreach (var command in commands)
					{
						ParseCommand(command.Split(' '));
					}
				}

				void ParseCommand(string[] components)
				{
					switch (components[0])
					{
						case "shop":
							gameObject.GetComponent<ShopInteractable>().SetupMerchandise(components[1]);
							break;
						case "index":
							if (levelObjectSpawn.GetSpawnedIndex() == int.Parse(components[1]))
							{
								string[] command = new string[components.Length - 2];
								for (int i = 0; i < command.Length; i++)
									command[i] = components[i + 2];
								ParseCommand(command);
							}
							break;
						case "flip":
							gameObject.GetComponent<SyncSpriteFlip>().SetFlipX(true);
							break;
					}
				}

				NetworkServer.Spawn(gameObject);
			}
			Destroy(levelObjectSpawn.gameObject);
		}
	}

	public Transform GetLevelObjectRoot()
	{
		return currentLevel.transform.Find("LevelObjects");
	}

	public int GetLevelIndex()
	{
		return levelIndex;
	}

	public bool IsLevelShop()
	{
		return levelIndex % majorLevelModulo == 0 && levelIndex != 0 && levelIndex != levelCount;
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

	private IEnumerator StartLevelLoadSequence()
	{
		screenWipe.SetTrigger("wipeon");

		yield return new WaitForSeconds(0.25f);

		Destroy(currentLevel.gameObject);
		ToolTipManager.Instance.ClearTooltips();

		NotifyReadyToLoad();
	}

	[Command(requiresAuthority = false)]
	private void NotifyReadyToLoad()
	{
		readyPings++;

		if (readyPings >= TotalPlayerCount() || IsLocalGame)
		{
			readyPings = 0;

			int nextLevelId = 0;
			FinishLoad(nextLevelId);
		}
	}

	[ClientRpc]
	private void FinishLoad(int id)
	{
		LoadLevel(id);

		StartCoroutine(FinishLevelLoadSequence());
	}

	private IEnumerator FinishLevelLoadSequence()
	{
		yield return new WaitForSeconds(0.25f);

		screenWipe.SetTrigger("wipeoff");
		yield return new WaitForSeconds(0.25f);

		CompleteLevelTransition();
	}

	public int GetShopRanking(PlayerController player)
	{
		List<PlayerController> players = new List<PlayerController>(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));

		players.Sort(delegate (PlayerController a, PlayerController b)
		{
			if (a.GetVictoryStats().MoneyHeld > b.GetVictoryStats().MoneyHeld)
				return -1;
			else if (a.GetVictoryStats().MoneyHeld < b.GetVictoryStats().MoneyHeld)
				return 1;
			else
				return 0;
		});

		int index = players.IndexOf(player);

		switch (players.Count)
		{
			case 1:
				return 0;
			case 2:
				if (index == 1)
					return 2;
				return 0;
			case 3:
				switch (index)
				{
					case 0:
						return 0;
					case 1:
						return 2;
					case 2:
						return 3;
				}
				break;
			case 4:
				return index;
			case 5:
				switch (index)
				{
					case 0:
						return 0;
					case 1:
						return 1;
					case 2:
						return 2;
					case 3:
						return 2;
					case 4:
						return 3;
				}
				break;
			case 6:
				switch (index)
				{
					case 0:
						return 0;
					case 1:
						return 1;
					case 2:
						return 2;
					case 3:
						return 2;
					case 4:
						return 3;
					case 5:
						return 3;
				}
				break;
			case 7:
				switch (index)
				{
					case 0:
						return 0;
					case 1:
						return 1;
					case 2:
						return 1;
					case 3:
						return 2;
					case 4:
						return 2;
					case 5:
						return 3;
					case 6:
						return 3;
				}
				break;
			case 8:
				switch (index)
				{
					case 0:
						return 0;
					case 1:
						return 1;
					case 2:
						return 1;
					case 3:
						return 2;
					case 4:
						return 2;
					case 5:
						return 2;
					case 6:
						return 3;
					case 7:
						return 3;
				}
				break;
		}

		return 0;
	}

	public void FireVictorySequence()
	{
		StartVictorySequence();
		StartCoroutine(PrepForVictory());

		IEnumerator PrepForVictory()
		{
			yield return new WaitUntil(() => { return readyPings >= TotalPlayerCount(); });

			readyPings = 0;
			EndVictorySequence();

			[ClientRpc] void EndVictorySequence()
			{
				StartCoroutine(EndSequence());
			}
		}

		[ClientRpc] void StartVictorySequence()
		{
			StartCoroutine(StartSequence());
		}

		IEnumerator StartSequence()
		{
			screenWipe.SetTrigger("wipeon");

			foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
			{
				if (player.isLocalPlayer)
				{
					player.SetStasis(true);
					player.UpdateVictoryStats();
				}
			}

			yield return new WaitForSeconds(0.75f);

			ToolTipManager.Instance.ClearTooltips();
			Destroy(currentLevel.gameObject);
			NotifyNextStep();

			[Command(requiresAuthority = false)] void NotifyNextStep()
			{
				readyPings++;

				if (IsLocalGame)
				{
					readyPings = TotalPlayerCount();
				}
			}
		}

		IEnumerator EndSequence()
		{
			victoryScreen.gameObject.SetActive(true);
			screenWipe.SetTrigger("wipeoff");

			yield return new WaitForSeconds(0.25f);

			var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
			victoryScreen.StartVictorySequence(players);
		}
	}

	public void RemovePlayer(PlayerController player)
	{
		Destroy(player.GetInput().gameObject);
		Destroy(player.gameObject);
	}

	public void CleanupGame()
	{
		FindAnyObjectByType<PlayerInputManager>().EnableJoining();
		PlayerInputManager.instance.onPlayerJoined -= AddLocalPlayer;
		SteamMatchmaking.LeaveLobby(Singleton<SteamManager>.Instance.LobbyID);
		if (NetworkServer.activeHost)
		{
			FindAnyObjectByType<NetworkManager>().StopHost();
		}
		else
		{
			FindAnyObjectByType<NetworkManager>().StopClient();
		}

		if (IsLocalGame)
		{
			foreach (var input in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
			{
				Destroy(input.gameObject);
			}
		}
	}
}
