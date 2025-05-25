using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerVictoryCard : MonoBehaviour
{
    [SerializeField] private Image playerIcon;
    [SerializeField] private TextMeshProUGUI crownCount;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private MaterialLibrary colors;
    public PlayerController player;

    public void SetIconMaterial(int index)
    {
        playerIcon.material = colors.GetUIColor(index);
    }

    public void SetCrownCount(int amt) {
        crownCount.text = amt.ToString();
    }

    public void SetUsername(string name) {
        username.text = name;
    }
}
