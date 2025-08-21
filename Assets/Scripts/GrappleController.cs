using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GrappleController : MonoBehaviour
{
    [SerializeField] private Transform player;
    private LineRenderer lineRend;
    private Vector3 anchorPos;

    void Awake()
    {
        lineRend = GetComponent<LineRenderer>();
    }

    void DrawGrappleTight() {
        lineRend.positionCount = 2;

        lineRend.SetPosition(0, anchorPos);
        lineRend.SetPosition(1, player.position);
    }

    [Header("General Settings:")]
    [SerializeField] private int percision = 40;
    [Range(0, 20)] [SerializeField] private float straightenLineSpeed = 5;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] [SerializeField] private float StartWaveSize = 2;
    float waveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;

    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;

    bool strightLine = true;
    bool retracting = false;

    void Start()
    {
        lineRend.enabled = false;   
    }
    public void AnimatePullSequence(Vector3 anchorPos, float duration)
    {
        this.anchorPos = anchorPos;
        moveTime = 0;
        lineRend.positionCount = percision;
        waveSize = StartWaveSize;
        strightLine = false;

        LinePointsToFirePoint();

        lineRend.enabled = true;

        StartCoroutine(EndSequence(duration));
    }

    public IEnumerator EndSequence(float duration)
    {
        yield return new WaitForSeconds(duration/2);
        retracting = true;

        yield return new WaitForSeconds(duration/2);

        retracting = false;
        lineRend.enabled = false;
        isGrappling = false;
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < percision; i++)
        {
            lineRend.SetPosition(i, player.position);
        }
    }

    private void Update()
    {
        if (lineRend.enabled)
        {
            moveTime += Time.deltaTime;
            DrawRope();
        }

        if (retracting)
        {
            anchorPos = player.transform.position + 0.5f * (anchorPos - player.transform.position);
        }
    }

    void DrawRope()
    {
        if (!strightLine)
        {
            if (lineRend.GetPosition(percision - 1).x == anchorPos.x)
            {
                strightLine = true;
            }
            else
            {
                DrawRopeWaves();
            }
        }
        else
        {
            if (!isGrappling)
            {
                isGrappling = true;
            }
            if (waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                waveSize = 0;

                if (lineRend.positionCount != 2) { lineRend.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves()
    {
        for (int i = 0; i < percision; i++)
        {
            float delta = (float)i / ((float)percision - 1f);
            Vector2 offset = Vector2.Perpendicular(anchorPos - player.position).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(player.position, anchorPos, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(player.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            lineRend.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        lineRend.SetPosition(0, player.position);
        lineRend.SetPosition(1, anchorPos);
    }
}
