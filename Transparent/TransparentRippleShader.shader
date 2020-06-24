Shader "Unlit/TransparentRippleShader"
{
    Properties
    {
        _WaveTex("Wave",2D) = "gray" {}
        _BumpMap("Normalmap", 2D) = "bump" {}
        _ChangeAmt("BumpAmt", Range(0,9999)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 100
        Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

        GrabPass
        {
            
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            
            sampler2D _WaveTex;
            float4 _WaveTex_TexelSize;
            sampler2D _BumpMap;
            float _ChangeAmt;
            sampler2D _GrabTexture;
            float4x4 _TranslL2W;
		


            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = o.vertex;
                o.worldPos = mul(_TranslL2W, v.vertex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                //常時小波発生用の法線マップからx,y方向の歪みを取得.（波を遷移させるため、_Timeを掛ける)
			    float3 bump = UnpackNormal(tex2D( _BumpMap, i.uv + _Time.x)).rgb;

			    //波動方程式の解を_WaveTexで受け取り、波による歪み具合をbumpに代入.
			    //_WaveTexは波の高さなので、高さの変化量から接ベクトル（接平面）を求め、接ベクトルから法線を求める
			    float2 shiftX = { _WaveTex_TexelSize.x,  0 };
			    float2 shiftZ = { 0, _WaveTex_TexelSize.y };
                //_WaveTexから渡される値（result）はR8（0 <= r <= 1）なので、(-1 <= r <= 1)に変換し、高さ（波）の値を復元
			    float3 texX = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy + shiftX,0,0)) - 1;
			    float3 texx = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy - shiftX,0,0)) - 1;
			    float3 texZ = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy + shiftZ,0,0)) - 1;
			    float3 texz = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy - shiftZ,0,0)) - 1;
                //XY平面・YZ平面からそれぞれ接ベクトルを求めるが、なぜそれぞれxとzの値が１で固定なのか謎★..
			    float3 du = { 1, _ChangeAmt*(texX.x - texx.x)/2, 0 };
			    float3 dv = { 0, _ChangeAmt*(texZ.x - texz.x)/2, 1 };
                //2つの接ベクトルから法線ベクトルを求める.
			    bump += normalize(cross(du, dv));
                //歪みを適用。なぜ頂点座標のz値に掛けてから足しているのか謎★.歪めばなんでもいいのかな?.
                i.grabPos.xy += (bump * i.grabPos.z);
                //クリップ空間座標系の頂点座標を、プロジェクション座標空間に変換。その後、uv座標に変換。
                //GrabPassはプロジェクション空間にてオブジェクトより深度値が大きい空間に描画されたものを全てテクスチャとして取得する
                //つまり、スクリーンの左下(-1,-1)・右上(1,1)。UZ座標(0~1,0~1)に変換
                float2 grabUV = i.grabPos / i.grabPos.w * 0.5 + 0.5;
                fixed4 col = tex2D(_GrabTexture, 1-grabUV);

                float3 lightDir = normalize(_WorldSpaceLightPos0 - i.vertex);
                float diff = max(0, dot(bump, lightDir)) + 1.0;
                col *= diff;

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float NdotL = dot(bump, lightDir);
                float3 refDir = -lightDir + (2.0 * bump * NdotL);
                //pow関数で非線形にし、より入射ベクトルと反射ベクトルの違いを強調
                float spec = pow(max(0, dot(viewDir, refDir)), 10.0);
                col += spec;

                return col;
            }
            ENDCG
        }
    }
}
