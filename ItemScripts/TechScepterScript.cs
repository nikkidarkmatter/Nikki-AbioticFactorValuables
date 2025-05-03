using UnityEngine;
using Photon.Pun;

namespace AbioticFactorValuables.ItemScripts
{
    public class TechScepterScript : MonoBehaviour
    {
        public Animator animator;
        private PhysGrabObject physGrabObject;

        private PhotonView photonView;

        private bool isGrabbed = false;
        private bool wasGrabbedOnPreviousUpdate;

        private void Awake()
        {
            physGrabObject = GetComponent<PhysGrabObject>();
            photonView = GetComponent<PhotonView>();
        }

        private void Update()
        {
            if (physGrabObject != null)
            {
                isGrabbed = physGrabObject.grabbed;
            }

            if (isGrabbed != wasGrabbedOnPreviousUpdate)
            {
                photonView.RPC("SetAnimationState", RpcTarget.All, isGrabbed);
            }

            wasGrabbedOnPreviousUpdate = isGrabbed;
        }

        [PunRPC]
        public void SetAnimationState(bool grab)
        {
            if (grab)
            {
                animator.SetBool("grabbed", true);
            }
            else
            {
                animator.SetBool("grabbed", false);
            }
        }
    }
}
