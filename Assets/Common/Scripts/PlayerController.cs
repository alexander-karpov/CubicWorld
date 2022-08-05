using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public ContactFilter2D MovementFilter;

    [SerializeField]
    public float Speed = 5f;

    [SerializeField]
    public float JumpHeight = 1.5f;

    [SerializeField]
    public AnimationCurve jumpCurve;

    [SerializeField]
    public float JumpDuration = 1f;

    SpriteRenderer _spriteRenderer;

    Rigidbody2D _rb;

    RaycastHit2D[] moveCollisions = new RaycastHit2D[3];

    Vector2 _groundNormal = Vector2.up;

    Vector2 _positionOnGround;

    float _jumpTime = 0f;

    // bool isFacingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        _rb = this.GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _spriteRenderer = this.GetComponent<SpriteRenderer>();

        _positionOnGround = _rb.position;
    }

    void FixedUpdate()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var jump = Input.GetAxisRaw("Jump");

        Move (horizontal);
        Flip (horizontal);
        Jump (jump);
    }

    Vector2 JumpOffset
    {
        get
        {
            return _groundNormal * jumpCurve.Evaluate(_jumpTime / JumpDuration) * JumpHeight;
        }
    }

    private void Jump(float jump)
    {
        if (jump > 0 && _jumpTime == 0 || _jumpTime > 0)
        {
            _jumpTime += Time.fixedDeltaTime;
            _rb.MovePosition(_positionOnGround + JumpOffset);

            if (_jumpTime >= JumpDuration)
            {
                _jumpTime = 0;
            }
        }
    }

    void Move(float horizontal)
    {
        if (horizontal == 0)
        {
            return;
        }

        var directionAlongGround = Vector2.Perpendicular(_groundNormal) * -horizontal;
        var movement = directionAlongGround * Speed * Time.fixedDeltaTime;

        var (normal, point) = CastToGround(_rb.position + movement - JumpOffset);
        _groundNormal = normal;
        _positionOnGround = point;

        _rb.MovePosition(_positionOnGround + JumpOffset);
        _rb.MoveRotation(Vector2.SignedAngle(Vector2.up, _groundNormal));
    }

    void Flip(float horizontal)
    {
        var isFacingRight = horizontal > 0;
        var isFacingLeft = horizontal < 0;

        if (
            isFacingRight && transform.localScale.x < 0 ||
            isFacingLeft && transform.localScale.x > 0
        )
        {
            var s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }
    }

    (Vector2 normal, Vector2 point) CastToGround(Vector2 position)
    {
        // Если не поднять точку, то она пропустит землю,
        // если мы в неё немного провалились
        var pointAbove = position + _groundNormal;
        var collisions = new RaycastHit2D[1];

        int count =
            Physics2D
                .RaycastNonAlloc(pointAbove,
                -_groundNormal,
                collisions,
                // Расстояние обязательно, без него не работает маска
                // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                100f,
                MovementFilter.layerMask);

        if (count != 0)
        {
            return (collisions[0].normal, collisions[0].point);
        }

        return (_groundNormal, position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _groundNormal);
    }
}
