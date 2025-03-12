using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Terrix.Shaders.JFA
{
    public class OutlineRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private RenderPassEvent inject;

        [SerializeField] private Shader jfaInit;
        [SerializeField] private Shader jfaPass;
        [SerializeField] private Shader jfaOutline;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField, Min(0)] private float outlinePixelWidth = 2f;

        private OutlineRenderPass outlineRenderPass;

        public override void Create()
        {
            outlineRenderPass = new OutlineRenderPass
            {
                renderPassEvent = inject
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (jfaInit == null)
            {
                Debug.LogWarning($"{nameof(jfaInit)} is null {nameof(OutlineRenderPass)} skipped...");
                return;
            }

            if (jfaPass == null)
            {
                Debug.LogWarning($"{nameof(jfaPass)} is null {nameof(OutlineRenderPass)} skipped...");
                return;
            }
            
            if (jfaOutline == null)
            {
                Debug.LogWarning($"{nameof(jfaOutline)} is null {nameof(OutlineRenderPass)} skipped...");
                return;
            }

            outlineRenderPass.Setup(
                jfaInit,
                jfaPass,
                jfaOutline,
                outlinePixelWidth,
                targetLayers);

            renderer.EnqueuePass(outlineRenderPass);
        }

        private class OutlineRenderPass : ScriptableRenderPass
        {
            private Shader jfaInitShader;
            private Shader jfaPassShader;
            private Shader jfaOutlineShader;
            
            private float outlinePixelWidth;

            private FilteringSettings filteringSettings;

            readonly List<ShaderTagId> shaderTagIds = new()
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };


            private class FillMaskPassData
            {
                public RendererListHandle maskedObjects;
            }

            public void Setup(Shader jfaInit, Shader jfaPass, Shader jfaOutline ,float outlineWidth, LayerMask targetLayers)
            {
                this.outlinePixelWidth = outlineWidth;
                this.jfaInitShader = jfaInit;
                this.jfaPassShader = jfaPass;
                this.jfaOutlineShader = jfaOutline;
                this.filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask: targetLayers);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                if (resourceData.isActiveTargetBackBuffer)
                {
                    return;
                }

                var renderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();

                var bufferTextureDescriptor = new RenderTextureDescriptor(
                    cameraData.cameraTargetDescriptor.width,
                    cameraData.cameraTargetDescriptor.height,
                    cameraData.cameraTargetDescriptor.graphicsFormat,
                    0
                );
                var bufferTextureDescriptor2 = new RenderTextureDescriptor(
                    cameraData.cameraTargetDescriptor.width,
                    cameraData.cameraTargetDescriptor.height,
                    cameraData.cameraTargetDescriptor.graphicsFormat,
                    0
                );

                var ping = UniversalRenderer.CreateRenderGraphTexture(renderGraph, bufferTextureDescriptor, "Buffer 1", true);
                var pong = UniversalRenderer.CreateRenderGraphTexture(renderGraph, bufferTextureDescriptor2, "Buffer 2", true);

                using (var builder =
                       renderGraph.AddRasterRenderPass<FillMaskPassData>("Fill Mask Pass", out var passData))
                {
                    var drawSettings = RenderingUtils.CreateDrawingSettings(
                        shaderTagIds,
                        renderingData,
                        cameraData,
                        lightData,
                        cameraData.defaultOpaqueSortFlags
                    );

                    var rendererListParameters =
                        new RendererListParams(renderingData.cullResults, drawSettings, filteringSettings);
                    passData.maskedObjects = renderGraph.CreateRendererList(rendererListParameters);

                    builder.SetRenderAttachment(ping, 0);
                    builder.UseRendererList(passData.maskedObjects);

                    builder.SetRenderFunc((FillMaskPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(true, true, Color.clear);
                        context.cmd.DrawRendererList(data.maskedObjects);
                    });
                }

                var jfaInitPass =
                    new RenderGraphUtils.BlitMaterialParameters(ping, pong, new Material(jfaInitShader), 0)
                    {
                        sourceTexturePropertyID = Shader.PropertyToID("_MainTex")
                    };
                renderGraph.AddBlitPass(jfaInitPass, "JFA Init Pass");

                var maxTextureSize = Mathf.Max(
                    cameraData.cameraTargetDescriptor.width,
                    cameraData.cameraTargetDescriptor.height);

                var numMips = Mathf.CeilToInt(Mathf.Log(maxTextureSize / 2, 2f));

                for (var i = 1; i <= numMips; i++)
                {
                    var stepWidth = 1 / Mathf.Pow(2, i);
                    var jfaPassMaterial = new Material(jfaPassShader);
                    jfaPassMaterial.SetFloat(Shader.PropertyToID("_StepSize"), stepWidth);
                    var jfaPass = new RenderGraphUtils.BlitMaterialParameters(pong, ping, jfaPassMaterial, 0)
                    {
                        sourceTexturePropertyID = Shader.PropertyToID("_MainTex")
                    };
                    renderGraph.AddBlitPass(jfaPass, "JFA Pass");
                    (ping, pong) = (pong, ping);
                }

                var jfaOutline = new RenderGraphUtils.BlitMaterialParameters(pong, ping, new Material(jfaOutlineShader), 0)
                {
                    sourceTexturePropertyID = Shader.PropertyToID("_MainTex")
                };
                renderGraph.AddBlitPass(jfaOutline, "JFA Outline");
                
                resourceData.cameraColor = ping;
            }
        }
    }
}