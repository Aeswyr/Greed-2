using DG.Tweening;
using TMPro;
using UnityEngine;

public class LevelTooltip : MonoBehaviour
{
    private int playersOverlapping;
    [SerializeField] private TextMeshPro text;

    void Start()
    {
        if (playersOverlapping == 0)
        {
            text.alpha = 0;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        playersOverlapping++;

        if (playersOverlapping > 0)
        {
            text.DOFade(1, 0.5f);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        playersOverlapping--;

        if (playersOverlapping == 0)
        {
            text.DOFade(0, 0.75f);
        }
    }
}
