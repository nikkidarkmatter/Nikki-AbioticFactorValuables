using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

namespace AbioticFactorValuables.ItemScripts
{
    public class ValuableGravityCube : MonoBehaviour
    {
        private PhysGrabObject physGrabObject;

        private PhotonView photonView;

        private void Update()
        {
            foreach (PhysGrabber item in physGrabObject.playerGrabbing)
            {
                if (physGrabObject.grabbedLocal)
                {
                    PlayerController.instance.AntiGravity(0.1f);
                }
                else
                {
                    return;
                }
            }
        }
        private void Start()
        {
            physGrabObject = GetComponent<PhysGrabObject>();
            photonView = GetComponent<PhotonView>();
            List<PhysGrabber> playerGrabbing = physGrabObject.playerGrabbing;
        }
    }
}
