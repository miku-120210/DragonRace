using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public NetworkPrefabRef Player;
    [SerializeField] private List<GameObject> _spawnPoints;

}
