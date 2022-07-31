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
    public float JumpHeight = 1f;

    [SerializeField]
    public AnimationCurve jumpCurve;

    SpriteRenderer _spriteRenderer;

    Rigidbody2D _rb;

    RaycastHit2D[] moveCollisions = new RaycastHit2D[3];

    Vector2 _groundNormal = Vector2.up;

    Vector2 _positionOnGround;

    float _heightAboveGround = 0f;

    float _jumpDuration = 0f;

    // bool isFacingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        _rb = this.GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var jump = Input.GetAxisRaw("Jump");

        if (horizontal != 0)
        {
            Move (horizontal);
            Flip(horizontal > 0);
        }

        if (jump > 0 && _jumpDuration == 0)
        {
            _jumpDuration += Time.fixedDeltaTime;
            _heightAboveGround = jumpCurve.Evaluate(_jumpDuration);

            _rb.MovePosition(_positionOnGround + Vector2.up * _heightAboveGround);
        }

        if (_jumpDuration > 0)
        {
            _jumpDuration += Time.fixedDeltaTime;
            _heightAboveGround = jumpCurve.Evaluate(_jumpDuration);

            _rb.MovePosition(_positionOnGround + Vector2.up * _heightAboveGround);

            if (_jumpDuration >= 1f)
            {
                _jumpDuration = 0;
            }
        }

        // if (!IsGrounded)
        // {
        //     Fall();
        // }
        // rb
        //     .MovePosition(rb.position +
        //     Vector2.right * Time.fixedDeltaTime * Input.GetAxisRaw("Horizontal") * Speed);
    }

    void Flip(bool isFacingRight)
    {
        if (
            isFacingRight && transform.localScale.x < 0 ||
            !isFacingRight && transform.localScale.x > 0
        )
        {
            var s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }
    }

    void Move(float horizontal)
    {
        var directionAlongGround = Vector2.Perpendicular(_groundNormal) * -horizontal;
        var movement = directionAlongGround * Speed * Time.fixedDeltaTime;

        var (normal, point, distance) = CastDown(_rb.position + movement, MovementFilter);
        _groundNormal = normal;
        _positionOnGround = point;

        _rb.MovePosition(point + Vector2.up * _heightAboveGround);
        _rb.MoveRotation(Vector2.SignedAngle(Vector2.up, _groundNormal));
    }

    static (Vector2 normal, Vector2 point, float distance)
    CastDown(Vector2 position, ContactFilter2D contactFilter)
    {
        // Если не поднять точку, то она пропустит землю,
        // если мы в неё немного провалились
        var pointAbove = position + Vector2.up * 5f;
        var collisions = new RaycastHit2D[1];

        int count =
            Physics2D
                .RaycastNonAlloc(pointAbove,
                Vector2.down,
                collisions,
                // Расстояние обязательно, без него не работает маска
                // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                100f,
                contactFilter.layerMask);

        if (count != 0)
        {
            return (collisions[0].normal, collisions[0].point, collisions[0].distance);
        }

        return (Vector2.up, position, 0f);
    }
}
