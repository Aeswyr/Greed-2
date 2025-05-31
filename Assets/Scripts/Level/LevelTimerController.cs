using Mirror;
using TMPro;
using UnityEngine;

public class LevelTimerController : NetworkBehaviour
{
    [SerializeField] private float finalLevelTimer;
    [SerializeField] private TextMeshPro timer;
    float endTime;
    [SyncVar(hook = nameof(UpdateTimer))] int seconds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endTime = Time.time + finalLevelTimer;
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        int nextSeconds = (int)(endTime - Time.time);
        if (seconds != nextSeconds)
        {
            seconds = nextSeconds;
        }

        if (Time.time > endTime)
        {
            GameManager.Instance.FireVictorySequence();
            Destroy(gameObject);
        }
    }

    private void UpdateTimer(int oldValue, int newValue)
    {
        timer.text = newValue.ToString();
    }

}
