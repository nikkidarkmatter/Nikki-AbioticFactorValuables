using Photon.Pun;
using UnityEngine;

public class ItemCrossbow : MonoBehaviour
{
    private PhysGrabObject physGrabObject;

    private ItemToggle itemToggle;

    public int numberOfBolts = 1;

    [Range(0f, 65f)]
    public float gunRandomSpread;

    public float gunRange = 50f;

    public float distanceKeep = 0.8f;

    public float gunRecoilForce = 1f;

    public float cameraShakeMultiplier = 1f;

    public float torqueMultiplier = 1f;

    public float grabStrengthMultiplier = 1f;

    public float shootCooldown = 1f;

    public float batteryDrain = 0.1f;

    public bool batteryDrainFullBar;

    public int batteryDrainFullBars = 1;

    public Animator crossbowAnim;

    public LineRenderer lineRenderer;

    public Transform midPoint;

    [Range(0f, 100f)]
    public float misfirePercentageChange = 50f;

    public AnimationCurve shootLineWidthCurve;

    public float grabVerticalOffset = -0.2f;

    public float aimVerticalOffset = -10f;

    public float investigateRadius = 20f;

    public Transform gunMuzzle;

    public GameObject boltPrefab;

    public Transform gunTrigger;

    public Sound soundShoot;

    public Sound soundShootGlobal;

    public Sound soundReload;

    public Sound soundNoAmmoClick;

    public Sound soundHit;

    private float shootCooldownTimer;

    private ItemBattery itemBattery;

    private PhotonView photonView;

    private PhysGrabObjectImpactDetector impactDetector;

    private bool prevToggleState;

    private AnimationCurve triggerAnimationCurve;

    private float triggerAnimationEval;

    private bool triggerAnimationActive;

    private ItemEquippable itemEquippable;

    private void Start()
    {
        physGrabObject = GetComponent<PhysGrabObject>();
        itemEquippable = GetComponent<ItemEquippable>();
        itemToggle = GetComponent<ItemToggle>();
        itemBattery = GetComponent<ItemBattery>();
        photonView = GetComponent<PhotonView>();
        impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
        triggerAnimationCurve = AssetManager.instance.animationCurveClickInOut;
    }

    private void FixedUpdate()
    {
        if (physGrabObject.grabbed && physGrabObject.grabbedLocal)
        {
            PhysGrabber.instance.OverrideGrabDistance(distanceKeep);
        }
        if (triggerAnimationActive)
        {
            float num = 45f;
            triggerAnimationEval += Time.deltaTime * 4f;
            gunTrigger.localRotation = Quaternion.Euler(num * triggerAnimationCurve.Evaluate(triggerAnimationEval), 0f, 0f);
            if (triggerAnimationEval >= 1f)
            {
                gunTrigger.localRotation = Quaternion.Euler(0f, 0f, 0f);
                triggerAnimationActive = false;
                triggerAnimationEval = 1f;
            }
        }
        UpdateMaster();
    }

    private void LateUpdate()
    {
        lineRenderer.SetPosition(1, midPoint.localPosition);
    }

