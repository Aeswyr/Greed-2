using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ExitMultitap : NetworkBehaviour
{
    [SerializeField] private ExitInteractable exitInteractable;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Sprite[] sprites;
    [SyncVar(hook = nameof(OnTapChanged))] private int taps;
    private float nextTap;
    public void OnInteract(PlayerController owner) {
        if (taps >= 3) {
            exitInteractable.OnInteract(owner);
        } else {
            AttemptTap();
        }
    }

    [Command(requiresAuthority =false)] private void AttemptTap() {
        if (Time.time > nextTap) {
            taps++;
            nextTap = Time.time + 1f;
        }
    }

    private void OnTapChanged(int oldValue, int newValue) {
        sprite.sprite = sprites[newValue];
    }
}
