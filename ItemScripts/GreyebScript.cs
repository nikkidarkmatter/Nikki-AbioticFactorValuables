using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace AbioticFactorValuables.ItemScripts
{
    public class ValuableGreyeb : MonoBehaviour
    {
        public GreyebDetectionCone cone;

        public Transform greyebEye;

        // public LineRenderer debugLine;

        private PhysGrabObject physGrabObject;

        private PhotonView photonView;

        public bool visionOverride = false;

        public float visionOverrideTimer = 120f;

        private float visionTimer = 0.5f;

        private void Start()
        {
            physGrabObject = GetComponent<PhysGrabObject>();
            photonView = GetComponent<PhotonView>();
            cone = GetComponentInChildren<GreyebDetectionCone>();
            cone.gameObject.SetActive(true);
            // debugLine = GetComponentInChildren<LineRenderer>(true);
        }

        private void Update()
        {
            visionTimer -= Time.deltaTime;
            if (visionOverride && visionOverrideTimer > 0f)
            {
                visionOverrideTimer -= Time.deltaTime;
                cone.gameObject.SetActive(false);
            }
            if (visionTimer <= 0f && !visionOverride)
            {
                if (cone.playerList.Count > 0)
                {
                    CheckForVision();
                }
                visionTimer = 0.5f;
            }
            if (visionOverride && visionOverrideTimer <= 0f)
            {
                visionOverride = false;
                visionOverrideTimer = 120f;
                cone.gameObject.SetActive(true);
            }

        }

        public void CheckForVision()
        {
            visionTimer = 0.5f;
            if (physGrabObject != null && physGrabObject.hasNeverBeenGrabbed && cone.playerList.Count > 0)
            {
                foreach (PlayerAvatar targetPlayer in cone.playerList)
                {                  
                    if (greyebEye != null && targetPlayer.transform.position != null && Physics.Raycast(greyebEye.transform.position, (targetPlayer.transform.position - greyebEye.transform.position).normalized, out RaycastHit hit, 20f, SemiFunc.LayerMaskGetVisionObstruct()))
                    {
                        // debugLine.gameObject.SetActive(true);
                        // debugLine.SetPositions(new Vector3[2]{ greyebEye.transform.position, hit.point });
                        if (hit.collider.tag == "Player" || hit.collider.gameObject.layer == 26)
                        {
                            OverrideVision();
                        }
                    }                   
                }
                if (visionOverride)
                {
                    cone.playerList.Clear();
                }
            }
        }
        public void OverrideVision()
        {
            if (physGrabObject.hasNeverBeenGrabbed && cone.playerList.Count > 0)
            {
                foreach (PlayerAvatar targetPlayer in cone.playerList)
                {
                    SemiFunc.PlayerEyesOverride(targetPlayer, greyebEye.transform.position, 0.1f, base.gameObject);
                    if (targetPlayer.isLocal)
                    {
                        Vector3 vector = targetPlayer.localCameraPosition - greyebEye.transform.position;
                        float num = Vector3.Dot(Vector3.down, vector);
                        float strengthNoAim = 10f;
                        if (num > 0.9f)
                        {
                            strengthNoAim = 5f;
                        }
                        CameraAim.Instance.AimTargetSoftSet(greyebEye.transform.position, 0.5f, 3f, strengthNoAim, base.gameObject, 90);
                        CameraGlitch.Instance.PlayLong();
                    }
                }
                visionOverride = true;
                visionOverrideTimer = 120f;
            }
        }
    }

    public class GreyebDetectionCone : MonoBehaviour
    {
        public ValuableGreyeb greyeb;

        public List<PlayerAvatar> playerList = new List<PlayerAvatar>();


        private void Start()
        {
            greyeb = GetComponentInParent<ValuableGreyeb>();
        }

        private void Update()
        {
            if (greyeb != null && playerList.Count > 0)
            {

            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!playerList.Equals(other.GetComponentInParent<PlayerAvatar>()))
                {
                    playerList.Add(other.GetComponentInParent<PlayerAvatar>());
                }
                if (greyeb != null && playerList.Count > 0 && !greyeb.visionOverride)
                {
                    greyeb.CheckForVision();
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (playerList.Count > 0)
                {
                    playerList.Remove(other.GetComponentInParent<PlayerAvatar>());
                }
            }
        }
    }
}
