using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using REPOLib.Modules;

namespace AbioticFactorValuables.ItemScripts
{
    class VendingMachineScript : MonoBehaviour
    {
        private PhysGrabObject physGrabObject;

        private PhotonView photonView;

        public Animator VendingMachineAnim;

        public GameObject saltzItem;

        public Transform spawnPosTransform;

        public Sound itemDispense;

        public Sound itemDispenseFail;

        private bool hasDispensed = false;

        private bool buttonPressed = false;
        private void Start()
        {
            physGrabObject = GetComponent<PhysGrabObject>();
            photonView = GetComponent<PhotonView>();
            List<PhysGrabber> playerGrabbing = physGrabObject.playerGrabbing;
        }
        public void VendItemPlay()
        {
            if (physGrabObject.grabbedLocal || physGrabObject.grabbed)
            {
                if (!hasDispensed && !buttonPressed)
                {
                    photonView.RPC("SetAnimationState", RpcTarget.All, true);
                    VendingMachineAnim.enabled = true;
                }
                if (hasDispensed || buttonPressed)
                {
                    photonView.RPC("PlaySoundRPC", RpcTarget.All, true);
                }
            }
        }
        public void SpawnItem()
        {
            Vector3 spawnPos = spawnPosTransform.position;
            {
                if (SemiFunc.IsMultiplayer() && SemiFunc.IsMasterClient())
                {
                    Valuables.SpawnValuable(AbioticFactorValuables.saltzItemRef, spawnPos, Quaternion.identity);
                }
                else if (SemiFunc.IsNotMasterClient())
                {
                    // Do nothing here.
                }
                else
                {
                    print($"Locally spawning Saltz at {spawnPos}.");
                    Instantiate(saltzItem, spawnPos, Quaternion.identity);
                }
                hasDispensed = true;
            }
        }
        public void OutOfStockItem()
        {
            itemDispenseFail.Play(physGrabObject.centerPoint);
        }
        [PunRPC]
        public void SetAnimationState(bool vendanim)
        {
            VendingMachineAnim.SetBool("active", vendanim);
            buttonPressed = true;
            itemDispense.Play(physGrabObject.centerPoint);
        }
        [PunRPC]
        public void PlaySoundRPC(bool playsound)
        {
            itemDispenseFail.Play(physGrabObject.centerPoint);
        }
    }
}
