using UnityEngine;
using UnityEngine.Rendering.Universal;
using PhotoMode;

namespace PhotoMode
{

    public class Blit : ScriptableRendererFeature
    {
        public Material blitMaterial = null;
        private BlitRenderPass blitRenderPass;

        public override void Create()
        {
            blitRenderPass = new BlitRenderPass(RenderPassEvent.AfterRendering, blitMaterial, name);
            blitRenderPass.source = "_AfterPostProcessTexture";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            //Check to make sure the blit has a material and exit gracefully if it doesn't
            if (blitMaterial == null)
            {
                Debug.LogError("Blit is missing it's Material. Make sure you have assigned a material in the renderer");
                return;
            }

            //Add a the blit render pass to the que of render passes to execute
            renderer.EnqueuePass(blitRenderPass);
        }
    }
}