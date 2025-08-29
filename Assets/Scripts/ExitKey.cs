using Mirror;
using UnityEngine;

public class ExitKey : NetworkBehaviour
{
    [SerializeField] private ExitInteractable exitInteractable;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private ParticleSystem[] openingFX;
    [SyncVar(hook = nameof(OnKeyChanged))] private int keys;
    public void OnInteract(PlayerController owner) {
        if (keys >= 3) {
            exitInteractable.OnInteract(owner);
        } else {
            AttemptKey(owner);
        }
    }

    [Command(requiresAuthority =false)] private void AttemptKey(PlayerController player) {
        if (player.HasKey() && keys < 3)
        {
            keys++;
            var key = player.UseKey();
            NetworkServer.Destroy(key.gameObject);
        }
    }

    private void OnKeyChanged(int oldValue, int newValue)
    {
        if (newValue > 3)
            newValue = 3;
        sprite.sprite = sprites[newValue];
        VFXManager.Instance.Screenshake(0.1f * newValue, 0.2f + 0.1f * newValue);
        SFXManager.Instance.PlaySound("knock");

        openingFX[newValue - 1].Play();
    }
}
