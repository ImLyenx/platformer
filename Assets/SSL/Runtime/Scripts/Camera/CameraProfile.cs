using UnityEngine;

public enum CameraProfileType
{
    Static = 0,
    FollowTarget
}

public class CameraProfile : MonoBehaviour
{
    [Header("Profile Type")]
    [SerializeField]
    private CameraProfileType _profileType = CameraProfileType.Static;

    [Header("Follow")]
    [SerializeField]
    private Transform _targetToFollow = null;

    public CameraProfileType ProfileType => _profileType;

    public Transform TargetToFollow => _targetToFollow;

    private Camera _camera;

    public float CameraSize => _camera.orthographicSize;

    public Vector3 Position => _camera.transform.position;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            _camera.enabled = false;
        }
    }
}
