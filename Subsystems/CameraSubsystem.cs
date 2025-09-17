using UnityEngine;

public class CameraSubsystem
{
    private static Camera _camera;
    public static Camera Camera => _camera == null ? Camera.main : _camera;

    public static void SetMainCamera(Camera camera)
    {
        _camera = camera;
    }
}