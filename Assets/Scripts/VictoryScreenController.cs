using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using DG.Tweening;
using TMPro;

public class VictoryScreenController : MonoBehaviour
{
    [SerializeField] private GameObject playerPodiumPrefab;
    [SerializeField] private Transform playerPodiumHolder;
    [SerializeField] private TextMeshProUGUI bonusCrownTitle;
    [SerializeField] private TextMeshProUGUI bonusCrownDesc;

    private List<PlayerVictoryCard> playerCards = new();

    public void Start()
    {
        bonusCrownTitle.alpha = 0;
        bonusCrownDesc.alpha = 0;
    }

    public void StartVictorySequence(PlayerController[] players)
    {

        foreach (var player in players)
        {
            var card = Instantiate(playerPodiumPrefab, playerPodiumHolder).GetComponent<PlayerVictoryCard>();

            card.SetIconMaterial(player.GetCurrentColor());
            card.SetUsername(player.GetDisplayName());
            card.player = player;

            playerCards.Add(card);
        }

        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        List<PlayerVictoryCard> baseCrowns = new(playerCards);
        for (int i = 0; baseCrowns.Count > 0; i++)
        {
            for (int j = 0; j < baseCrowns.Count; j++)
            {
                var card = baseCrowns[j];

                card.SetCrownCount(i);
                if (i != 0)
                    GiveCrownFX(card);

                if (i >= card.player.GetVictoryStats().CrownsHeld)
                {
                    baseCrowns.RemoveAt(j);
                    j--;
                }
            }

            yield return new WaitForSeconds(0.75f);
        }

        yield return GrantBonusCrown("Treasure Hoarder", delegate(PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().MoneyHeld > b.player.GetVictoryStats().MoneyHeld)
                return -1;
            if (a.player.GetVictoryStats().MoneyHeld < b.player.GetVictoryStats().MoneyHeld)
                return 1;
            return 0;
        });

        yield return GrantBonusCrown("Big Spender", delegate(PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().MoneySpent > b.player.GetVictoryStats().MoneySpent)
                return -1;
            if (a.player.GetVictoryStats().MoneySpent < b.player.GetVictoryStats().MoneySpent)
                return 1;
            return 0;
        });

        yield return GrantBonusCrown("Gladiator", delegate(PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().HitsLanded > b.player.GetVictoryStats().HitsLanded)
                return -1;
            if (a.player.GetVictoryStats().HitsLanded < b.player.GetVictoryStats().HitsLanded)
                return 1;
            return 0;
        });

        yield return GrantBonusCrown("Dungeon Delver", delegate(PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().DoorsEntered > b.player.GetVictoryStats().DoorsEntered)
                return -1;
            if (a.player.GetVictoryStats().DoorsEntered < b.player.GetVictoryStats().DoorsEntered)
                return 1;
            return 0;
        });

        yield return GrantBonusCrown("Close Calls", delegate(PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().CloseCalls > b.player.GetVictoryStats().CloseCalls)
                return -1;
            if (a.player.GetVictoryStats().CloseCalls < b.player.GetVictoryStats().CloseCalls)
                return 1;
            return 0;
        });
        
        yield return GrantBonusCrown("Collector", delegate (PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().ThingsCollected > b.player.GetVictoryStats().ThingsCollected)
                return -1;
            if (a.player.GetVictoryStats().ThingsCollected < b.player.GetVictoryStats().ThingsCollected)
                return 1;
            return 0;
        });

        yield return GrantBonusCrown("Lucky", delegate (PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.player.GetVictoryStats().Luck > b.player.GetVictoryStats().Luck)
                return -1;
            if (a.player.GetVictoryStats().Luck < b.player.GetVictoryStats().Luck)
                return 1;
            return 0;
        });


        yield return new WaitForSeconds(2f);

        playerCards.Sort(delegate (PlayerVictoryCard a, PlayerVictoryCard b)
        {
            if (a.GetCrowns() > b.GetCrowns())
                return -1;
            if (a.GetCrowns() < b.GetCrowns())
                return 1;
            return 0;
        });

        yield return playerCards[0].FireWinner();

        

        IEnumerator GrantBonusCrown(string name, System.Comparison<PlayerVictoryCard> comparer)
        {
            yield return new WaitForSeconds(1f);

            bonusCrownTitle.DOFade(1, 0.5f);
            yield return new WaitForSeconds(1f);

            bonusCrownDesc.text = name;
            bonusCrownDesc.DOFade(1, 0.5f);

            yield return new WaitForSeconds(2f);
            playerCards.Sort(comparer);
            playerCards[0].AddCrown();
            GiveCrownFX(playerCards[0]);
            yield return new WaitForSeconds(2f);
            
            bonusCrownTitle.DOFade(0, 0.5f);
            bonusCrownDesc.DOFade(0, 0.5f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void GiveCrownFX(PlayerVictoryCard card)
    {
        card.FireSmallRewardFX();
        var rect = (RectTransform)card.transform;
        rect.DOSizeDelta(rect.sizeDelta + 75 * Vector2.up, 0.75f);
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