    private void UpdateMaster()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            return;
        }
        if (physGrabObject.grabbed)
        {
            Quaternion turnX = Quaternion.Euler(aimVerticalOffset, 0f, 0f);
            Quaternion turnY = Quaternion.Euler(0f, 0f, 0f);
            Quaternion identity = Quaternion.identity;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = true;
            foreach (PhysGrabber item in physGrabObject.playerGrabbing)
            {
                if (flag3)
                {
                    if (item.playerAvatar.isCrouching || item.playerAvatar.isCrawling)
                    {
                        flag2 = true;
                    }
                    flag3 = false;
                }
                if (item.isRotating)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                physGrabObject.TurnXYZ(turnX, turnY, identity);
            }
            float num = grabVerticalOffset;
            if (flag2)
            {
                num += 0.5f;
            }
            physGrabObject.OverrideGrabVerticalPosition(num);
            if (!flag)
            {
                if (grabStrengthMultiplier != 1f)
                {
                    physGrabObject.OverrideGrabStrength(grabStrengthMultiplier);
                }
                if (torqueMultiplier != 1f)
                {
                    physGrabObject.OverrideTorqueStrength(torqueMultiplier);
                }
                if (itemBattery.batteryLife <= 0f)
                {
                    physGrabObject.OverrideTorqueStrength(0.1f);
                }
            }
            if (flag)
            {
                physGrabObject.OverrideAngularDrag(40f);
                physGrabObject.OverrideTorqueStrength(6f);
            }
        }
        if (itemToggle.toggleState != prevToggleState)
        {
            if (shootCooldownTimer <= 0f)
            {
                Shoot();
                shootCooldownTimer = shootCooldown;
            }
            if (itemBattery.batteryLife <= 0f)
            {
                soundNoAmmoClick.Play(base.transform.position);
                SemiFunc.CameraShakeImpact(1f, 0.1f);
                physGrabObject.rb.AddForceAtPosition(-gunMuzzle.forward * 1f, gunMuzzle.position, ForceMode.Impulse);
                if (itemBattery.batteryLife >= 0f)
                {
                    if (!this.gameObject.activeSelf || itemEquippable.isEquipped)
                        return;
                    if (SemiFunc.IsMultiplayer())
                    {
                        photonView.RPC("ChargeReloadRPC", RpcTarget.All);
                    }
                    else
                    {
                        ChargeReloadRPC();
                    }
                }
            }
            prevToggleState = itemToggle.toggleState;
        }
        if (shootCooldownTimer > 0f)
        {
            shootCooldownTimer -= Time.deltaTime;
        }
    }

    public void Misfire()
    {
        if (!physGrabObject.grabbed && !physGrabObject.hasNeverBeenGrabbed && SemiFunc.IsMasterClientOrSingleplayer() && (float)Random.Range(0, 100) < misfirePercentageChange)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        bool flag = false;
        if (itemBattery.batteryLife <= 0f)
        {
            flag = true;
        }
        if (Random.Range(0, 10000) == 0)
        {
            flag = false;
        }
        if (!flag)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("ShootRPC", RpcTarget.All);
            }
            else
            {
                ShootRPC();
            }
        }
    }


    [PunRPC]
    public void ShootRPC()
    {
        float distanceMin = 3f * cameraShakeMultiplier;
        float distanceMax = 16f * cameraShakeMultiplier;
        SemiFunc.CameraShakeImpactDistance(gunMuzzle.position, 5f * cameraShakeMultiplier, 0.1f, distanceMin, distanceMax);
        SemiFunc.CameraShakeDistance(gunMuzzle.position, 0.1f * cameraShakeMultiplier, 0.1f * cameraShakeMultiplier, distanceMin, distanceMax);
        soundShoot.Play(gunMuzzle.position);
        soundShootGlobal.Play(gunMuzzle.position);
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            return;
        }
        if (investigateRadius > 0f)
        {
            EnemyDirector.instance.SetInvestigate(base.transform.position, investigateRadius);
        }
        physGrabObject.rb.AddForceAtPosition(-gunMuzzle.forward * gunRecoilForce, gunMuzzle.position, ForceMode.Impulse);
        if (!batteryDrainFullBar)
        {
            itemBattery.batteryLife -= batteryDrain;
        }
        else
        {
            itemBattery.RemoveFullBar(batteryDrainFullBars);
        }
        for (int i = 0; i < numberOfBolts; i++)
        {
            Vector3 endPosition = gunMuzzle.position;
            bool hit = false;
            bool flag = false;
            Vector3 vector = gunMuzzle.forward;
            if (gunRandomSpread > 0f)
            {
                float angle = Random.Range(0f, gunRandomSpread / 2f);
                float angle2 = Random.Range(0f, 360f);
                Vector3 normalized = Vector3.Cross(vector, Random.onUnitSphere).normalized;
                Quaternion quaternion = Quaternion.AngleAxis(angle, normalized);
                vector = (Quaternion.AngleAxis(angle2, vector) * quaternion * vector).normalized;
            }
            if (Physics.Raycast(gunMuzzle.position, vector, out var hitInfo, gunRange, (int)SemiFunc.LayerMaskGetVisionObstruct() + LayerMask.GetMask("Enemy")))
            {
                endPosition = hitInfo.point;
                hit = true;
            }
            else
            {
                flag = true;
            }
            if (flag)
            {
                endPosition = gunMuzzle.position + gunMuzzle.forward * gunRange;
                hit = false;
            }
            ShootBolt(endPosition, hit);
        }
    }

    private void ShootBolt(Vector3 _endPosition, bool _hit)
    {
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("ShootBoltRPC", RpcTarget.All, _endPosition, _hit);
            }
            else
            {
                ShootBoltRPC(_endPosition, _hit);
            }
        }
    }

    private void ChargeReload()
    {
        if (!this.gameObject.activeSelf)
            return;
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("ChargeReloadRPC", RpcTarget.All);
            }
            else
            {
                ChargeReloadRPC();
            }
        }
    }

    [PunRPC]
    public void ShootBoltRPC(Vector3 _endPosition, bool _hit)
    {
        crossbowAnim.SetBool("shoot", true);
        if (itemBattery.batteryLife <= 0f || batteryDrainFullBars <= 0)
        {
            crossbowAnim.SetBool("outofammo", true);
        }
        else
        {
            crossbowAnim.SetBool("outofammo", false);
            crossbowAnim.SetBool("shoot", false);
            soundReload.Play(gunMuzzle.position);
        }
        if (physGrabObject.playerGrabbing.Count > 1)
        {
            foreach (PhysGrabber item in physGrabObject.playerGrabbing)
            {
                item.OverrideGrabRelease();
            }
        }
        ItemGunBullet component = Object.Instantiate(boltPrefab, gunMuzzle.position, gunMuzzle.rotation).GetComponent<ItemGunBullet>();
        component.hitPosition = _endPosition;
        component.bulletHit = _hit;
        soundHit.Play(_endPosition);
        component.shootLineWidthCurve = shootLineWidthCurve;
        component.ActivateAll();      
    }

    [PunRPC]
    public void ChargeReloadRPC()
    {
        crossbowAnim.SetBool("shoot", false);
        crossbowAnim.SetBool("outofammo", false);
    }
}
