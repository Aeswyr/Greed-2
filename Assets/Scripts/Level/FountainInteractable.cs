using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FountainInteractable : NetworkBehaviour
{

    [SerializeField] private Animator animator;
    public void OnInteract(PlayerController owner) {
        
        SendActivation();
        owner.GiveBuff(BuffType.RANDOM);

        [Command(requiresAuthority = false)] void SendActivation() {
            RecieveActivation();
        }

        [ClientRpc] void RecieveActivation() {
            animator.Play("empty");
        }
    }
}
