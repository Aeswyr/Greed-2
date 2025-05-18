using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DartInteractable : NetworkBehaviour
{
    float nextFiring;
    public void OnInteract(PlayerController owner) {
        if (Time.time > nextFiring) {
            StartCoroutine(FiringSequence(owner));
        }
    }

    private IEnumerator FiringSequence(PlayerController owner) {
        Vector2 facing = new (GetComponent<SpriteRenderer>().flipX ? -1 : 1, 0);

        AttackBuilder.GetAttack(owner.transform).SetParent(owner.transform).SetSize(new Vector2(2f, 0.5f))
            .MakeProjectile(transform.position + 2f * Vector3.up + 1f * (Vector3)facing)
            .SetAnimation(3)
            .SetVelocity(40f * facing)
            .RotateWithVelocity()
            .SetParticleType(ParticleType.PROJECTILE_HITSPARK)
            .Finish();

        yield return new WaitForSeconds(0.15f);

        AttackBuilder.GetAttack(owner.transform).SetParent(owner.transform).SetSize(new Vector2(2f, 0.5f))
            .MakeProjectile(transform.position + 2.75f * Vector3.up + 1f * (Vector3)facing)
            .SetAnimation(3)
            .SetVelocity(40f * facing)
            .RotateWithVelocity()
            .SetParticleType(ParticleType.PROJECTILE_HITSPARK)
            .Finish();

        yield return new WaitForSeconds(0.15f);
        
        AttackBuilder.GetAttack(owner.transform).SetParent(owner.transform).SetSize(new Vector2(2f, 0.5f))
            .MakeProjectile(transform.position + 1.25f * Vector3.up + 1f * (Vector3)facing)
            .SetAnimation(3)
            .SetVelocity(40f * facing)
            .RotateWithVelocity()
            .SetParticleType(ParticleType.PROJECTILE_HITSPARK)
            .Finish();
    }
}
