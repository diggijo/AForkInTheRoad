using Unity.VisualScripting;
using UnityEngine;

public class RoadNode : MonoBehaviour
{
    [SerializeField] private bool isPlayer;
    [SerializeField] private bool isDestination;

    public bool IsPlayer => isPlayer;
    public bool IsDestination => isDestination;
}
