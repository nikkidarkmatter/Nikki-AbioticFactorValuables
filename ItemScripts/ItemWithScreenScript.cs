using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace AbioticFactorValuables.ItemScripts
{
    public class ItemWithScreenScript : Trap
    {
        public GameObject ScreenScreen;

        public UnityEvent ScreenTimer;

        public float runTime = 10f;

        private float speedMulti = 1f;

        public MeshRenderer ScreenStatic;

        public Light ScreenLight;

        public AnimationCurve ScreenStaticCurve;

        public float ScreenStaticTime = 0.5f;

        public float ScreenStaticTimer;

        [Space]
        [Header("___________________ Screen Sounds ___________________")]
        public Sound LoopSound;

        public bool LoopSoundRandomStart = true;

        public Sound StartSound;

        public Sound StopSound;

        [HideInInspector]
        public bool TrapDone;

        private bool ScreenStart = true;

        public override void Start()
        {
            base.Start();
            if (ScreenStatic != null)
            {
                ScreenStatic.enabled = false;
            }
            ScreenLight.enabled = false;
            ScreenScreen.SetActive(false);
            photonView = GetComponent<PhotonView>();
            if (GameManager.instance.gameMode == 0)
            {
                isLocal = true;
            }
        }

        public void TrapActivate()
        {
            if (!trapTriggered)
            {
                trapActive = true;
                trapTriggered = true;
                ScreenStart = true;
                ScreenTimer.Invoke();
                if (LoopSoundRandomStart == false)
                {
                    RestartSound();
                }
            }
        }

        public void TrapStop()
        {
            if (trapActive)
            {
                if (ScreenStatic != null)
                {
                    ScreenStaticTimer = 0f;
                }
                ScreenScreen.SetActive(false);
                StopSound.Play(physGrabObject.centerPoint);
                trapActive = false;
                LoopSound.Stop();
                if (LoopSoundRandomStart == false)
                {
                    RestartSound();
                }
            }
        }

        public override void Update()
        {
            base.Update();
            LoopSound.PlayLoop(trapActive, 0.9f, 0.9f);
            if (trapStart)
            {
                TrapActivate();
            }
            if (!trapActive)
            {
                return;
            }
            enemyInvestigate = true;
            if (ScreenStart)
            {
                ScreenLight.enabled = true;
                ScreenScreen.SetActive(true);
                ScreenStart = false;
                StartSound.Play(physGrabObject.centerPoint);
            }
            if (ScreenStatic != null)
            {
                float num = ScreenStaticCurve.Evaluate(ScreenStaticTimer / ScreenStaticTime);
                ScreenStaticTimer += 1f * Time.deltaTime * speedMulti;
                if (num > 0.5f)
                {
                    ScreenStatic.enabled = true;
                }
                else
                {
                    ScreenStatic.enabled = false;
                }
                if (ScreenStaticTimer > ScreenStaticTime)
                {
                    ScreenStaticTimer = 0f;
                    ScreenStatic.enabled = false;
                }
            }
        }

        private void RestartSound()
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("RestartSoundRPC", RpcTarget.All);
            }
            else
            {
                RestartSoundRPC();
            }
        }

        [PunRPC]
        private void RestartSoundRPC()
        {
            LoopSound.Stop();
            LoopSound.PlayLoop(trapActive, 0.9f, 0.9f);
        }
    }
}
