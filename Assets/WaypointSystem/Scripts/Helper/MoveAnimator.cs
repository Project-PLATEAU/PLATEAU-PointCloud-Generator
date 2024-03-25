using UnityEngine;
using UnityEngine.AI;

namespace WaypointSystem
{
    using DG.Tweening;

    public class MoveAnimator : MonoBehaviour
    {
        private splineMove sMove;
        private NavMeshAgent nAgent;
        private Animator animator;
        private float lastRotY;

        void Start()
        {
            animator = GetComponentInChildren<Animator>();

            sMove = GetComponent<splineMove>();
            if (!sMove)
                nAgent = GetComponent<NavMeshAgent>();

        }


        void OnAnimatorMove()
        {
            float speed = 0f;
            float angle = 0f;

            if (sMove)
            {
                speed = (sMove.tween == null || !sMove.tween.IsActive() || !sMove.tween.IsPlaying()) ? 0f : sMove.speed;
                angle = (transform.eulerAngles.y - lastRotY) * 10;
                lastRotY = transform.eulerAngles.y;
            }
            else
            {
                speed = nAgent.velocity.magnitude;
                Vector3 velocity = Quaternion.Inverse(transform.rotation) * nAgent.desiredVelocity;
                angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;
            }

            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", angle, 0.15f, Time.deltaTime);
        }
    }
}