using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ExitMultitap : NetworkBehaviour
{
    [SerializeField] private ExitInteractable exitInteractable;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private ParticleSystem[] openingFX;
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
        if (Time.time > nextTap && taps < 3) {
            taps++;
            nextTap = Time.time + 1f;
            #if UNITY_EDITOR
                //nextTap = Time.time;
            #endif
        }
    }

    private void OnTapChanged(int oldValue, int newValue)
    {
        if (newValue > 3)
            newValue = 3;
        sprite.sprite = sprites[newValue];
        VFXManager.Instance.Screenshake(0.1f * newValue, 0.2f + 0.1f * newValue);
        SFXManager.Instance.PlaySound("knock");

        openingFX[newValue - 1].Play();
    }
}
