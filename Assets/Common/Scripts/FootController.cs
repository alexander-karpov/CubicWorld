using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace CubicWorld.Common
{
    public class FootController : MonoBehaviour
    {
        public Transform Body;

        public float StepDistance = 2f;
        public float Speed = 1f;

        public float LegLength = 1f;

        public AnimationCurve StepCurve;

        public Transform RightFoot;

        public Transform LeftFoot;

        public ContactFilter2D GroundFilter;

        Transform _movingFoot;

        Transform _staingFoot;

        Vector2 _target;

        Vector2 _oldPosition;

        float _lerp = 0;

        void Start()
        {
            RightFoot.position = Body.position;
            LeftFoot.position = Body.position;

            _movingFoot = RightFoot;
            _staingFoot = LeftFoot;
            _target = RightFoot.position;
            _oldPosition = RightFoot.position;
        }

        Vector2 _pos;

        // Update is called once per frame
        void Update()
        {
            if (_lerp < 1)
            {
                if (Input.GetAxisRaw("Horizontal") > 0) {
                    Speed = 3;
                } else if (Input.GetAxisRaw("Horizontal") < 0) {
                    Speed = .1f;
                }
                else {
                          Speed = 1;
                }


                var pos = Vector2.Lerp(_oldPosition, _target, _lerp);
                _pos = pos;

                // Body.transform.position =
                //     new Vector3((_staingFoot.position.x + _movingFoot.position.x) / 2f,
                //         (_staingFoot.position.y + pos.y) / 2f + Height,
                //         0f);

                // Ищем тут пересечение окружности радиусом LegLength с центром
                // в нижней ноге и вертикальной прямой на позиции midpoint.x
                var A = _staingFoot.position.y < pos.y ? (Vector2)_staingFoot.position : pos ;
                var ANot = _staingFoot.position.y >= pos.y ? (Vector2)_staingFoot.position : pos ;
                var midpoint = Vector2.Lerp(A, ANot, 0.5f).x;
                var x = A.x - midpoint;
                var a = Mathf.Sqrt(Mathf.Pow(LegLength, 2) - Mathf.Pow(x, 2));
                var C = new Vector2(midpoint, A.y + a);

                Body.position = C;

                // Body.transform.position = Vector3.MoveTowards(
                //     Body.transform.position,
                //     new Vector3(
                //         (_staingFoot.position.x + _movingFoot.position.x) / 2f,
                //         (_staingFoot.position.y + pos.y) / 2f + Height,
                //         0
                //     ),
                //     Time.deltaTime
                // );

                pos.y += StepCurve.Evaluate(_lerp);
                _movingFoot.position = pos;


                _lerp += Time.deltaTime * Speed;
            }
            else
            {
                (_movingFoot, _staingFoot) = (_staingFoot, _movingFoot);

                var (n, p) = FindNextStepPoint(1);

                _target = p;
                _lerp = 0;
                _oldPosition = _movingFoot.position;
            }


            if (Input.GetAxisRaw("Jump") > 0.1) {
                _movingFoot.position = new Vector2(-60, -10);
                _staingFoot.position = new Vector2(-60, -10);
                _target = new Vector2(-60, -10);
                _oldPosition = _movingFoot.position;
            }
        }

        (Vector2 normal, Vector2 point) FindNextStepPoint(float horizontal)
        {
            Assert.IsTrue(horizontal is 1 or -1);

            var midpoint = Vector2.Lerp(LeftFoot.position, RightFoot.position, 0.5f);

            var collisions = new RaycastHit2D[1];
            var xBound = _staingFoot.position.x + (StepDistance * horizontal);

            // Ищем препятствия впереди
            int count =
                Physics2D
                    .Raycast(new Vector2(_staingFoot.position.x,midpoint.y + LegLength),
                    new Vector2(horizontal, 0),
                    GroundFilter,
                    collisions,
                    // Расстояние обязательно, без него не работает маска
                    // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                    StepDistance);

            if (count != 0)
            {
                // Если угол поверхности не слишком большой
                if (Vector2.Angle(Vector2.up, collisions[0].normal) < 45)
                {
                    return (collisions[0].normal, collisions[0].point);
                }
                else
                {
                    // Чуть-чуть отступаем от стены чтобы не ставить ногу прямо в угол
                    xBound = collisions[0].point.x - (StepDistance/5*horizontal);
                }

            }

            count =
                Physics2D
                    .Raycast(new Vector2(xBound,
                       midpoint.y + LegLength),
                    Vector2.down,
                    GroundFilter,
                    collisions,
                    // Расстояние обязательно, без него не работает маска
                    // https://answers.unity.com/questions/1699320/why-wont-physicsraycastnonalloc-mask-layers-proper.html
                    1000f);

            if (count != 0)
            {
                return (collisions[0].normal, collisions[0].point);
            }

            return (Vector2.up, _staingFoot.position);
        }


        void UpdateBodyPosition()
        {
          var  midpointX = (_staingFoot.position.x + _movingFoot.position.x) / 2f;

           var x2 = Mathf.Pow(midpointX - _staingFoot.position.x, 2);
           var y = Mathf.Sqrt(Mathf.Pow(LegLength, 2) - x2);

           Body.position = new Vector3(midpointX, y);
        }

        void OnDrawGizmos() {
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawSphere(LeftFoot.position, 0.1f);

            // Gizmos.color = Color.green;
            // Gizmos.DrawSphere(RightFoot.position, 0.1f);

            // var A = _staingFoot.position;
            // var B = Vector2.Lerp(A, _pos, 0.5f);
            // var c = Mathf.Abs(A.x - B.x);
            // var b = LegLength;
            // var a = Mathf.Sqrt(Mathf.Pow(b, 2) - Mathf.Pow(c, 2));
            // var C = new Vector2(B.x, B.y + a);

            // Gizmos.color = Color.red;
            // Gizmos.DrawSphere(A, 0.1f);

            // Gizmos.color = Color.green;
            // Gizmos.DrawSphere(B, 0.1f);

            // Gizmos.color = Color.yellow;
            // Gizmos.DrawSphere(C, 0.1f);
        }


        // Vector2 SolveTridPointOfTriangle(Vector2 A, Vector2 C, float a, float c)
        // {
        //     // http://mathhelpplanet.com/viewtopic.php?f=28&t=22911
        //     var (x1, y1, x2, y2) = (A.x, A.y, C.x, C.y);
        //     var x1Pow2 = Mathf.Pow(x1, 2);
        //     var x1Pow3 = Mathf.Pow(x1, 3);
        //     var x2Pow2 = Mathf.Pow(x2, 2);
        //     var y1Pow2 = Mathf.Pow(y1, 2);
        //     var y1Pow3 = Mathf.Pow(y1, 3);
        //     var y2Pow2 = Mathf.Pow(y2, 2);
        //     var y2Pow3 = Mathf.Pow(y2, 3);
        //     var aPow2 = Mathf.Pow(a, 2);
        //     var cPow2 = Mathf.Pow(c, 2);

        //     var x = (1 / 2) * ((y1 - y2) * Mathf.Sqrt(-(-x1Pow2 + 2 * x2 * x1 - x2Pow2 + (-c + a - y1 + y2) * (-c + a + y1 - y2)) * (-x1Pow2 + 2 * x2 * x1 - x2Pow2 + (c + a - y1 + y2) * (c + a + y1 - y2)) * Mathf.Pow(x1 - x2, 2)) + (x1Pow3 - x1Pow2 * x2 + (y2Pow2 - 2 * y1 * y2 - cPow2 + y1Pow2 + aPow2 - x2Pow2) * x1 - x2 * (aPow2 - cPow2 - x2Pow2 - y2Pow2 + 2 * y1 * y2 - y1Pow2)) * (x1 - x2)) / ((x1 - x2) * (x1Pow2 - 2 * x2 * x1 + x2Pow2 +  Mathf.Pow(y1 - y2, 2) ));
        //     var y = (-Mathf.Sqrt(-(-x1Pow2 + 2 * x2 * x1 - x2Pow2 + (-c + a - y1 + y2) * (-c + a + y1 - y2)) * (-x1Pow2 + 2 * x2 * x1 - x2Pow2 + (c + a - y1 + y2) * (c + a + y1 - y2)) * Mathf.Pow(x1 - x2,2)) + y1Pow3 - y1Pow2 * y2 + (aPow2 + x1Pow2 - cPow2 + x2Pow2 - 2 * x2 * x1 - y2Pow2) * y1 + y2Pow3 + (x2Pow2 - 2 * x2 * x1 + cPow2 - aPow2 + x1Pow2) * y2) / (2 * y1Pow2 - 4 * y1 * y2 + 2 * y2Pow2 + 2 * Mathf.Pow(x1 - x2, 2));

        //     return new Vector2(x, y);
        // }
    }
}
