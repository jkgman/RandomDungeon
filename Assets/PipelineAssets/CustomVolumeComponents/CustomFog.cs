using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/CustomFog")]
public sealed class CustomFog : CustomPostProcessVolumeComponent, IPostProcessComponent
{
   

    [Tooltip("Controls the intensity of the effect.")]

    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 10f);
    public ClampedFloatParameter startFallOff = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter endFallOff = new ClampedFloatParameter(1f, 0f, 1f);

    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterOpaqueAndSky;

    public override void Setup()

    {

        if (Shader.Find("Hidden/Shader/CustomFog") != null)
            m_Material = new Material(Shader.Find("Hidden/Shader/CustomFog"));

    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)

    {

        if (m_Material == null)

            return;

        m_Material.SetFloat("_Intensity", intensity.value);

        m_Material.SetFloat("_StartFallOff", startFallOff.value);

        m_Material.SetFloat("_EndFallOff", endFallOff.value);

        m_Material.SetTexture("_InputTexture", source);

        HDUtils.DrawFullScreen(cmd, m_Material, destination);

    }

    public override void Cleanup() => CoreUtils.Destroy(m_Material);

}
