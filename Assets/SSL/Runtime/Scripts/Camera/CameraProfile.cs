using UnityEngine;

public class CameraProfile : MonoBehaviour
{
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

