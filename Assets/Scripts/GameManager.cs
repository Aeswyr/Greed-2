using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using Telepathy;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
	[SerializeField]
	private TextMeshProUGUI levelDisplay;

	[SerializeField]
	private LevelController currentLevel;

	[SerializeField]
	private GameObject[] levels;

	[SerializeField]
	private GameObject[] shops;
	[Header("UI")]
	[SerializeField] private Animator screenWipe;

	private int levelCount = 0;

	private int readyPings;

	public Transform PlayerLobbyHolder => playerLobbyHolder;

	public bool IsLocalGame {
		get;
		private set;
	}

	private void Start()
	{
		if (PlayerInputManager.instance != null) {
			IsLocalGame = true;
			PlayerInputManager.instance.onPlayerJoined += AddLocalPlayer;
		}

		LoadLevelObjects();
	}

	public int TotalPlayerCount()
	{
		return FindObjectsOfType<PlayerController>().Length;
	}

	public void AddLocalPlayer(PlayerInput input) {
		Debug.Log("Creating new player");

		var playerObj = Instantiate(playerPrefab);
		NetworkServer.Spawn(playerObj);
		playerObj.GetComponent<PlayerController>().SetupInput(input.GetComponent<InputHandler>());
	}

	public void AddLobbyCard(PlayerController player, InputHandler input)
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
		LobbyCardController[] array = FindObjectsOfType<LobbyCardController>();
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

		AssignPlayerIds();

		SyncGameStart();
	}

	private void AssignPlayerIds() {
		PlayerController[] array = FindObjectsOfType<PlayerController>();
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
		if (IsLocalGame) {
			Destroy(FindObjectOfType<PlayerInputManager>());
		}

		HandlePlayerSpawns();

		playerLobby.SetActive(value: false);
		PlayerController[] array = FindObjectsOfType<PlayerController>();
		foreach (PlayerController playerController in array)
		{
			if (playerController.isLocalPlayer)
			{
				playerController.SetInputLocked(value: false);
			}
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

	public void SpawnGemBurst(Vector3 position, int amount)
	{
		while (amount > 0)
		{
			GameObject gameObject = Instantiate(pickupPrefab, position + Vector3.up, Quaternion.identity);
			Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
			component.velocity = new Vector2(Random.Range(-15, 15), Random.Range(40, 60));

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
		AuctionableInteractable[] array = FindObjectsOfType<AuctionableInteractable>();
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
		levelCount++;
		GameObject original = levels[id];
		if (IsLevelShop())
		{
			original = shops[id];
		}

		currentLevel = Instantiate(original).GetComponent<LevelController>();
		
		LoadLevelObjects();

		HandlePlayerSpawns();
	}

	private void HandlePlayerSpawns() {
		PlayerController[] array = FindObjectsOfType<PlayerController>();
		List<int> playerIds = new();

		foreach (PlayerController playerController in array)
		{
			StartCoroutine(SetupLevel(playerIds, playerController));
		}

		for (int i = 0; i < currentLevel.SpawnPoints.Count; i++) {
			if (!playerIds.Contains(i))
				currentLevel.SpawnPoints[i].SetActive(false);

		}
	}

	private IEnumerator SetupLevel(List<int> playerIds, PlayerController playerController) {
		playerIds.Add(playerController.PlayerID);
		if (playerController.isLocalPlayer)
		{
			yield return new WaitUntil(() => playerController.PlayerID != -1);

			playerController.transform.position = currentLevel.SpawnPoints[playerController.PlayerID].transform.position;
			playerController.EnterLevel();
			playerController.PingNameplate();
			levelDisplay.text = levelCount.ToString("D2");
		}
	}

	public void CompleteLevelTransition() {
		PlayerController[] array = FindObjectsOfType<PlayerController>();

		foreach (PlayerController playerController in array)
			if (playerController.isLocalPlayer)
				playerController.SetStasis(value: false);

	}

	private void LoadLevelObjects()
	{
		LevelObjectSpawn[] array = FindObjectsOfType<LevelObjectSpawn>();
		foreach (LevelObjectSpawn levelObjectSpawn in array)
		{
			if (isServer)
			{
				GameObject gameObject = Instantiate(levelObjectSpawn.GetSpawn(), levelObjectSpawn.transform.position, levelObjectSpawn.transform.rotation);
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
			Destroy(levelObjectSpawn.gameObject);
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

	private IEnumerator StartLevelLoadSequence() {
		screenWipe.SetTrigger("wipeon");
		
		yield return new WaitForSeconds(0.25f);
		
		Destroy(currentLevel.gameObject);

		NotifyReadyToLoad();
	}

	[Command(requiresAuthority = false)] private void NotifyReadyToLoad() {
		readyPings++;

		if (readyPings >= TotalPlayerCount() || IsLocalGame) {
			readyPings = 0;

			int nextLevelId = 0;
			FinishLoad(nextLevelId);
		}
	}

	[ClientRpc] private void FinishLoad(int id) {
		LoadLevel(id);

		StartCoroutine(FinishLevelLoadSequence());
	} 
	
	private IEnumerator FinishLevelLoadSequence() {
		yield return new WaitForSeconds(0.25f);

		screenWipe.SetTrigger("wipeoff");
		yield return new WaitForSeconds(0.25f);

		CompleteLevelTransition();
	}
}
