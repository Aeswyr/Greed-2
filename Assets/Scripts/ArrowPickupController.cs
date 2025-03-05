using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ArrowPickupController : NetworkBehaviour
{
    [SyncVar] private Transform owner;
    [SerializeField] private Rigidbody2D rbody;
    private bool chasing;
    private float chasingStart;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }
    public void SetOwner(Transform owner) {
        this.owner = owner;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isServer) {
            Vector3 dif = owner.position - transform.position;
            if (dif.sqrMagnitude < 4) {
                NetworkServer.Destroy(gameObject);

                owner.GetComponent<PlayerController>().RefreshAmmo();
            } else if (!chasing && (dif.sqrMagnitude < 36 || Time.time - spawnTime > 12.5f)) {
                chasing = true;
                chasingStart = Time.time;
            }

            if (chasing) {
                rbody.velocity = (1 + 2 * Mathf.Min((Time.time - chasingStart) / 3, 1)) * 15 * dif.normalized;
                transform.localRotation = Quaternion.FromToRotation(Vector2.right, -rbody.velocity);
            }
        }
    }
}
