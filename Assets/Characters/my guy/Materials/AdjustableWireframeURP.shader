Shader "Custom/Adjustable Wireframe URP"
{
    Properties
    {
        [MainColor] _BaseColor("Fill Color", Color) = (0.03, 0.03, 0.03, 1)
        _FillOpacity("Fill Opacity", Range(0.0, 1.0)) = 1.0
        [HDR] _WireColor("Wire Color", Color) = (0, 1, 1, 1)
        _WireOpacity("Wire Opacity", Range(0.0, 1.0)) = 1.0
        _WireThickness("Wire Thickness (Pixels)", Range(0.25, 8.0)) = 1.0
        _WireSoftness("Wire Softness", Range(0.0, 3.0)) = 1.0
        _WireIntensity("Wire Intensity", Range(0.0, 10.0)) = 1.0

        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4
        [Toggle] _ZWrite("Write Depth", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Wireframe"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _WireColor;
                float _FillOpacity;
                float _WireOpacity;
                float _WireThickness;
                float _WireSoftness;
                float _WireIntensity;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogFactor : TEXCOORD0;
                float3 barycentric : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                output.barycentric = input.color.rgb;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // fwidth converts the barycentric distance into approximately
                // screen-pixel-sized units, keeping line width stable with distance.
                float3 pixelWidth = max(fwidth(input.barycentric), 0.000001);
                float inner = max(_WireThickness - _WireSoftness, 0.0);
                float outer = max(
                    _WireThickness + _WireSoftness,
                    inner + 0.0001);

                float3 edgeDistance = smoothstep(
                    pixelWidth * inner,
                    pixelWidth * outer,
                    input.barycentric);

                float wireMask = 1.0 - min(
                    edgeDistance.x,
                    min(edgeDistance.y, edgeDistance.z));

                half3 wire = _WireColor.rgb * _WireIntensity;
                half3 color = lerp(_BaseColor.rgb, wire, wireMask);
                color = MixFog(color, input.fogFactor);

                half wireOpacity = _WireOpacity * _WireColor.a;
                half alpha = lerp(_FillOpacity * _BaseColor.a, wireOpacity, wireMask);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
