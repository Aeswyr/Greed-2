using TMPro;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI body;
    [SerializeField] private GameObject pointer;

    public void SetBodyText(string text)
    {
        body.text = text;
    }

    public void SetTitleText(string text)
    {
        title.text = text;
    }

    public void UpdatePointerPosition(float xPos)
    {
        Vector3 pos = pointer.transform.position;
        pos.x = xPos;
        pointer.transform.position = pos;
    }
}
