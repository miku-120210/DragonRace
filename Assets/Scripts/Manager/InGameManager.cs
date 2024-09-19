using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Linq;
using System.Threading.Tasks;
using System;
using FusionUtilsEvents;

public class InGameManager : MonoBehaviour
{
  public FusionEvent OnPlayerDisconnectEvent;
  [SerializeField] private float _levelTime = 300f;

  [Networked] private TickTimer StartTimer { get; set; }
  
}
