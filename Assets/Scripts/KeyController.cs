using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class KeyController : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D coll;
    Vector3 targetPos;
    Transform follow;

    private int nextPos;
    void Start()
    {
        follow = transform;
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        Vector3 dif = follow.position - transform.position;
        if (dif.sqrMagnitude > 4)
        {
            transform.position += Mathf.Max(10, dif.magnitude * 3) * Time.fixedDeltaTime * dif.normalized;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;
            
        var player = collision.transform.parent.GetComponent<PlayerController>();

        if (player.HasKey())
            return;

        coll.enabled = false;
        follow = player.transform;
        player.AddKey(this);
    }

    public void DetatchKey()
    {
        coll.enabled = true;

        follow = transform;
    }
}
