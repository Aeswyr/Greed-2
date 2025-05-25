using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;

public class VictoryScreenController : MonoBehaviour
{
    [SerializeField] private GameObject playerPodiumPrefab;
    [SerializeField] private Transform playerPodiumHolder;

    private List<PlayerVictoryCard> playerCards = new();

    public void StartVictorySequence(PlayerController[] players)
    {
        foreach (var player in players)
        {
            var card = Instantiate(playerPodiumPrefab, playerPodiumHolder).GetComponent<PlayerVictoryCard>();

            card.SetIconMaterial(player.GetCurrentColor());
            card.SetUsername(Utils.GetLocalSteamName());
            card.player = player;

            playerCards.Add(card);
        }

        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(1f);

        List<PlayerVictoryCard> baseCrowns = playerCards;
        for (int i = 0; baseCrowns.Count > 0; i++)
        {
            for (int j = 0; j < baseCrowns.Count; j++)
            {
                baseCrowns[j].SetCrownCount(i);
                var rect = (RectTransform)baseCrowns[j].transform;
                rect.sizeDelta = rect.sizeDelta + 75 * Vector2.up;

                if (i >= baseCrowns[j].player.GetVictoryStats().CrownsHeld)
                {
                    baseCrowns.RemoveAt(j);
                    j--;
                }
            }

            yield return new WaitForSeconds(0.75f);
        }
    } 

    public void ToTitle()
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
}
