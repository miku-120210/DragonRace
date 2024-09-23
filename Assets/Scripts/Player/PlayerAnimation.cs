using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private PlayerRigidBodyMovement _movement;
    [SerializeField] private Animator _anim;

    void LateUpdate()
    {
        if (_movement.Velocity.x < -.1f)
        {
            _renderer.flipX = false;
        }
        else if (_movement.Velocity.x > .1f)
        {
            _renderer.flipX = true;
        }

        //_anim.SetBool("Grounded", _movement.GetGrounded());
        //_anim.SetFloat("Y_Speed", _movement.Velocity.y);
        //_anim.SetFloat("X_Speed_Abs", Mathf.Abs(_movement.Velocity.x));
    }

}
