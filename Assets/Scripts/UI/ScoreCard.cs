using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCard : MonoBehaviour
{
    [SerializeField] private MaterialLibrary colors;
    [SerializeField] private Image playerIcon;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI crownText;
    [SerializeField] private TextMeshProUGUI nameplateText;

    public void SetGold(int val)
    {
        goldText.text = val.ToString();
    }

    public void SetCrowns(int val)
    {
        crownText.text = val.ToString();
    }

    public void SetName(string name)
    {
        nameplateText.text = name;
    }

    public void SetColor(int colorId)
    {
        playerIcon.material = colors.GetUIColor(colorId);
    }
}
