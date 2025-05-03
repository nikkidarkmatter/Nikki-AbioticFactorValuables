using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System.Collections.Generic;
using AbioticFactorValuables.MiscScripts;
using System;
using Unity.VisualScripting;

namespace AbioticFactorValuables.ItemScripts
{
    public class ItemSlushieBomb : MonoBehaviour
    {
        public Color blinkColor;

        public UnityEvent onDetonate;

        private ItemToggle itemToggle;

        private ItemAttributes itemAttributes;

        internal bool isActive;

        private PhotonView photonView;

        private PhysGrabObjectImpactDetector physGrabObjectImpactDetector;

        private ItemEquippable itemEquippable;

        private Vector3 grenadeStartPosition;

        private Quaternion grenadeStartRotation;

        private PhysGrabObject physGrabObject;

        [FormerlySerializedAs("isThiefGrenade")]
        [HideInInspector]
        public bool isSpawnedGrenade;

        public GameObject throwLine;

        private Rigidbody rb;

        private TrailRenderer throwLineTrail;

        public Transform freezeExplosion;

        public GameObject primeParticle;

        public Sound primeSound;

        public Sound bombExplosion;

        public Sound bombFreeze;

        private void Start()
        {
            itemEquippable = GetComponent<ItemEquippable>();
            itemToggle = GetComponent<ItemToggle>();
            itemAttributes = GetComponent<ItemAttributes>();
            photonView = GetComponent<PhotonView>();
            physGrabObjectImpactDetector = GetComponent<PhysGrabObjectImpactDetector>();
            grenadeStartPosition = base.transform.position;
            grenadeStartRotation = base.transform.rotation;
            physGrabObject = GetComponent<PhysGrabObject>();
            rb = GetComponent<Rigidbody>();
            freezeExplosion = GetComponentInChildren<FreezeExplosion>().transform;
            freezeExplosion.gameObject.SetActive(false);
            primeParticle.gameObject.SetActive(false);
            throwLineTrail = throwLine.GetComponent<TrailRenderer>();
        }

        private void Update()
        {
            if (itemEquippable.isEquipped)
            {
                if (isActive)
                {
                    isActive = false;
                    itemToggle.ToggleItem(false);
                    throwLineTrail.emitting = false;
                    primeParticle.SetActive(false);
                }
                return;
            }
            if (isActive)
            {
                float value = Mathf.PingPong(Time.time * 8f, 1f);
                Color value2 = blinkColor * Mathf.LinearToGammaSpace(value);
            }
            if (isActive && rb.velocity.magnitude >= 0.1f)
            {
                throwLineTrail.emitting = true;
            }
            if (!isActive || rb.velocity.magnitude < 0.1f)
            {
                throwLineTrail.emitting = false;
            }
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            if (itemToggle.toggleState && !isActive)
            {
                isActive = true;
                BombPrimed();
            }
        }

        private void BombPrimed()
        {
            if (SemiFunc.IsMasterClient())
            {
                photonView.RPC("BombPrimedRPC", RpcTarget.All);
            }
            else
            {
                BombPrimedRPC();
            }
        }

        public void BombExplode()
        {
            if (!isActive)
            {
                return;
            }
            bombExplosion.Play(base.transform.position);
            GameObject obj = PhotonNetwork.Instantiate("Freeze Explosion", base.transform.position, base.transform.rotation);
            obj.transform.parent = null;
            obj.SetActive(true);
            physGrabObjectImpactDetector.DestroyObject(obj);
            if (SemiFunc.IsMasterClient())
            {
                photonView.RPC("BombExplodeRPC", RpcTarget.All);
            }
            else
            {
                BombExplodeRPC();
            }
        }

        private void BombReset()
        {
            isActive = false;
            throwLine.SetActive(value: false);
            itemToggle.ToggleItem(toggle: false);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        [PunRPC]
        private void BombExplodeRPC()
        {
            if (itemEquippable.isEquipped)
            {
                return;
            }
            primeParticle.gameObject.SetActive(false);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (!SemiFunc.RunIsShop() || isSpawnedGrenade)
                {
                    if (!isSpawnedGrenade)
                    {
                        StatsManager.instance.ItemRemove(itemAttributes.instanceName);
                    }
                }
                else
                {
                    physGrabObject.Teleport(grenadeStartPosition, grenadeStartRotation);
                }
                if (SemiFunc.RunIsShop() && !isSpawnedGrenade)
                {
                    BombReset();
                }
            }
        }

