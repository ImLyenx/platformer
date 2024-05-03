using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField]
    private Camera _camera;

    [Header("Profile System")]
    [SerializeField]
    private CameraProfile _defaultCameraProfile;
    private CameraProfile _currentCameraProfile;
    private float _profileTransitionTimer = 0f;
    private float _profileTransitionDuration = 0f;
    private Vector3 _profileTransitionStartPosition;
    private float _profileTransitionStartSize;

    private void Awake()
    {
        Instance = this;
    }

    private void _SetCameraPosition(Vector3 position)
    {
        Vector3 newCameraPosition = _camera.transform.position;
        newCameraPosition.x = position.x;
        newCameraPosition.y = position.y;
        _camera.transform.position = newCameraPosition;
    }

    private void _SetCameraSize(float size)
    {
        _camera.orthographicSize = size;
    }

    private void Start()
    {
        _InitToDefaultProfile();
    }

    private void _InitToDefaultProfile()
    {
        _currentCameraProfile = _defaultCameraProfile;
        _SetCameraPosition(_currentCameraProfile.Position);
        _SetCameraSize(_currentCameraProfile.CameraSize);
    }

    private void Update()
    {
        if (_IsPlayingProfileTransition())
        {
            _profileTransitionTimer += Time.deltaTime;
            Vector3 transitionPosition = _CalculateProfileTransitionCameraPosition(
                _currentCameraProfile.Position
            );
            _SetCameraPosition(transitionPosition);
            float transitionSize = _CalculateProfileTransitionCameraSize(
                _currentCameraProfile.CameraSize
            );
            _SetCameraSize(transitionSize);
        }
        else
        {
            _SetCameraPosition(_currentCameraProfile.Position);
            _SetCameraSize(_currentCameraProfile.CameraSize);
        }
    }

    public void EnterProfile(CameraProfile profile, CameraProfileTransition transition = null)
    {
        _currentCameraProfile = profile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
    }

    public void ExitProfile(CameraProfile profile, CameraProfileTransition transition = null)
    {
        if (_currentCameraProfile != profile)
            return;
        _currentCameraProfile = _defaultCameraProfile;
        if (transition != null)
        {
            _PlayProfileTransition(transition);
        }
    }

    private void _PlayProfileTransition(CameraProfileTransition transition)
    {
        _profileTransitionStartPosition = _camera.transform.position;
        _profileTransitionStartSize = _camera.orthographicSize;
        _profileTransitionTimer = 0f;
        _profileTransitionDuration = transition.duration;
    }

    private bool _IsPlayingProfileTransition()
    {
        return _profileTransitionTimer < _profileTransitionDuration;
    }

    private float _CalculateProfileTransitionCameraSize(float endSize)
    {
        float percentage = _profileTransitionTimer / _profileTransitionDuration;
        float startSize = _profileTransitionStartSize;
        return Mathf.Lerp(startSize, endSize, percentage);
    }

    private Vector3 _CalculateProfileTransitionCameraPosition(Vector3 endPosition)
    {
        float percentage = _profileTransitionTimer / _profileTransitionDuration;
        Vector3 startPosition = _profileTransitionStartPosition;
        return Vector3.Lerp(startPosition, endPosition, percentage);
    }
}
