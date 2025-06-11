using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MillitaryBase : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _dronePrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private int _minSpawnCount = 1;
    [SerializeField] private int _maxSpawnCount = 10;

    [Header("Drop-Off Point")]
    [SerializeField] private Transform _dropPoint;
    public Transform DropPoint => _dropPoint;

    [Header("Appearance")]
    [SerializeField] private Color _color = Color.white;
    public Color Color => _color;

    [Header("Faction")]
    [SerializeField] private int _factionId = 1;
    public int FactionId => _factionId;

    public List<SimpleDrone> Drones { get; private set; } = new List<SimpleDrone>();
    private readonly List<SimpleDrone> _dropQueue = new List<SimpleDrone>();

    private void Start()
    {
        int count = Random.Range(_minSpawnCount, _maxSpawnCount + 1);
        for (int i = 0; i < count; i++)
            SpawnOneDrone();
    }

    private void SpawnOneDrone()
    {
        Transform sp = _spawnPoints[Drones.Count % _spawnPoints.Length];
        GameObject go = Instantiate(_dronePrefab, sp.position, Quaternion.identity);
        SimpleDrone dr = go.GetComponent<SimpleDrone>();
        dr.HomeBase = this;
        dr.dropPoint = _dropPoint;
        Drones.Add(dr);
        StartCoroutine(DroneDeliveryLoop(dr));
    }

    private IEnumerator DroneDeliveryLoop(SimpleDrone dr)
    {
        while (dr != null)
        {
            yield return new WaitUntil(() => ResourceNode.All.Any(r => !r.IsReserved));
            if (dr == null) yield break;
            yield return dr.ProcessDeliveryCycle();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetDroneCount(int desiredCount)
    {
        int current = Drones.Count;
        if (desiredCount > current)
            for (int i = 0; i < desiredCount - current; i++)
                SpawnOneDrone();
        else if (desiredCount < current)
            for (int i = 0; i < current - desiredCount; i++)
            {
                var d = Drones[Drones.Count - 1];
                Drones.RemoveAt(Drones.Count - 1);
                Destroy(d.gameObject);
            }
    }

    public int EnqueueDrop(SimpleDrone dr)
    {
        if (!_dropQueue.Contains(dr))
            _dropQueue.Add(dr);
        return _dropQueue.IndexOf(dr);
    }

    public int GetDropIndex(SimpleDrone dr) => _dropQueue.IndexOf(dr);

    public void DequeueDrop(SimpleDrone dr)
    {
        if (_dropQueue.Count > 0 && _dropQueue[0] == dr)
            _dropQueue.RemoveAt(0);
    }

    public int MinSpawnCount => _minSpawnCount;
    public int MaxSpawnCount => _maxSpawnCount;
}
