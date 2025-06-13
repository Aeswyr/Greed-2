using Mirror;
using UnityEngine;

public class TurretController : NetworkBehaviour
{

    [SerializeField] private SpriteRenderer rangeIndicator;
    [SerializeField] private float shotDelay;
    [SerializeField] private float range;
    private float nextShot;
    [SyncVar] private Transform owner;

    void Start()
    {
        nextShot = Time.time + 1f;
        Color col = rangeIndicator.color;
        col.a = 0;
        rangeIndicator.color = col;  
    }
    public void SetOwner(Transform owner)
    {
        this.owner = owner;
    }
    public void OnHit(HitboxData data)
    {
        /*if (data.Owner == owner)
            return;*/// Design change, make it easy to accidentally destory your own gem, rather than the friendly fire angle

        VFXManager.Instance.SyncVFX(ParticleType.HITSPARK, 0.5f * (transform.position + data.transform.position), flip: false);
        OnBreak();

        [Command(requiresAuthority = false)] void OnBreak()
        {
            VFXManager.Instance.SyncVFX(ParticleType.TURRET_DESTROY, transform.position, false);
            NetworkServer.Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        PlayerController closest = null;
        foreach (var player in GameManager.Instance.GetPlayers())
        {
            if ((closest == null
            || Vector3.SqrMagnitude(transform.position - closest.transform.position) < Vector3.SqrMagnitude(transform.position - player.transform.position))
            && player.transform != owner)
                closest = player;
        }

        float dist = closest == null ? 100 : Vector3.Distance(closest.transform.position, transform.position);

        Color col = rangeIndicator.color;
        col.a = Mathf.Clamp((15f - dist) / 10f, 0, 0.5f);
        rangeIndicator.color = col;

        if (!isServer)
            return;

        if (Time.time < nextShot)
            return;



        if (closest != null && dist <= range + 1)
        {
            Vector2 aim = (closest.transform.position - transform.position).normalized;
            AttackBuilder.GetAttack(owner).SetParent(transform).SetSize(new Vector2(1f, 1f)).AddImmunity(transform)//.EnableFriendlyFire()
					.MakeProjectile(transform.position)
                    .SetAnimation(9)
                    .SetVelocity(30f * aim)
                    .SetLifetime(range / 30f)
                    .RotateWithVelocity()
                    .SetParticleType(ParticleType.PROJECTILE_HITSPARK)
                    .Finish();

            nextShot = Time.time + shotDelay;
        }
    }
}
