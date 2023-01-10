Shader "Unlit/Environment"
{
    Properties
    {
        _Horizon("horizon", Range (0, 1)) = 0
		_Smoothness("smoothness", Range(0.001, 1)) = 1
		_StartColor("start color", Color) = (0,0,0,1)
		_EndColor("end color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Front

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            half _Horizon, _Smoothness;
			half4 _StartColor, _EndColor;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                half tmp = _Horizon + _Smoothness;
				half tmp1 = max(_Horizon, tmp);
				half tmp2 = min(_Horizon, tmp);

				half forLerp = smoothstep(tmp1, tmp2, v.uv.g);

				o.color = lerp(_StartColor, _EndColor, forLerp);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
