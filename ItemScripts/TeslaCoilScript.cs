using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace AbioticFactorValuables.ItemScripts
{
    public class TeslaCoilScript : Trap
    {
        public Material material;

        public Sound teslaActiveSound;

        public Sound teslaShockSound;

        public ParticleSystem teslaActiveParticles;

        public ParticleSystem teslaShockParticles;

        public Transform shockStartTransform;

        private Color emission;

        private float emissionValue = 0f;

        public override void Start()
        {
            base.Start();
            photonView = GetComponent<PhotonView>();
            physGrabObject = GetComponent<PhysGrabObject>();
            material.enableInstancing = true;
            teslaActiveParticles.gameObject.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            emission = Color.Lerp(Color.black, Color.white, emissionValue);
            material.SetColor("_EmissionColor", emission);
            teslaActiveSound.PlayLoop(physGrabObject.grabbed, 0.2f, 1f);
            if (physGrabObject.grabbed)
            {
                teslaActiveParticles.gameObject.SetActive(true);
                emissionValue += (Time.deltaTime * 5);
            }
            if (!physGrabObject.grabbed)
            {
                teslaActiveParticles.gameObject.SetActive(false);
                emissionValue -= Time.deltaTime;
            }
            emissionValue = Mathf.Clamp01(emissionValue);
        }

        public void TrapActivate()
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("TrapActivateRPC", RpcTarget.All);
            }
            else
            {
                TrapActivateRPC();
            }
        }

        [PunRPC]
        public void TrapActivateRPC()
        {
            if (physGrabObject.hasNeverBeenGrabbed)
                return;
            foreach (PlayerAvatar player in SemiFunc.PlayerGetAllPlayerAvatarWithinRange(10f, this.gameObject.transform.position, this.physGrabObject))
            {
                Vector3 normalized = (player.transform.position - shockStartTransform.position).normalized;
                if (Physics.Raycast(shockStartTransform.position, normalized, out RaycastHit hit, 15f, SemiFunc.LayerMaskGetShouldHits() - LayerMask.GetMask("PhysGrabObject"), QueryTriggerInteraction.Ignore))
                {
                    teslaShockSound.Play(this.gameObject.transform.position);
                    Debug.Log($"Raycast hit object: {hit.collider.gameObject.name} in {hit.collider.gameObject.transform.parent.name}");
                    Debug.Log($"Hit point at {hit.point.normalized}");
                    teslaShockParticles.gameObject.transform.rotation = Quaternion.LookRotation(hit.point.normalized);
                    teslaShockParticles.Play();
                    if (!(hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")))
                    {
                        continue;
                    }

                    player.playerHealth.Hurt(30, true);
                    if (player.playerHealth.health <= 0)
                        continue;
                    if (!player.isTumbling)
                    {
                        player.tumble.TumbleRequest(_isTumbling: true, _playerInput: false);
                        player.tumble.TumbleOverrideTime(2f);
                    }
                    player.tumble.ImpactHurtSet(2f, 0);
                    player.tumble.TumbleForce(normalized * 20f);
                    player.tumble.TumbleTorque(shockStartTransform.position.normalized);
                    if (!player.isLocal)
                        continue;
                    PlayerController playerController = PlayerController.instance;
                    playerController.ForceImpulse(normalized * 20f);
                }
            }
        }
    }
}
