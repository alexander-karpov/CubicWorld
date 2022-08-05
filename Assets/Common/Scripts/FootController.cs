using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubicWorld.Common
{
    public class FootController : MonoBehaviour
    {
        public Transform Body;

        public float StepDistance = 2f;

        public float Height = 2f;

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
        }

        // Update is called once per frame
        void Update()
        {
            if (_lerp < 1)
            {
                var pos = Vector2.Lerp(_oldPosition, _target, _lerp);

                Body.transform.position =
                    new Vector3((_staingFoot.position.x + _movingFoot.position.x) / 2f,
                        (_staingFoot.position.y + pos.y) / 2f + Height,
                        0f);

                pos.y += StepCurve.Evaluate(_lerp);
                _movingFoot.position = pos;

                _lerp += Time.deltaTime;
            }
            else
            {
                (_movingFoot, _staingFoot) = (_staingFoot, _movingFoot);

                var (n, p) =
                    CastToGround(new Vector2(_staingFoot.position.x + StepDistance,
                        Body.position.y));

                _target = p;
                _lerp = 0;
                _oldPosition = _movingFoot.position;
            }
        }

        (Vector2 normal, Vector2 point) CastToGround(Vector2 position)
        {
            var collisions = new RaycastHit2D[1];

            int count =
                Physics2D
                    .Raycast(position,
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

            return (Vector2.up, position);
        }
    }
}
