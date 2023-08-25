using System.Collections.Generic;
using ScreenSpaceBoolean;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MaskRendererFeature : ScriptableRendererFeature {

    public class MaskRenderPass : ScriptableRenderPass {

        private Settings settings;
        private FilteringSettings filteringSettings;
        private ProfilingSampler _profilingSampler;
        private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
        private RTHandle depthTarget, rtTempColor;

        public MaskRenderPass(Settings settings, string name) {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);
            
            // Use default tags
            shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));
            shaderTagsList.Add(new ShaderTagId("UniversalGBuffer"));
           // shaderTagsList.Add(new ShaderTagId("UniversalForwardOnly"));
            
            _profilingSampler = new ProfilingSampler(name);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        var depthDesc = renderingData.cameraData.cameraTargetDescriptor;
        depthDesc.depthBufferBits = 32; // should be default anyway
        RenderingUtils.ReAllocateIfNeeded(ref depthTarget, depthDesc,
        name: settings.depthTargetDestinationID);
            // Using camera's depth target (that way we can ZTest with scene objects still)
            RTHandle rtCameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            ConfigureTarget(depthTarget, rtCameraDepth);
            ConfigureClear(ClearFlag.Color, new Color(0,0,0,0));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get();
            // Set up profiling scope for Profiler & Frame Debugger
            using (new ProfilingScope(cmd, _profilingSampler)) {
                // Command buffer shouldn't contain anything, but apparently need to
                // execute so DrawRenderers call is put under profiling scope title correctly
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // Draw Renderers to Render Target (set up in OnCameraSetup)
                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
                if (settings.overrideMaterial != null) {
                    drawingSettings.overrideMaterialPassIndex = settings.overrideMaterialPass;
                    drawingSettings.overrideMaterial = settings.overrideMaterial;
                }
                  for (int i = 0; i < settings.maskDrawNum; ++i) {
                foreach (var subtractor in settings.subtractors){
                cmd.DrawRenderer(subtractor, settings.overrideMaterial, 0, settings.overrideMaterialPass);
                cmd.DrawRenderer(subtractor, settings.overrideMaterial, 0, settings.overrideMaterialPass+1);
               cmd.DrawRenderer(subtractor, settings.overrideMaterial, 0, settings.overrideMaterialPass+2);
                cmd.DrawRenderer(subtractor, settings.overrideMaterial, 0, settings.overrideMaterialPass+3);
                    }
                }
                   Debug.Log(settings.subtractors.Count);
              //  context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                // Pass our custom target to shaders as a Global Texture reference
                // In a Shader Graph, you'd obtain this as a Texture2D property with "Exposed" unticked
                if (settings.depthTargetDestinationID != "") 
                    cmd.SetGlobalTexture(settings.depthTargetDestinationID, depthTarget);


            }
            // Execute Command Buffer one last time and release it
            // (otherwise we get weird recursive list in Frame Debugger)
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {}

        // Cleanup Called by feature below
        public void Dispose() {
            if (settings.depthTargetDestinationID != "")
                depthTarget?.Release();
            rtTempColor?.Release();
        }
    }

    // Exposed Settings

    [System.Serializable]
    public class Settings {
        public bool showInSceneView = true;
        public RenderPassEvent _event = RenderPassEvent.AfterRenderingOpaques;

        [Header("Draw Renderers Settings")]
        public LayerMask layerMask = 1;
        public Material overrideMaterial;
        public int overrideMaterialPass ;
        public int maskDrawNum;
        public string depthTargetDestinationID = "_SubtractionDepth";
        public List<Renderer> subtractors;

    }

    public Settings settings = new Settings();

    private MaskRenderPass m_ScriptablePass;

    public override void Create() {
        m_ScriptablePass = new MaskRenderPass(settings, name);
        m_ScriptablePass.renderPassEvent = settings._event;

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return; // Ignore feature for editor/inspector previews & asset thumbnails
        if (!settings.showInSceneView && cameraType == CameraType.SceneView) return;
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing) {
        m_ScriptablePass.Dispose();
    }
}

