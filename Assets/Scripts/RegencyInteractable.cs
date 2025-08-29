using Mirror;
using UnityEngine;

public class RegencyInteractable : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private Sprite destroyed_sprite;
    public void OnInteract(PlayerController owner)
    {
        sprite.sprite = destroyed_sprite;
        hitbox.enabled = false;
        owner.GiveBuff(BuffType.REGENCY);
    }
}
