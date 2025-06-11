using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleDrone : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float flyHeight = 10f;
    [SerializeField] private float takeoffSpeed = 5f;
    [SerializeField] private float landingSpeed = 5f;

    [Header("Movement Settings")]
    [SerializeField] private float _cruiseSpeed = 5f;
    [SerializeField] private float maxAcceleration = 10f;
    [SerializeField] private float separationDistance = 2f;
    [SerializeField] private float separationWeight = 1f;

    [Header("Avoidance Settings")]
    [SerializeField] private float rayDistance = 5f;

    [Header("Blade Settings")]
    [SerializeField] private Transform[] blades;
    [SerializeField] private float bladeSpeedFactor = 200f;

    [Header("Drop-Off Point")]
    public Transform dropPoint;

    [Header("Body Color")]
    [SerializeField] private MeshRenderer _bodyMesh;

    public float CruiseSpeed
    {
        get => _cruiseSpeed;
        set => _cruiseSpeed = value;
    }

    [HideInInspector] public MillitaryBase HomeBase;
    [HideInInspector] public List<Vector3> PathPoints = new List<Vector3>();

    private ResourceNode _reserved;
    private Vector3 _startPos, _lastPos;
    private const float queueSpacing = 1.5f;

    private void Start()
    {
        _startPos = transform.position;
        _lastPos = transform.position;
        ColorTo(HomeBase.Color);
    }

    private void Update()
    {
        float vel = (transform.position - _lastPos).magnitude / Time.deltaTime;
        foreach (var b in blades)
            b.Rotate(0, 0, vel * bladeSpeedFactor * Time.deltaTime, Space.Self);
        _lastPos = transform.position;
    }

    private void OnDestroy()
    {
        if (_reserved != null)
            _reserved.IsReserved = false;
    }

    public IEnumerator ProcessDeliveryCycle()
    {
        ColorTo(HomeBase.Color);

        var target = FindClosestResource();
        if (target == null) yield break;

        yield return VerticalLift(flyHeight);
        yield return MoveTo(new Vector3(target.transform.position.x, flyHeight, target.transform.position.z));
        yield return VerticalLift(target.transform.position.y);
        yield return new WaitForSeconds(2f);
        Destroy(target.gameObject);

        yield return VerticalLift(flyHeight);

        Vector3 dp = dropPoint.position;
        yield return MoveTo(new Vector3(dp.x, flyHeight, dp.z));

        int idx = HomeBase.EnqueueDrop(this);
        Vector3 hold = new Vector3(dp.x + idx * queueSpacing, flyHeight, dp.z);
        yield return MoveTo(hold);

        yield return new WaitUntil(() => HomeBase.GetDropIndex(this) == 0);

        yield return VerticalLift(dp.y);
        yield return new WaitForSeconds(1f);

        ResourceCounter.Add(HomeBase.FactionId);
        UIManager.Instance.UpdateAllCounters();

        yield return VerticalLift(flyHeight);
        HomeBase.DequeueDrop(this);

        yield return MoveTo(new Vector3(_startPos.x, flyHeight, _startPos.z));
        yield return VerticalLift(_startPos.y);
    }

    public void SetDrawPath(bool on)
    {
        if (on) PathPoints.Clear();
    }

    private IEnumerator MoveTo(Vector3 dest)
    {
        PathPoints.Clear();
        bool flying = true;
        while (flying)
        {
            PathPoints.Add(transform.position);

            Vector3 forward = transform.forward;
            Vector3 dir = (dest - transform.position).normalized;

            Vector3 sep = Vector3.zero;
            foreach (var o in HomeBase.Drones)
                if (o != this && Vector3.Distance(transform.position, o.transform.position) < separationDistance)
                    sep += (transform.position - o.transform.position).normalized;
            if (sep != Vector3.zero)
                dir = (dir + sep * separationWeight).normalized;

            Vector3 next = Physics.Raycast(transform.position, forward, out RaycastHit h, rayDistance)
                ? Vector3.Reflect(forward, h.normal)
                : dir;

            Quaternion rot = Quaternion.LookRotation(next, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                 Quaternion.Euler(0, rot.eulerAngles.y, 0),
                                                 Time.deltaTime * maxAcceleration);

            transform.position += transform.forward * _cruiseSpeed * Time.deltaTime;
            if (Vector3.Distance(transform.position, dest) < 0.5f)
                flying = false;

            yield return null;
        }
    }

    private IEnumerator VerticalLift(float y)
    {
        Vector3 from = transform.position, to = new Vector3(from.x, y, from.z);
        float speed = y > from.y ? takeoffSpeed : landingSpeed;
        float dur = Mathf.Abs(to.y - from.y) / speed, t = 0f;
        while (t < dur)
        {
            float p = t / dur;
            p = p < 0.5f ? 2f * p * p : -1f + (4f - 2f * p) * p;
            transform.position = Vector3.Lerp(from, to, p);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
    }

    private ResourceNode FindClosestResource()
    {
        ResourceNode best = null;
        float md = float.MaxValue;
        foreach (var r in ResourceNode.All)
            if (!r.IsReserved)
            {
                float d = Vector3.Distance(transform.position, r.transform.position);
                if (d < md) { md = d; best = r; }
            }
        if (best != null) { best.IsReserved = true; _reserved = best; }
        return best;
    }

    public void ColorTo(Color c)
    {
        if (_bodyMesh != null)
            _bodyMesh.material.color = c;
    }
}
