using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PhotoMode;

namespace PhotoMode
{

    public class BlitRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public RenderTargetIdentifier source;

        RenderTargetHandle temporaryColorTexture;
        RenderTargetHandle destinationTexture;

        string profilerTag;

        //Default constructor for the Blit Render Pass
        public BlitRenderPass(RenderPassEvent renderPassEvent, Material blitMat, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            blitMaterial = blitMat;
            profilerTag = tag;
            temporaryColorTexture.Init("_TemporaryColorTexture");
            destinationTexture.Init("_AfterPostProcessTexture");
        }

        //Override the Execute function decalared in the scriptable render pass class.
        //Any code in here will execute as part of the rendering process.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //Create a command buffer, a list of graphical instructions to execute
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(destinationTexture.id, opaqueDesc, FilterMode.Point);

            //Get a temporary render texture
            cmd.GetTemporaryRT(temporaryColorTexture.id, opaqueDesc);

            //Copy what the camera is rendering to the render texture and apply the blit material
            Blit(cmd, source, temporaryColorTexture.Identifier(), blitMaterial);

            //Copy what the temporary render texture is rendering back to the camera
            Blit(cmd, temporaryColorTexture.Identifier(), source);


            //Execute the graphic commands
            context.ExecuteCommandBuffer(cmd);

            //Release the command buffer
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            //Release temporary Render Textures
            cmd.ReleaseTemporaryRT(destinationTexture.id);
            cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
        }
    }
}