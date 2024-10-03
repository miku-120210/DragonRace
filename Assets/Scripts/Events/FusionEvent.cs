using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace FusionUtilsEvents
{
    [CreateAssetMenu]
    public class FusionEvent : ScriptableObject
    {
        private List<Action<PlayerRef, NetworkRunner>> _responses = new ();

        public void Raise(PlayerRef player = default, NetworkRunner runner = null)
        {
            for (int i = 0; i < _responses.Count; i++)
            {
                _responses[i].Invoke(player, runner);
            }
        }

        public void RegisterResponse(Action<PlayerRef, NetworkRunner> response)
        {
            _responses.Add(response);
        }

        public void RemoveResponse(Action<PlayerRef, NetworkRunner> response)
        {
            if (_responses.Contains(response))
                _responses.Remove(response);
        }
    }
}
