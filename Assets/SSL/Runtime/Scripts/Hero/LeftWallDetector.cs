using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftWallDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField]
    private Transform[] _detectionPoints;

    [SerializeField]
    private float _detectionLength = 0.1f;

    [SerializeField]
    private LayerMask _wallLayerMask;

    public bool DetectLeftWallNearby()
    {
        foreach (Transform detectionPoint in _detectionPoints)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.left,
                _detectionLength,
                _wallLayerMask
            );

            if (hitResult.collider != null)
            {
                return true;
            }
        }

        return false;
    }
}
