using TMPro;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI body;

    public void SetBodyText(string text)
    {
        body.text = text;
    }

    public void SetTitleText(string text)
    {
        title.text = text;
    }
}
