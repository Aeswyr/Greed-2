using Mirror;
using UnityEngine;

public class SyncSpriteFlip : NetworkBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SyncVar(hook = nameof(OnSpriteFlip))]
    private bool flipX;

    private void OnSpriteFlip(bool oldValuve, bool newValue)
    {
        spriteRenderer.flipX = newValue;
    }

    public void SetFlipX(bool newState)
    {
        flipX = newState;
    }
}
