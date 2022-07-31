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
        var ty = Camera.main.ViewportToWorldPoint(new Vector3(0, 1 - 0.618f, 0)).y;
        target.y -= ty - Player.transform.position.y;

        if (Player.transform.localScale.x >= 0f)
        {
            target.x += Offset;
        }
        else
        {
            target.x -= Offset;
        }

        transform.position =
            Vector3.Lerp(transform.position, target, OffsetSmoothing * Time.deltaTime);
    }
}
