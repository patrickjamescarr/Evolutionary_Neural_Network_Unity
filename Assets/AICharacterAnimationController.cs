using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AICharacterAnimationController : MonoBehaviour
{
    public AIPath aiPath;
    IsometricCharacterRenderer isoRenderer;


    private void Awake()
    {
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        isoRenderer.SetDirection(new Vector2(aiPath.desiredVelocity.x, aiPath.desiredVelocity.y));
    }
}
