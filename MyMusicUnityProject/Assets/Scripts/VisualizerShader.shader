Shader "Hidden/VisualizerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		

		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Anim("Anim", Range(0,1)) = 0.0
		_Bar("Bar", Int) = 0
		_B4("B4", Int) = 0
		_B16("B16", Int) = 0
		_B64("B64", Int) = 0
		_Pbar("Pbar", Vector) = (0,0,0,0)
		_Volume("Volume", Vector) = (1,1,1,1)
		_Tr1("Tr1", Vector) = (1,1,0,0)
		_Tr2("Tr2", Vector) = (1,1,0,0)
		_Tr3("Tr3", Vector) = (1,1,0,0)
		_Tr4("Tr4", Vector) = (1,1,0,0)
		_DEBUG("DEBUG", Range(0,1)) = 0.0
	}
    SubShader
    {

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Assets/Scripts/Drawing.glslinc"

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

			sampler2D _MainTex;
			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			float _Anim;
			int _Bar;
			int _B4;
			int _B16;
			int _B64;
			float4 _Pbar;
			float4 _Volume;
			float4 _Tr1;
			float4 _Tr2;
			float4 _Tr3;
			float4 _Tr4;
			float _DEBUG;


			float4 tex(float2 p) {
				float2 uv = (p + 1) / 2;

				return  tex2D(_MainTex, uv);
			}

			float4 tl1(float2 p, float4 tr, CPalette cp)
			{
				float vout = tr.x;
				float env = tr.y;
				float pitch = tr.z;
				float loopp = tr.w;
				float rad = ((pitch)+0.0);
				float ll = sqrt(3.0 / 4.0) * rad;
				float2 unit = float2(rad, ll);
				float2 one = float2(1, 1);
				float2 mp = (fmod2(p + unit, unit * 2) - unit);
				float4 res = circle(mp, rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 2), rad, 1), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 4), rad, 5), rad, cp.penW) * cp.penC;



				res += circle(hexP(hexP(mp, rad, 3), rad, 2), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 3), rad, 4), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 6), rad, 1), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 6), rad, 5), rad, cp.penW) * cp.penC;


				//res += circle(hexP(mp, rad, 2), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 2), rad, 2), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 4), rad, 4), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 1), rad, 1), rad, cp.penW) * cp.penC;
				res += circle(hexP(hexP(mp, rad, 5), rad, 5), rad, cp.penW) * cp.penC;

				//res += Line(mp, float2(0, -1), float2(0, 1), cp.penW) * cp.penC;
				//res += Line(mp, float2(-1, 0), float2(1, 0), cp.penW) * cp.penC;

				for (int i = 1; i < 7; i++) {
					//res += circle(hexP(mp, rad, i), rad, cp.penW) * cp.penC;
					//res += circle(hexP( hexP(mp, rad, i), rad, i), rad, cp.penW) * cp.penC;
				}
				return res * env;
			}
			float4 prevTone(float2 p, float4 tr, CPalette cp)
			{
				float audio = tr.x;
				float trigPhase = tr.y;
				float pitch = tr.z;
				float loopPhase = tr.w;
				int step = 16;
				int loopI = (int)(loopPhase * step);
				float stepPhase = (loopPhase * step) - loopI;
				float4 res = 0;

				float radi = -2 * PI * loopI / 16.0;


				float pstep = 1.0 / 16;
				float2 r = rot(p, radi);
				float pitchp = 1 - fmod(pitch, 1);
				res = fan(r, pitchp, 2 * PI * pstep * stepPhase);
				res += -fan(r, pitchp - pstep, 2 * PI * pstep * stepPhase);

				//float2 tt = p+toCircle(pitchp, radi);
				float2 tt = rot(p, radi) - float2(0, pitchp - pstep / 2);
				float2 rt = rot(p, -2 * PI * loopPhase) - float2(0, pitchp - pstep / 2);
				res += disc(tt, pstep / 2);
				res += disc(rt, pstep / 2);
				res = clamp(res, 0, 1);
				res *= cp.fillC;




				return res;
			}
			float4 prevRythm(float2 p, float4 tr, CPalette cp)
			{
				float audio = tr.x;
				float trigPhase = tr.y;
				float env = tr.z;
				float loopPhase = tr.w;
				int step = 16;
				int loopI = (int)(loopPhase * step);
				float stepPhase = (loopPhase * step) - loopI;
				float4 res = 0;

				float radi = -2 * PI * loopI / step;
				float stepR = 1 / step;


				float rad = 1.0 / 25;
				float2 r = rot(p, radi);
				res = fan(r, 0.3, 2 * PI * stepR * stepPhase);
				res += -fan(r, 0.2, 2 * PI * stepR * stepPhase);

				//float2 tt = p+toCircle(pitchp, radi);
				float2 tt = rot(p, radi) - float2(0, rad);
				float2 rt = rot(p, -2 * PI * loopPhase) - float2(0, rad);
				res += disc(tt, rad / 2);
				res += disc(rt, rad / 2);
				res = clamp(res, 0, 1);
				res *= cp.fillC;




				return res;
			}
			float2 toPole(float2 p) {

				return float2(length(p), atan2(p.y, p.x));
			}


			fixed4 frag(v2f i) : SV_Target
			{

				float2 ss = _ScreenParams.xy;
				float screenratio = ss.x / ss.y;
				float2 uv = float2(i.uv.x * screenratio,i.uv.y);

				float4 bgtex = tex2D(_MainTex, i.uv);

				float2 p = float2(uv.x * 2 - screenratio, uv.y * 2 - 1) ;
				float3 uvc = float3(p.x, p.y , 1);
				float2 center = float2(0,0);
				float4 dt = float4(0,4,8,12);
				float4 bt = fmod(dt + _Pbar.x + _Bar, 16.0f) / 16.0f;
				float4 bts = 0 / 2.0f + 0.5;
				float tr1 = 1 - _Tr1.y;


				float t2 = 1 - fmod(_Pbar.x + _Bar, 2.0f) / 2.0f;
				float ts2 = 1 - sin(t2 * PI * 2) / 2.0f;
				float t3 = 1 - fmod(_Pbar.y + _B4, 1.0f) / 1.0f;

				float t4 = 1 - fmod(_Pbar.x + _Bar, 4.0f) / 4.0f;
				float ts4 = sin(t4 * PI * 2) / 2.0f + 0.5;

				float2 ter1 = rot(p, -((bt.x * PI * 2) - (length(p) * bts.x * 2)));
				float2 ter2 = rot(p,  ((bt.y * PI * 2) - (length(p) * bts.y * 2)));

				float t5 = fmod(_Pbar.x + _Bar, 8.0f) / 8.0f;

				float4 tc1 = tex(p) * t5 * t5 * t5 * length(p) * length(p) * 0.1;

				// Albedo comes from a texture tinted by color
				float4 c1 = tex(ter1) * bts.x * (0.1f);
				float4 c2 = _Tr4.y * 0.1;
				float4 c3 = tr1 * float4(1, 1, 1, 1);
				//fixed4 c = tex2D(_MainTex, uv) * _Color *  Line(uv, float2(1, 1), float2(0.5, 0.5), 0.1);
				CPalette cp;
				cp.fillC = c2;
				cp.penC = c3;
				cp.penW = 0.0051;
				//o.Albedo = c.rgb * (1 - fmod(_Anim + _Time, 1.0f));
				//o.Albedo = Line(p, float2(-0.9, 0.5), float2(-0.1, 0.3), 0.03);
				//o.Albedo += c * fan(uv, center, 0.3, (t * 2 - 1) * PI);
				float2 r1 = rot(p, center, -bts.x * PI * 2 * (disc(p - center, bts.x)));

				fixed4 yingyangv = (yingyang(r1, center, bts.x * 6) * 0.5 + 0.5) * c1;
				fixed4 yingyangv2 = yingyang2(r1, float2(center.x, center.y - bts.x * 6 / 2), bts.x * 6 / 2, c2);
				fixed4 yingyangv3 = yingyang2(r1, float2(center.x, center.y + bts.x * 6 / 2), bts.x * 6 / 2, c3);
				//o.Albedo += ptl(0.5, uv + 1.0 * toCircle(0.01, 1.0 * 2.0 * PI / 6.0), 0.02);
				//o.Albedo += tl2(uv, 0.1, 0.002);
				//o.Albedo += (tl(yy, r, 0.0051) * yingyangv + tl(yt, r / 2, 0.005) * (yingyangv2 + yingyangv3));

				float4 trc1 = metatron(ter1 , bts.x, cp) * bts.x
					//+ metatron(ter2, bts.y, cp) * bts.y
					//+ metatron(ter1, bts.z, cp) * bts.z
					//+ metatron(ter2, bts.w, cp) * bts.w
					;
				//o.Albedo = float4(1, 0.3, 1, 1);

				cp.penC = (_Tr2.y) * float4(1, 0.3, 1, 0.3);

				float4 trc2 = tl2(ter1, 0.5 * bts.x, cp) * bts.x
					//+ tl2(ter2, 0.5 * bts.y, cp) * bts.y
					//+ tl2(ter1, 0.5 * bts.z, cp) * bts.z
					//+ tl2(ter2, 0.5 * bts.w, cp) * bts.w

					;


				float loopP = _Tr3.w;
				float pitch = _Tr3.z;
				cp.penC = float4((_Tr3.z), 0.6, 0.3, 1);
				cp.fillC = (_Tr4.y) * float4(0.2, 0.6, 0.3, 1);
				float2 r2 = rot(p, center, 1 * PI * 2);

				float4 trc3 = tl1(ter1, _Tr3, cp);


				float4 trc4 = metatron(ter1, _Tr3.z, cp) * bts.x;

				fixed4 result;
				result = trc1 * _Volume.x
					+ trc2 * length(p) * 2 * _Volume.y
					+ trc3 * _Volume.z
					+ trc4 * _Volume.w



					;


				float4 prev =
					prevRythm(p, _Tr1, cp) * 1 * _Volume.x
					+ prevRythm(p, _Tr2, cp) * 1 * _Volume.y
					+ prevTone(p, _Tr3, cp) * 1 * _Volume.z
					+ prevTone(p, _Tr4, cp) * 1 * _Volume.w;


				//o.Albedo += (tex(ter1/1.25) + tex(ter2 / 1.25))  * (1 - disc(0, 0.9));
				//o.Albedo = tl2(r1, t11, cp) ;
				float2 poleP = toPole(p);
				result += _DEBUG * (grid(poleP, 1 / 16.0f / length(1), cp) - prev);
				//o.Albedo = metatron(uv, 0.4, cp) / mdm(t11);
				//o.Albedo += yingyangv - yingyangv2;





				float l = (1 - length(p));
				result += bgtex*l*l;


                return result;
            }
            ENDCG
        }
    }
}
