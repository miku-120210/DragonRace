using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float Speed = 1f;
    public Transform CameraTarget;

    private Vector3 _offset = new Vector3(0, 5, 0);
    private float _step;

    // private List<PlayerBehaviour> _spectatingList = new List<PlayerBehaviour>();
    private bool _spectating = false;
    
    private void LateUpdate()
    {
        if (CameraTarget == null)
        {
            return;
        }

        _step = Speed * Vector2.Distance(CameraTarget.position, transform.position) * Time.deltaTime;

        Vector2 pos = Vector2.MoveTowards(transform.position, CameraTarget.position + _offset, _step);
        transform.position = pos;
    }
}
