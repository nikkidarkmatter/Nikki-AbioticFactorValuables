using Photon.Pun;
using UnityEngine;

namespace AbioticFactorValuables.ItemScripts
{
    public class ValuablePitchfork : MonoBehaviour
    {
        public GameObject hurtCollider;

        private PhysGrabObjectImpactDetector impactDetector;

        private Rigidbody rb;

        private PhysGrabObject physGrabObject;

        private PhotonView photonView;

        internal int breakLevelHeavy = 0;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            physGrabObject = GetComponent<PhysGrabObject>();
            photonView = GetComponent<PhotonView>();
            impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
            hurtCollider.SetActive(false);
        }
        private void Update()
        {
            if (rb != null)
            {
                float velocity = rb.velocity.magnitude;
                if (!physGrabObject.impactDetector.inCart)
                {
                    if (velocity >= 0.75f || (physGrabObject.grabbed && velocity >= 0.25f))
                    {
                        hurtCollider.SetActive(true);
                    }
                }
                if (physGrabObject.impactDetector.inCart)
                {
                    hurtCollider.SetActive(false);
                }
                else
                {
                    hurtCollider.SetActive(false);
                }
            }          
        }

        public void OnHitEnemy()
        {
            Vector3 contactPoint = hurtCollider.transform.position;
            impactDetector.BreakMedium(contactPoint);
        }
    }
}
