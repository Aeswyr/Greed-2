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
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image skillIcon;
    [SerializeField] private SpriteLibrary itemIconLibrary;

    public void SetWeapon(PickupType type)
    {
        if (type == PickupType.MAX) {
            weaponIcon.sprite = null;
            weaponIcon.color = Color.clear;
        } else {
            weaponIcon.sprite = itemIconLibrary[Utils.GetItemIconIndex(type)];
            weaponIcon.color = Color.white;
        }
        skillIcon.SetNativeSize();
    }

    public void SetSkill(PickupType type)
    {
        if (type == PickupType.MAX) {
            skillIcon.sprite = null;
            skillIcon.color = Color.clear;
        } else {
            skillIcon.sprite = itemIconLibrary[Utils.GetItemIconIndex(type)];
            skillIcon.color = Color.white;
        }
        skillIcon.SetNativeSize();
    }

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
