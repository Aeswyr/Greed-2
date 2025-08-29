using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class KeyController : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D coll;
    List<Vector3> positions = new();
    Transform follow;

    void Start()
    {
        follow = transform;
    }

    void FixedUpdate()
    {
        if (!isServer)
            return;

        positions.Add(follow.position);
        if (follow != transform)
        {
            transform.position = positions[0];
        }

        if (positions.Count > 16)
        {
            positions.RemoveAt(0);
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
