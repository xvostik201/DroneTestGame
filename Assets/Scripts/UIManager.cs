using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Defaults")]
    [SerializeField] private int _defaultDroneCount = 5;
    [SerializeField] private float _defaultDroneSpeed = 5f;

    [Header("UI Controls")]
    [SerializeField] private Slider _dronesCountSlider;
    [SerializeField] private Slider _dronesSpeedSlider;
    [SerializeField] private TMP_InputField _spawnFreqInput;
    [SerializeField] private Toggle _drawPathToggle;

    [Header("Counters")]
    [SerializeField] private TMP_Text _faction1CountText;
    [SerializeField] private TMP_Text _faction2CountText;

    [Header("Bases")]
    [SerializeField] private MillitaryBase _faction1Base;
    [SerializeField] private MillitaryBase _faction2Base;

    public bool DrawPathToggleIsOn => _drawPathToggle.isOn;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _dronesCountSlider.onValueChanged.AddListener(OnDronesCountChanged);
        _dronesSpeedSlider.onValueChanged.AddListener(OnDronesSpeedChanged);
        _spawnFreqInput.onEndEdit.AddListener(OnSpawnFreqChanged);
        _drawPathToggle.onValueChanged.AddListener(OnDrawPathToggled);
    }

    private void Start()
    {
        int curr = _faction1Base.Drones.Count;
        _dronesCountSlider.minValue = _faction1Base.MinSpawnCount;
        _dronesCountSlider.maxValue = _faction1Base.MaxSpawnCount;
        _dronesCountSlider.value = curr > 0 ? curr : Mathf.Clamp(_defaultDroneCount, _faction1Base.MinSpawnCount, _faction1Base.MaxSpawnCount);

        _dronesSpeedSlider.minValue = 0.1f;
        _dronesSpeedSlider.maxValue = 20f;
        float sp = curr > 0 ? _faction1Base.Drones[0].CruiseSpeed : _defaultDroneSpeed;
        _dronesSpeedSlider.value = Mathf.Clamp(sp, _dronesSpeedSlider.minValue, _dronesSpeedSlider.maxValue);

        _spawnFreqInput.text = ResourceSpawner.Instance.RespawnInterval.ToString("F1");
        _drawPathToggle.isOn = false;
        UpdateAllCounters();
    }

    private void OnDestroy()
    {
        _dronesCountSlider.onValueChanged.RemoveListener(OnDronesCountChanged);
        _dronesSpeedSlider.onValueChanged.RemoveListener(OnDronesSpeedChanged);
        _spawnFreqInput.onEndEdit.RemoveListener(OnSpawnFreqChanged);
        _drawPathToggle.onValueChanged.RemoveListener(OnDrawPathToggled);
    }

    private void OnDronesCountChanged(float v)
    {
        int c = Mathf.RoundToInt(v);
        _faction1Base.SetDroneCount(c);
        _faction2Base.SetDroneCount(c);
    }

    private void OnDronesSpeedChanged(float v)
    {
        foreach (var dr in _faction1Base.Drones) dr.CruiseSpeed = v;
        foreach (var dr in _faction2Base.Drones) dr.CruiseSpeed = v;
    }

    private void OnSpawnFreqChanged(string txt)
    {
        if (float.TryParse(txt, out float f))
            ResourceSpawner.Instance.RespawnInterval = Mathf.Max(0.1f, f);
        else
            _spawnFreqInput.text = ResourceSpawner.Instance.RespawnInterval.ToString("F1");
    }

    private void OnDrawPathToggled(bool on)
    {
        foreach (var dr in _faction1Base.Drones) dr.SetDrawPath(on);
        foreach (var dr in _faction2Base.Drones) dr.SetDrawPath(on);
    }

    public void UpdateAllCounters()
    {
        _faction1CountText.text = ResourceCounter.GetCountForFaction(_faction1Base.FactionId).ToString();
        _faction2CountText.text = ResourceCounter.GetCountForFaction(_faction2Base.FactionId).ToString();
    }
}
