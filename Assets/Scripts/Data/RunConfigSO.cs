using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Run/Run Config")]
public class RunConfigSO : ScriptableObject
{
    [Tooltip("Ordered list of floors for this run")]
    public List<FloorSO> Floors;
}
