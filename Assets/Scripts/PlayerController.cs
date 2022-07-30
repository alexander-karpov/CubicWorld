using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public ContactFilter2D MovementFilter;

    [SerializeField]
    public float Speed = 5f;

    // public Transform groundCheck;
    // [SerializeField]
    // public LayerMask groundLayer;
    // public SpriteRenderer spriteRenderer;
    // [SerializeField]
    // private Bounds _characterBounds;
    Rigidbody2D rb;

    RaycastHit2D[] moveCollisions = new RaycastHit2D[3];

    // bool isFacingRight = true;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal == 0)
        {
            return;
        }

        Move2 (horizontal);

        // if (!IsGrounded)
        // {
        //     Fall();
        // }
        // rb
        //     .MovePosition(rb.position +
        //     Vector2.right * Time.fixedDeltaTime * Input.GetAxisRaw("Horizontal") * Speed);
    }

    // // private bool IsGrounded
    // // {
    // //     get
    // //     {
    // //         return Physics2D.Overlap
    // //     }
    // // }
    // void Flip()
    // {
    //     isFacingRight = !isFacingRight;
    //     spriteRenderer.flipX = isFacingRight;
    // }
    // void Fall()
    // {
    //     rb.MovePosition(rb.position + Vector2.down * Time.fixedDeltaTime * 5);
    // }
    // void OnDrawGizmos()
    // {
    //     var cf = new ContactFilter2D();
    //     cf.SetLayerMask(groundLayer);
    //     rb.OverlapCollider(cf, )
    //     // Draw a yellow sphere at the transform's position
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawRay(transform.position, Vector3.right);
    // }
    // void Move(float horizontal)
    // {
    //     var direction = Vector2.down;
    //     int count = rb.Cast(direction, MovementFilter, moveCollisions, Speed * Time.fixedDeltaTime);
    //     if (count == 0)
    //     {
    //         rb.MovePosition(rb.position + direction * Speed * Time.fixedDeltaTime);
    //         return;
    //     }
    //     var directionAlongCollision = Vector2.Perpendicular(moveCollisions[0].normal) * -horizontal;
    //     count =
    //         rb
    //             .Cast(directionAlongCollision,
    //             MovementFilter,
    //             moveCollisions,
    //             Speed * Time.fixedDeltaTime);
    //     if (count == 0)
    //     {
    //         rb.MovePosition(rb.position + directionAlongCollision * Speed * Time.fixedDeltaTime);
    //         return;
    //     }
    //     directionAlongCollision = Vector2.Perpendicular(moveCollisions[0].normal) * -horizontal;
    //     count =
    //         rb
    //             .Cast(directionAlongCollision,
    //             MovementFilter,
    //             moveCollisions,
    //             Speed * Time.fixedDeltaTime);
    //     if (count == 0)
    //     {
    //         rb.MovePosition(rb.position + directionAlongCollision * Speed * Time.fixedDeltaTime);
    //         return;
    //     }
    // }
    void Move2(float horizontal)
    {
        var newPosition = rb.position;

        // var lookingAhead = newPosition + new Vector2(horizontal, 10f) * Speed * Time.fixedDeltaTime;
        var lookingAhead = rb.position + Vector2.up * 3f;

        // // var collisions = new RaycastHit2D[1];
        int count =
            Physics2D
                .RaycastNonAlloc(lookingAhead,
                Vector2.down,
                moveCollisions,
                // Расстояние обязательно, без него не работает маска
                // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                10f,
                MovementFilter.layerMask);
        if (count == 0)
        {
            return;
        }
        var ground = moveCollisions[0];
        var dirAlongGround = Vector2.Perpendicular(ground.normal) * -horizontal;
        newPosition += dirAlongGround * Speed * Time.fixedDeltaTime;

        // count = rb.Cast(Vector2.down, MovementFilter, moveCollisions);
        // if (count != 0)
        // {
        //     newPosition += Vector2.down * moveCollisions[0].distance;
        // }
        count =
            Physics2D
                .RaycastNonAlloc(newPosition + Vector2.up * 3f,
                Vector2.down,
                moveCollisions,
                // Расстояние обязательно, без него не работает маска
                // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                10f,
                MovementFilter.layerMask);

        rb.MoveRotation(Vector2.Angle(Vector2.up, moveCollisions[0].normal));
        rb.MovePosition(moveCollisions[0].point);
    }

    void OnDrawGizmos()
    {
        // foreach (var c in moveCollisions)
        // {
        //     Gizmos.DrawRay(transform.position, c.normal);
        // }
        var horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0)
        {
            var dir = new Vector2(horizontal, 0);
            Gizmos.color = Color.red;

            Gizmos.DrawRay(transform.position, dir);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, moveCollisions[0].normal);

            Gizmos.color = Color.green;
            Gizmos
                .DrawRay(transform.position,
                Vector2.Perpendicular(moveCollisions[0].normal) * -horizontal);
        }
    }
}
