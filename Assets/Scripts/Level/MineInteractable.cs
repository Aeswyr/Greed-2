using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class MineInteractable : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject detectRadius;
    private bool activated;
    private PlayerController owner;

    public void OnInteract(PlayerController owner) {
        
        SendActivation(owner);

        [Command(requiresAuthority = false)] void SendActivation(PlayerController owner) {
            RecieveActivation();

            activated = true;
            this.owner = owner;
            detectRadius.SetActive(true);
        }

        [ClientRpc] void RecieveActivation() {
            animator.Play("active");
        }
    }

    [Command(requiresAuthority = false)] public void OnTrigger() {
        if (activated) {
            var hits = Physics2D.CircleCastAll(transform.position, 6f, Vector2.zero, 0, layerMask: LayerMask.GetMask(new[]{"PlayerDetect"}));

            if (hits.Length > 1 
                || (hits.Length == 1 && hits[0].transform.TryGetComponent<PlayerController>(out var player) && player != owner)) {
                AttackBuilder.GetAttack(owner.transform).SetParent(transform).SetSize(new Vector2(10f, 10f)).SetDuration(0.2f)
                    .MakeProjectile(transform.position)
                    .DisableWorldImpact()
                    .DisableEntityImpact()
                    .SetAnimation(5)
                    .SetLifetime(0.85f)
                    .Finish();
                
                NetworkServer.Destroy(gameObject);
                }
        }
    }
}
