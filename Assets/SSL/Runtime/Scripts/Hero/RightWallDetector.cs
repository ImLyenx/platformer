using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightWallDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField]
    private Transform[] _detectionPoints;

    [SerializeField]
    private float _detectionLength = 0.1f;

    [SerializeField]
    private LayerMask _wallLayerMask;

    public bool DetectRightWallNearby()
    {
        foreach (Transform detectionPoint in _detectionPoints)
        {
            RaycastHit2D hitResult = Physics2D.Raycast(
                detectionPoint.position,
                Vector2.right,
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
