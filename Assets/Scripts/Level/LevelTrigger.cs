using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System.Collections.Generic;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent<PlayerController> triggeredAction;
    [SerializeField] private bool onlyTriggerOnce;

    private List<PlayerController> triggeredPlayers = new();

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (onlyTriggerOnce && triggeredPlayers.Count > 0)
            return;
            
        var source = collision.transform.GetComponentInParent<PlayerController>();
        
        if (triggeredPlayers.Contains(source))
            return;

        triggeredPlayers.Add(source);
        triggeredAction.Invoke(source);
    }

    public void DestroyOnTrigger(PlayerController source)
    {
        Destroy(gameObject);
    }

    public void GiveMoneyOnTrigger(PlayerController source)
    {
        source.TryAddMoney(15);
    }

    public void EnablePeaceful(PlayerController source)
    {
        GameManager.Instance.SetPeaceful(true);
    }

    public void DisablePeaceful(PlayerController source)
    {
        GameManager.Instance.SetPeaceful(false);
    }

}