        [PunRPC]
        private void BombPrimedRPC()
        {
            primeParticle.SetActive(true);
            primeSound.Play(base.transform.position);
            isActive = true;
        }
    }

    public class FreezeExplosion : MonoBehaviour
    {
        private HurtCollider hurtCollider;

        private PhotonView photonView;

        private PhotonTransformView photonTransformView;

        public ItemSlushieBomb itemSlushieBomb;

        public GameObject freezeParticles;

        public GameObject freezeParticlesAttached;

        private Color colorIce = Color.Lerp(Color.cyan, Color.white, 0.3f);

        private float removeTimer = 0.1f;

        private float freezeTime = 5f;

        private static Collider[] hitObjects = new Collider[40];

        private void Start()
        {
            hurtCollider = GetComponentInChildren<HurtCollider>();
            photonView = GetComponent<PhotonView>();
            photonTransformView = GetComponent<PhotonTransformView>();
            freezeTime = 5f;
            freezeParticlesAttached.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (hurtCollider.enabled)
            {
                removeTimer -= Time.deltaTime;
            }
            if (removeTimer <= 0f)
            {
                FreezeExplosionReset();
            }
        }
        public void FreezeExplosionReset()
        {
            removeTimer = 0f;
            hurtCollider.enabled = false;
        }
        private void FreezeExplode()
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("FreezeExplodeLogic", RpcTarget.All);
            }
            else
            {
                FreezeExplodeLogic();
            }
        }

        [PunRPC]
        public void FreezeExplodeLogic()
        {
            int numHits = Physics.OverlapSphereNonAlloc(this.gameObject.transform.position, 2.5f, hitObjects, SemiFunc.LayerMaskGetShouldHits(), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < numHits; i++)
            {
                Collider hits = hitObjects[i];
                if (!(hits.gameObject.CompareTag("Phys Grab Object") || hits.gameObject.CompareTag("Enemy")))
                {
                    continue;
                }
                EnemyParent enemyParent = hits.gameObject.GetComponentInParent<EnemyParent>();
                if (enemyParent == null)
                {
                    continue;
                }
                Enemy enemy = enemyParent.Enemy;

                if (!enemy.HasStateStunned)
                {
                    continue;
                }

                Animator enemyAnimator = enemyParent.GetComponentInChildren<Animator>();
                if (enemyAnimator != null)
                {                  
                    var freezeAnimController = enemyAnimator.gameObject.GetOrAddComponent<FreezeController>();
                    freezeAnimController.SetFreezeDuration(freezeTime, false);
                    freezeAnimController.SetAnimatorDisabled(enemyAnimator);
                }

                var freezeMatController = enemyParent.gameObject.GetOrAddComponent<FreezeController>();
                freezeMatController.SetFreezeDuration(freezeTime, false);
                freezeMatController.SetChildMaterialColors(colorIce);
            }

            var playersToFreeze = SemiFunc.PlayerGetAllPlayerAvatarWithinRange(2.5f, this.gameObject.transform.position);
            {
                foreach (PlayerAvatar playerAvatar in playersToFreeze)
                {
                    Animator playerAnimator = playerAvatar.playerAvatarVisuals.animator;
                    if (playerAnimator == null)
                    {
                        continue;
                    }
                    var playerFreezeController = playerAnimator.gameObject.GetOrAddComponent<FreezeController>();
                    playerFreezeController.SetFreezeDuration(freezeTime, false);
                    playerFreezeController.SetAnimatorDisabled(playerAnimator);
                    playerFreezeController.SetChildMaterialColors(colorIce);

                    if (!playerAvatar.isLocal)
                    {
                        continue;
                    }

                    PlayerController playerController = PlayerController.instance;
                    playerController.MoveMult(0f, freezeTime);
                    playerController.OverrideLookSpeed(0f, 0.1f, 1f, freezeTime);
                    CameraZoom.Instance.OverrideZoomSet(30f, freezeTime, 0.5f, 1f, base.gameObject, 0);
                    PostProcessing.Instance.VignetteOverride(colorIce, 10f, 0.5f, 0.5f, 1f, freezeTime, base.gameObject);
                }
            }
        }
    }
}
