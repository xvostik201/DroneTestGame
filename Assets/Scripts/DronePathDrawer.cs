using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(SimpleDrone))]
public class DronePathDrawer : MonoBehaviour
{
    [Header("Path Drawing")]
    [SerializeField] private Color _pathColor = Color.cyan;

    private LineRenderer _lr;
    private SimpleDrone _drone;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _drone = GetComponent<SimpleDrone>();
        _lr.startColor = _lr.endColor = _pathColor;
    }

    private void Update()
    {
        if (!UIManager.Instance.DrawPathToggleIsOn)
        {
            _lr.positionCount = 0;
            return;
        }

        var pts = _drone.PathPoints;
        _lr.positionCount = pts.Count;
        _lr.SetPositions(pts.ToArray());
    }
}
