using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class PlayerVictoryCard : MonoBehaviour
{
    [SerializeField] private ParticleSystem smallRewardBurst;
    [SerializeField] private ParticleSystem winFX;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private Image playerIcon;
    [SerializeField] private TextMeshProUGUI crownCount;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private MaterialLibrary colors;
    public PlayerController player;
    private int crowns;

    public void Start()
    {
        winText.alpha = 0;
    }
    public void SetIconMaterial(int index)
    {
        playerIcon.material = colors.GetUIColor(index);
    }

    public void SetCrownCount(int amt)
    {
        crownCount.text = amt.ToString();
        crowns = amt;
    }

    public void AddCrown()
    {
        crowns++;
        crownCount.text = crowns.ToString();
    }

    public int GetCrowns()
    {
        return crowns;
    }

    public void SetUsername(string name)
    {
        username.text = name;
    }

    public void FireSmallRewardFX()
    {
        smallRewardBurst.Play();
    }

    public IEnumerator FireWinner()
    {
        winFX.Play();
        yield return new WaitForSeconds(1f);
        winText.DOFade(1, 0.5f);
    }
}
