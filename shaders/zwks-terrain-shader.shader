Shader "Custom/zwks-terrain-shader" {
    Properties {
        _MaxHeight ("Maximum height", Float) = 20.0
        _MinHeight ("Minimum height", Float) = 0

        _LevelsUsed ("Number of levels", Int) = 2

        _ColorBase ("Base color", Color) = (0, 0.15, 0.25, 1)
        _Color1 ("Color 1", Color) = (0, 0.75, 0.85, 1)
        _Color2 ("Color 2", Color) = (0.5, 0.95, 0.95, 1)
        _Color3 ("Color 3", Color) = (0.7, 0.7, 0.5, 1)
        _Color4 ("Color 4", Color) = (0.5, 0.7, 0.2, 1)
        _Color5 ("Color 5", Color) = (0.35, 0.35, 0.35, 1)
        _Color6 ("Color 6", Color) = (0.4, 0.4, 0.4, 1)
        _Color7 ("Color 7", Color) = (0.8, 0.8, 1, 1)

        _Height1 ("Threshold 1", Float) = 0.15
        _Height2 ("Threshold 2", Float) = 0.25
        _Height3 ("Threshold 3", Float) = 0.30
        _Height4 ("Threshold 4", Float) = 0.35
        _Height5 ("Threshold 5", Float) = 0.40
        _Height6 ("Threshold 6", Float) = 0.6
        _Height7 ("Threshold 7", Float) = 0.7        
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        float _MinHeight;
        float _MaxHeight;
        
        int _LevelsUsed;

        float4 _ColorBase;
        float4 _Color1;
        float4 _Color2;
        float4 _Color3;
        float4 _Color4;
        float4 _Color5;
        float4 _Color6;
        float4 _Color7;

        float _Height1;
        float _Height2;
        float _Height3;
        float _Height4;
        float _Height5;
        float _Height6;
        float _Height7;

        struct Input {
            float3 worldPos;
        };

        // Range: 0-1
        float inverseLerp(float a, float b, float value) {
            return saturate((value - a)/(b - a));
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float pointHeight = inverseLerp(_MinHeight, _MaxHeight, IN.worldPos.y);
            
            o.Albedo = _ColorBase;

            /*
                - if pointheight is below threshold albedo will remain the previous color
                - if pointHeight is above threshold albedo will be replaced by next color
                - if useLevel is 0 albedo will remain the previous color regardless
                
                pAT 1 + useLevel 1 -> changeColor 1
                pAT 0 + useLevel 1 -> changeColor 0
                pAT 1 + useLevel 0 -> changeColor 0
                pAT 0 + useLevel 0 -> changeColor 0

                changeColor logic:
                add pAT + useLevel -> range(0, 2). divide by 2 -> 0, 0.5, or 1. Subtract 0.5 -> -0.5, 0, or 0.5. 
                multiply by 2 -> -1, 0, 1 -> saturate -> 0 or 1  
            */
            float useLevel = saturate(sign(_LevelsUsed - 1));
            float pointAboveThreshold = saturate(sign(pointHeight - _Height1));
            float changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color1 * changeColor;
            
            useLevel = saturate(sign(_LevelsUsed - 2));
            pointAboveThreshold = saturate(sign(pointHeight - _Height2));
            changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color2 * changeColor;

            useLevel = saturate(sign(_LevelsUsed - 3));
            pointAboveThreshold = saturate(sign(pointHeight - _Height3));
            changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color3 * changeColor;

            useLevel = saturate(sign(_LevelsUsed - 4));
            pointAboveThreshold = saturate(sign(pointHeight - _Height4));
            changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color4 * changeColor;

            useLevel = saturate(sign(_LevelsUsed - 5));
            pointAboveThreshold = saturate(sign(pointHeight - _Height5));
            changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color5 * changeColor;

            useLevel = saturate(sign(_LevelsUsed - 6));
            pointAboveThreshold = saturate(sign(pointHeight - _Height6));
            changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color6 * changeColor;

            useLevel = saturate(sign(_LevelsUsed - 7));
            pointAboveThreshold = saturate(sign(pointHeight - _Height7));
            changeColor = saturate((((pointAboveThreshold + useLevel) / 2) - 0.5) * 2);
            o.Albedo = o.Albedo * (1-changeColor) + _Color7 * changeColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
