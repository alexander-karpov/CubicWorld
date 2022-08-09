using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject Player;

    public float Offset;

    public float OffsetSmoothing;

    // Update is called once per frame
    void Update()
    {
        var target = transform.position;
        target.x = Player.transform.position.x;

        // Размещает игрока по y на позицию золотого сечения
        // var ty = Camera.main.ViewportToWorldPoint(new Vector3(0, 1 - 0.618f, 0)).y;
        // target.y -= ty - Player.transform.position.y;
        // if (Player.transform.localScale.x >= 0f)
        // {
        //     target.x += Offset;
        // }
        // else
        // {
        //     target.x -= Offset;
        // }
        var z = 7f;
        var zy = 4f;

        // transform.position =
        //     Vector3.Lerp(transform.position, target, OffsetSmoothing * Time.deltaTime);
        if (Mathf.Abs(transform.position.y - Player.transform.position.y) > zy)
        {
            var p = transform.position;
            p.y =
                Player.transform.position.y +
                (transform.position.y > Player.transform.position.y ? -zy : +zy);
            transform.position = p;
        }

        if (Mathf.Abs(transform.position.x - Player.transform.position.x) > z)
        {
            var p = transform.position;
            p.x = Player.transform.position.x + z;
            transform.position = p;
        }
    }
}
