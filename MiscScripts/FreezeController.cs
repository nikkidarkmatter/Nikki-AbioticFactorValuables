using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AbioticFactorValuables.MiscScripts
{
    public class FreezeController : MonoBehaviour
    {
        [HideInInspector]
        public float duration = 1f;
        [HideInInspector]
        public float initialDuration = 1f;
        [HideInInspector]
        public Color newColor = Color.cyan;
        [HideInInspector]
        public Color oldColor = Color.white;
        [HideInInspector]
        public MeshRenderer[] renderers = null!;
        [HideInInspector]
        public Animator animator = null!;
        [HideInInspector]
        public Dictionary<Material, Color> matsAndColors = [];
        [HideInInspector]
        public GameObject particles;

        private static readonly int AlbedoColor = Shader.PropertyToID("_AlbedoColor");

        public void SetFreezeDuration(float newDuration, bool addToCurrentDuration)
        {
            if (addToCurrentDuration)
            {
                duration += newDuration;
                initialDuration += newDuration;
            }
            else
            {
                duration = newDuration;
                initialDuration = newDuration;
            }
        }

        public void SetChildMaterialColors(Color color)
        {
            renderers = GetComponentsInChildren<MeshRenderer>(true);
            if (renderers == null)
            {
                return;
            }
            foreach (MeshRenderer renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || !material.HasProperty(AlbedoColor) || material.GetColor(AlbedoColor) == color)
                    {
                        continue;
                    }
                    if (material.name.StartsWith("Player Avatar"))
                    {
                        material.enableInstancing = false;
                    }
                    var oldColor = material.GetColor(AlbedoColor);
                    newColor = color;
                    matsAndColors.Add(material, oldColor);
                    material.SetColor(AlbedoColor, newColor);
                }
            }
        }

        public void SetAnimatorDisabled(Animator animatorToAffect)
        {
            animator = animatorToAffect;
            if (animator == null)
            {
                return;
            }
        }

/*        public void SetFreezeParticles(GameObject parent, GameObject newParticles)
        {
            Debug.Log($"Instantiating particles at {parent.transform.position}.");
            particles = Instantiate(newParticles, parent.transform);
            particles.gameObject.SetActive(true);
            Debug.Log($"Particles active at {particles.gameObject.transform.position}.");
            particles.gameObject.GetComponent<ParticleSystem>().Play();
        } */

        public void Update()
        {
            duration -= Time.deltaTime;
            if (duration < (initialDuration - 0.15f) && animator != null && animator.enabled)
            {
                animator.enabled = false;
            }
            if (duration > 0f)
                return;
            if (animator != null)
            {
                animator.enabled = true;
            }
            if (renderers == null || renderers.Length == 0)
            {
                Destroy(this);
                return;
            }
            foreach (MeshRenderer renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material == null || !material.HasProperty(AlbedoColor))
                    {
                        continue;
                    }
                    foreach (var matAndColor in matsAndColors)
                    {
                        if (material.name.StartsWith(matAndColor.Key.name))
                        {
                            material.SetColor(AlbedoColor, matAndColor.Value);
                            material.enableInstancing = true;
                        }
                    }
                }
            }
            Destroy(this);
        }
    }
}
