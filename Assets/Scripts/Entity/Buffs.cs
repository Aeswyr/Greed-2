using UnityEngine;

public struct Buff {
    public BuffType type;
    public float expiration;
    public GameObject fx;
}

public enum BuffType {
    BARRIER, SWIFT, BLOODLUST, GHOSTFORM, GREED, RANDOM
}