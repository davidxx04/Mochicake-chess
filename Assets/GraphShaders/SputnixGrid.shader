Shader "Custom/SputnixGrid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // <-- EL CEBO PARA UNITY
        _BgColor ("Color de Fondo", Color) = (1.0, 1.0, 1.0, 1.0)
        _GradientColorA ("Gradiente A", Color) = (1.0, 0.2, 0.6, 1.0)
        _GradientColorB ("Gradiente B", Color) = (1.0, 0.9, 0.2, 1.0)
        _GradientScale ("Escala Gradiente", Float) = 2.5
        _GradientSpeed ("Velocidad Gradiente", Float) = 0.6
        [Toggle] _ColorDriftEnabled ("Cambio Color Activo", Float) = 1
        _ColorDriftSpeed ("Velocidad Cambio Color", Float) = 0.15
        _ColorDriftAmount ("Intensidad Cambio Color", Range(0, 1)) = 1.0
        _BackgroundIntensity ("Brillo Fondo", Range(0, 2)) = 1.0
        _LineColor ("Color de Linea", Color) = (1.0, 1.0, 1.0, 1.0)
        _GridSize ("Densidad Cuadricula", Float) = 20.0
        _LineThickness ("Grosor Linea", Float) = 0.05
        _WaveFreq ("Frecuencia Onda", Float) = 8.0
        _Distortion ("Distorsion Sputnix", Float) = 0.05
        _Speed ("Velocidad", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // <-- DECLARACIÓN INTERNA PARA QUE COMPILE
            float4 _BgColor;
            float4 _GradientColorA;
            float4 _GradientColorB;
            float _GradientScale;
            float _GradientSpeed;
            float _ColorDriftEnabled;
            float _ColorDriftSpeed;
            float _ColorDriftAmount;
            float _BackgroundIntensity;
            float4 _LineColor;
            float _GridSize;
            float _LineThickness;
            float _WaveFreq;
            float _Distortion;
            float _Speed;

            float Hash11(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            float Noise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                float a = Hash11(i);
                float b = Hash11(i + 1.0);
                float u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u);
            }

            float3 RandomWeights(float t, float seed)
            {
                float3 w;
                w.x = Noise1D(t + seed + 0.0);
                w.y = Noise1D(t + seed + 10.0);
                w.z = Noise1D(t + seed + 20.0);
                float sum = w.x + w.y + w.z + 1e-5;
                return w / sum;
            }

            float4 PaletteMix(float t, float seed)
            {
                float3 w = RandomWeights(t, seed);
                return _GradientColorA * w.x + _GradientColorB * w.y + _BgColor * w.z;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Ajustar UVs para que el centro sea la pantalla
                o.uv = v.uv - 0.5; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x *= aspect;
                
                // MAGIA SPUTNIX: Distorsión senoidal cruzada animada con el tiempo
                uv.x += sin(uv.y * _WaveFreq + _Time.y * _Speed) * _Distortion;
                uv.y += cos(uv.x * _WaveFreq - _Time.y * _Speed) * _Distortion;

                float time = _Time.y * _GradientSpeed;
                float driftTime = _Time.y * _ColorDriftSpeed;
                float driftAmount = _ColorDriftAmount * _ColorDriftEnabled;
                float4 driftA = PaletteMix(driftTime, 1.7);
                float4 driftB = PaletteMix(driftTime, 3.4);
                float4 driftBg = PaletteMix(driftTime, 5.1);
                float4 driftLine = PaletteMix(driftTime, 7.3);
                float4 gradA = lerp(_GradientColorA, driftA, driftAmount);
                float4 gradB = lerp(_GradientColorB, driftB, driftAmount);
                float4 bgBase = lerp(_BgColor, driftBg, driftAmount);
                float4 lineColor = lerp(_LineColor, driftLine, driftAmount);

                float gx = sin(uv.x * _GradientScale + time);
                float gy = sin(uv.y * _GradientScale - time * 0.9);
                float gz = sin((uv.x + uv.y) * _GradientScale + time * 0.6);
                float blend = (gx + gy + gz) * (1.0 / 3.0);
                float t = blend * 0.5 + 0.5;
                float4 gradientColor = lerp(gradA, gradB, t);
                float4 bgColor = gradientColor * bgBase;
                bgColor.rgb *= _BackgroundIntensity;

                // Multiplicamos por la densidad para hacer la cuadrícula
                float2 grid = frac(uv * _GridSize);
                float2 gridDist = min(grid, 1.0 - grid);
                float lineDist = min(gridDist.x, gridDist.y);
                float aa = fwidth(lineDist);
                float isLine = 1.0 - smoothstep(_LineThickness, _LineThickness + aa, lineDist);
                
                // Mezclamos el gradiente con el color de la línea
                return lerp(bgColor, lineColor, isLine);
            }
            ENDCG
        }
    }
}