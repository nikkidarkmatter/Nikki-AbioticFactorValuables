using Photon.Pun;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace AbioticFactorValuables.ItemScripts
{
    public class ValuableAnvil : MonoBehaviour
    {
        public GameObject hurtCollider;

        public Sound anvilHitSound;

        private Rigidbody rb;

        private PhysGrabObject physGrabObject;

        private PhotonView photonView;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            physGrabObject = GetComponent<PhysGrabObject>();
            photonView = GetComponent<PhotonView>();
            hurtCollider.SetActive(false);
        }
        private void Update()
        {
            if (rb != null)
            {
                float velocity = rb.velocity.magnitude;
                if (physGrabObject.grabbed)
                {
                     hurtCollider.SetActive(false);
                }
                if (physGrabObject.impactDetector.inCart)
                {
                     hurtCollider.SetActive(false);
                }
                if (velocity >= 3.5f && !physGrabObject.grabbed && !physGrabObject.impactDetector.inCart)
                {
                     hurtCollider.SetActive(true);
                }
            }          
        }

        public void OnAnvilHit()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("AnvilPlaySoundRPC", RpcTarget.All);
                }
                else
                {
                    AnvilPlaySoundRPC();
                }
            }
        }

        [PunRPC]
        public void AnvilPlaySoundRPC()
        {
            if (rb != null)
            {
                anvilHitSound.Play(rb.position, 0.9f, 0.9f);
            }
            print("Anvil hit!");
        }
    }
}
