using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageEffectUtil {
    [RequireComponent(typeof(Camera))]
    public class ImageEffectBase : MonoBehaviour {

        [SerializeField] protected Shader shader;
        [SerializeField] protected bool enable;
        [HideInInspector] protected Material imageEffectMat;

        protected virtual void Start() {
            this.imageEffectMat = new Material(this.shader);
        }

        protected virtual void OnRenderImage(RenderTexture src, RenderTexture dst) {
            if (!IsSupportAndEnable())
                Graphics.Blit(src, dst, imageEffectMat);
            else
                Graphics.Blit(src, dst);
        }

        protected bool IsSupportAndEnable () {
            return imageEffectMat != null && imageEffectMat.shader.isSupported && enable;
        }
    }
}