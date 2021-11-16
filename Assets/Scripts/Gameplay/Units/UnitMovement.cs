using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethods
{
    public class UnitMovement : MonoBehaviour
    {
        // movement speed units are 500 units per meter, so a speed of 600 is 1.2 m / s.
        // However, this will be implemented in UI
        public float MoveSpeed = 1.2f;
        // amount of time to accelerate to MoveSpeed, from a stop
        public float MoveAccelInterval = 1.0f;

        private Rigidbody rigidbody;

        public class MovingType
        {
            public enum MovingStatus : byte { navigation, disabled, displacing }
            public float[] MovingDrags = { 6.0f, 1000.0f, 0.0f };

            private MovingStatus _status;
            public MovingStatus Status
            {
                get => _status;
                set
                {
                    _status = value;
                    if (registeredRigidBody != null)
                    {
                        registeredRigidBody.drag = MovingDrags[(int)value];
                    }
                }
            }
            private Rigidbody registeredRigidBody;
            public MovingType(Rigidbody RegisteringBody)
            {
                registeredRigidBody = RegisteringBody;
                if (RegisteringBody != null)
                {
                    MovingDrags[0] = RegisteringBody.drag;
                }
            }
        }
        private MovingType movingType;
        private Vector3 moveTarget;

        public void DirectMove(Vector3 MoveTarget)
        {
            movingType.Status = MovingType.MovingStatus.navigation;
            moveTarget = MoveTarget;
        }
        public void DirectMove()
        {
            movingType.Status = MovingType.MovingStatus.navigation;
        }

        public void DisableMovement()
        {
            movingType.Status = MovingType.MovingStatus.disabled;
        }

        public void SetDisplacing(Vector3 DisplacementForce)
        {
            movingType.Status = MovingType.MovingStatus.displacing;
            rigidbody.AddForce(DisplacementForce);
        }

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                Debug.Log("Unit " + name + " has a movement component but no rigidbody!");
                Destroy(this);
            }
            else
            {
                movingType = new MovingType(rigidbody);
            }
        }

        private void FixedUpdate()
        {
            if (movingType.Status == MovingType.MovingStatus.navigation)
            {
                Vector3 MoveTargetUnit = (moveTarget - rigidbody.position);
                MoveTargetUnit.y = 0.0f;
                MoveTargetUnit = (MoveTargetUnit.sqrMagnitude > 0.001) ? MoveTargetUnit.normalized : Vector3.zero;
                if(MoveTargetUnit != Vector3.zero)
                {
                    float CurrentSpeed = rigidbody.velocity.XZMagnitude();
                    Vector3 CurrentVelocity = new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z);
                    float CurrDotTarget = CurrentVelocity.x * MoveTargetUnit.x + CurrentVelocity.z * MoveTargetUnit.z;

                    //Scalar force with ceiling
                    float ScalarForce = Mathf.Min(Mathf.Max(0.0f, Mathf.Min(CurrDotTarget, MoveSpeed)) * rigidbody.drag + MoveSpeed / MoveAccelInterval, MoveSpeed);

                    //Drag Correction so that unit can reach max speed
                    ScalarForce /= 1.0f - rigidbody.drag * Time.deltaTime;

                    //Scale force
                    ScalarForce *= rigidbody.mass * Time.deltaTime;

                    rigidbody.AddForce(MoveTargetUnit * ScalarForce);
                }
            }
        }
    }

}

