using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace AbioticFactorValuables.ItemScripts
{
    public class RubberBallScript : MonoBehaviour
    {
        public Sound soundBoing;

        private Rigidbody rb;

        private PhotonView photonView;

        private PhysGrabObject physGrabObject;

        public HurtCollider hurtCollider;

        public Transform hurtTransform;

        private float hurtColliderTime;

        private Vector3 prevPosition;

        private float bounceTimer;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            physGrabObject = GetComponent<PhysGrabObject>();
            hurtCollider = GetComponentInChildren<HurtCollider>();
            hurtCollider.gameObject.SetActive(false);
            photonView = GetComponent<PhotonView>();
        }

        private void Update()
        {
            if (bounceTimer >= 0f)
            {
                bounceTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            Vector3 vector = (rb.position - prevPosition) / Time.fixedDeltaTime;
            Vector3 normalized = (rb.position - prevPosition).normalized;
            prevPosition = rb.position;
            if (hurtColliderTime > 0f)
            {
                hurtTransform.forward = normalized;
                if (!hurtCollider.gameObject.activeSelf)
                {
                    hurtCollider.gameObject.SetActive(value: true);
                    float num = vector.magnitude * 2f;
                    if (num > 50f)
                    {
                        num = 50f;
                    }
                    hurtCollider.physHitForce = num / 2;
                    hurtCollider.physHitTorque = num / 2;
                    hurtCollider.enemyHitForce = num / 2;
                    hurtCollider.enemyHitTorque = num / 2;
                    hurtCollider.playerTumbleForce = num;
                    hurtCollider.playerTumbleTorque = num;
                }
                hurtColliderTime -= Time.fixedDeltaTime;
            }
            else if (hurtCollider.gameObject.activeSelf)
            {
                hurtCollider.gameObject.SetActive(value: false);
            }
        }

        private void BallBounce()
        {
            if (!physGrabObject.hasNeverBeenGrabbed)
            {
                if (!physGrabObject.grabbed && bounceTimer <= 0f)
                {
                    rb.AddForce(Vector3.up * Random.Range(1f, 3f), ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * Random.Range(1f, 3f), ForceMode.Impulse);
                    rb.AddForce(Random.insideUnitCircle * Random.Range(1f, 3f), ForceMode.Impulse);
                    bounceTimer = Random.Range(0.5f, 1.5f);
                }
            }
        }

        public void Boing()
        {
            BallBounce();
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("BoingRPC", RpcTarget.All);
            }
            else
            {
                BoingRPC();
            }
        }
        [PunRPC]
        public void BoingRPC()
        {
            soundBoing.Play(base.transform.position);
        }
    }
}
