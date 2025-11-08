using Mirror;
using UnityEngine;

public class RegencyInteractable : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private Sprite destroyed_sprite;
    public void OnInteract(PlayerController owner)
    {
        owner.GiveBuff(BuffType.REGENCY);
        SyncRegencyUsed();
    }

    public void SyncRegencyUsed()
    {
        if (isServer)
            Recieve();
        else
            Send();
        
        [Command] void Send()
        {
            Recieve();
        }
        
        [ClientRpc] void Recieve()
        {
            sprite.sprite = destroyed_sprite;
            hitbox.enabled = false; 
        }
    }
}
