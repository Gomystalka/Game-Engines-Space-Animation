Shader "Custom/Wrap Lambert UModified" {
		Properties {
			_MainTex ("Texture", 2D) = "white" {}
			_AttenFactor("Attenuation", float) = 1.0
			_ShadowMult ("Shadow Multiplier", float) = 1.0
			_Intensity ("Intensity", float) = 2.0
		}
			SubShader{

			Tags {"RenderType" = "Opaque"}
			CGPROGRAM
			#pragma surface surf WrapLambert
			float _AttenFactor;
			float _ShadowMult;
			float _Intensity;

		half4 LightingWrapLambert(SurfaceOutput s, half3 lightDir, half atten) {
			  half NdotL = dot(s.Normal, lightDir);
			  half diff = NdotL * 0.5 + 0.5;
			  half4 c;
			  c.rgb = s.Albedo * _LightColor0.rgb * round((saturate((diff * (atten * _AttenFactor) * _ShadowMult)) * _Intensity) / 2);
			  c.a = s.Alpha;
			  return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}
		Fallback "Diffuse"
}