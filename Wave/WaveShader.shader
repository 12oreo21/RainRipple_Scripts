Shader "Unlit/WaveShader"
{
	Properties
	{
		_InputTex ("Input", 2D) = "black" {}
		_PrevTex ("Prev", 2D) = "black" {}
		_Prev2Tex ("Prev2", 2D) = "black" {}
		_Stride ("Stride", Float) = 1
		_C("C", Float) = 0.1
        _Attenuation("Attenuation", Range(0.1, 0.99)) = 0.9
	}
	SubShader
	{
        //なんのためにCull・ZWrite・ZTestの設定をしているか謎。処理を軽くするため？★
		Cull Off ZWrite Off ZTest Always

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
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				return o;
			}

            float2 _HitPos2UVArray[15];
			sampler2D _InputTex;
            float _Radius;
			sampler2D _PrevTex;
			float4 _PrevTex_TexelSize;
			sampler2D _Prev2Tex;
			float _Stride;
			float _C;
            float _Attenuation;

			fixed4 frag (v2f i) : SV_Target
			{
				//波動方程式を解くフラグメントシェーダー（波動方程式は、現時点と１つ前時点の高さ（波）の値から、１つ先時点の波を求める方程式）
				//_PrevTexと_Prev2Tex、結果として返すテクスチャはR8(赤色のみ)フォーマットのテクスチャで
				//波の高さを表現するので(r = 0.5)を高さ0の状態とする。（波なのでマイナスの値も扱いたい。凹み。）
				//そのため、高さを(-1 <= r <= 1)の範囲で表すために(r * 2 - 1)を行う（処理を行わないと、0 <= r <= 1 で波動方程式を計算することになってしまう）
                //_Cは波動方程式の位相速度（要は波が進む速度）。表現に合わせて自分で調整.
				float2 stride = float2(_Stride, _Stride) * _PrevTex_TexelSize.xy;
				half4 prev = (tex2D(_PrevTex, i.uv) * 2) - 1;
                //波動方程式から求められた１つ先時点の高さ（波）の値
                half result =
					(prev.r * 2 -
						(tex2D(_Prev2Tex, i.uv).r * 2 - 1) + (
						(tex2D(_PrevTex, float2(i.uv.x+stride.x, i.uv.y)).r * 2 - 1) +
						(tex2D(_PrevTex, float2(i.uv.x-stride.x, i.uv.y)).r * 2 - 1) +
						(tex2D(_PrevTex, float2(i.uv.x, i.uv.y+stride.y)).r * 2 - 1) +
						(tex2D(_PrevTex, float2(i.uv.x, i.uv.y-stride.y)).r * 2 - 1) -
						prev.r * 4) *
					_C);
                half input = tex2D(_InputTex, i.uv).r;
                
                for(int k=0;k<15;k++)
                {
                    float dis = distance(_HitPos2UVArray[k], i.uv);
                    if(dis <= _Radius)
                    {
                        input.r = 1;
                    }
                }
                
				result += input.r;
                //波の振動を減衰させる.
                result *= _Attenuation;
                //波動方程式の計算は終了したので、高さ（波）の値を(0 <= r <= 1)に再び戻す.
				result = (result + 1) * 0.5;
                
				return fixed4(result, 0, 0, 1);
			}
			ENDCG
		}
	}
}
