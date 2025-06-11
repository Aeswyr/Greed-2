using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FountainInteractable : NetworkBehaviour
{

    [SerializeField] private Animator animator;
    public void OnInteract(PlayerController owner) {
        
        SendActivation();
        if (Random.Range(0, 100) > 25)
        {
            owner.GiveBuff(BuffType.RANDOM);
        }
        else
        {
            owner.GetItem((PickupType)Random.Range((int)PickupType.ITEM_POTION_HEALTH, (int)PickupType.MAX));
        }

        [Command(requiresAuthority = false)] void SendActivation() {
            RecieveActivation();
        }

        [ClientRpc] void RecieveActivation() {
            animator.Play("empty");
        }
    }
}
