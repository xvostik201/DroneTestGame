using UnityEngine;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
    public static readonly List<ResourceNode> All = new List<ResourceNode>();
    public MillitaryBase OwnerBase;
    public bool IsReserved;

    private void Awake() => All.Add(this);
    private void OnDestroy() => All.Remove(this);

    public void Claim()
    {
        IsReserved = true;
        ResourceCounter.Add(OwnerBase.FactionId);
    }
}
