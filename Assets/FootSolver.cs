using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSolver : MonoBehaviour
{
    public Transform Body;

    public float FootSpacing;

    public LayerMask GroundLayer;

    // Update is called once per frame
    void Update()
    {
        var pos = Body.position + (Vector3.right * FootSpacing);
        RaycastHit2D info = Physics2D.Raycast(pos, Vector2.down, 10, GroundLayer.value);

        if (info)
        {
            transform.position = info.point;
        }
    }
}
