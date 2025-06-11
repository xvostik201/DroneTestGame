using System.Collections;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public static ResourceSpawner Instance { get; private set; }
    public float RespawnInterval;

    [Header("Общие настройки")]
    [SerializeField] private GameObject _resourcePrefab;
    [SerializeField] private int _initialCount = 20;
    [SerializeField] private float _respawnInterval = 5f;
    [SerializeField] private int _maxResources = 100;

    [Header("Зона спавна — кольцо")]
    [SerializeField] private float _minRadius = 10f;
    [SerializeField] private float _maxRadius = 25f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        RespawnInterval = _respawnInterval;
        for (int i = 0; i < _initialCount; i++)
            SpawnResource();
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(RespawnInterval);
            if (ResourceNode.All.Count < _maxResources)
                SpawnResource();
        }
    }

    private void SpawnResource()
    {
        Vector3 pos = GetRandomPositionInAnnulus();
        Instantiate(_resourcePrefab, pos, Quaternion.identity);
    }

    private Vector3 GetRandomPositionInAnnulus()
    {
        float t = Random.value;
        float radius = Mathf.Sqrt(t * (_maxRadius * _maxRadius - _minRadius * _minRadius) + _minRadius * _minRadius);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 c = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        return transform.position + new Vector3(c.x, 0f, c.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        DrawCircle(transform.position, _maxRadius);
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        DrawCircle(transform.position, _minRadius);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        const int seg = 64;
        Vector3 prev = center + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;
        for (int i = 1; i <= seg; i++)
        {
            float theta = (float)i / seg * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
