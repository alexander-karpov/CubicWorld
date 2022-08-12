using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace CubicWorld
{
    public class WalkController : MonoBehaviour
    {
        public Transform Body;

        public float StepDistance = 2f;

        public float Speed = 1f;

        public float LegLength = 1.9f;

        // Как стопа отрывается от земли во время шага
        public AnimationCurve FootRaising;

        [Range(0f, 1f)]
        public float FootRaisingFactor = 0;

        // Как масса тела снижает высоту корпуса после контакта
        public AnimationCurve MassPressure;

        [Range(0f, 1f)]
        public float MassPressureFactor = 0;

        // Фазы контакта проходят быстрее остальных
        public AnimationCurve ContactAcceleration;

        [Range(0f, 1f)]
        public float ContactAccelerationFactor;

        // Движение пятки постепенно ускоряется
        public AnimationCurve FootAcceleration;

        [Range(0f, 1f)]
        public float FootAccelerationFactor = 0;

        public Transform RightFoot;

        public Transform LeftFoot;

        public ContactFilter2D GroundFilter;

        Transform _movingFoot;

        Transform _staingFoot;

        Vector2 _target;

        Vector2 _oldPosition;

        float _time = 0;

        void Start()
        {
            RightFoot.position = Body.position;
            LeftFoot.position = Body.position;

            _movingFoot = RightFoot;
            _staingFoot = LeftFoot;
            _target = RightFoot.position;
            _oldPosition = RightFoot.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                Speed = 3f;
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                Speed = .2f;
            }
            else
            {
                Speed = 1;
            }

            var phase =
                Mathf.Lerp(_time, ContactAcceleration.Evaluate(_time), ContactAccelerationFactor);

            var centerOfMass = Vector2.Lerp(_oldPosition, _target, phase);

            // Body.transform.position =
            //     new Vector3((_staingFoot.position.x + _movingFoot.position.x) / 2f,
            //         (_staingFoot.position.y + pos.y) / 2f + Height,
            //         0f);
            // Ищем тут пересечение окружности радиусом LegLength с центром
            // в нижней ноге и вертикальной прямой на позиции midpoint.x
            var A =
                _staingFoot.position.y < centerOfMass.y
                    ? (Vector2) _staingFoot.position
                    : centerOfMass;
            var ANot =
                _staingFoot.position.y >= centerOfMass.y
                    ? (Vector2) _staingFoot.position
                    : centerOfMass;
            var midpoint = Vector2.Lerp(A, ANot, 0.5f).x;
            var x = A.x - midpoint;
            var a = Mathf.Sqrt(Mathf.Pow(LegLength, 2) - Mathf.Pow(x, 2));
            var C = new Vector2(midpoint, A.y + a);

            C.y += MassPressure.Evaluate(phase) * MassPressureFactor;
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
            var footTime =
                Mathf.Lerp(_time, FootAcceleration.Evaluate(_time), FootAccelerationFactor);
            var movingFootPos = Vector2.Lerp(_oldPosition, _target, footTime);
            movingFootPos.y += FootRaising.Evaluate(footTime) * FootRaisingFactor * LegLength;

            // movingFootPos.x =
            // Mathf
            //     .Lerp(FootAcceleration.Evaluate(_time),
            //     movingFootPos.x,
            //     FootAccelerationFactor);
            // movingFootPos.x = _oldPosition.x + FootAcceleration.Evaluate(_time);
            _movingFoot.position = movingFootPos;

            _time += Time.deltaTime * Speed;

            if (_time > 1)
            {
                (_movingFoot, _staingFoot) = (_staingFoot, _movingFoot);

                var (n, p) = FindNextStepPoint(1);

                _target = p;
                _time = 0;
                _oldPosition = _movingFoot.position;
            }

            if (Input.GetAxisRaw("Jump") > 0.1)
            {
                _movingFoot.position = new Vector2(-60, -10);
                _staingFoot.position = new Vector2(-60, -10);
                _target = new Vector2(-60, -10);
                _oldPosition = _movingFoot.position;
            }
        }

        (Vector2 normal, Vector2 point) FindNextStepPoint(float horizontal)
        {
            Assert.IsTrue(horizontal == 1 || horizontal == -1);

            var midpoint = Vector2.Lerp(LeftFoot.position, RightFoot.position, 0.5f);

            var collisions = new RaycastHit2D[1];
            var xBound = _staingFoot.position.x + (StepDistance * horizontal);

            // Ищем препятствия впереди
            int count =
                Physics2D
                    .Raycast(new Vector2(_staingFoot.position.x, midpoint.y + LegLength),
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
                    xBound = collisions[0].point.x - (StepDistance / 5 * horizontal);
                }
            }

            count =
                Physics2D
                    .Raycast(new Vector2(xBound, midpoint.y + LegLength),
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
    }
}
