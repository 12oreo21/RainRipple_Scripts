Shader "Unlit/ReflectionRippleShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ReflTex("Refl",2D) = "black" {}
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
                float4 reflPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReflTex;
            float4x4 _ReflL2W;
            float4x4 _ReflW2V2P;
            sampler2D _WaveTex;
            float4 _WaveTex_TexelSize;
            sampler2D _BumpMap;
            float _ChangeAmt;
		


            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                //各頂点をローカル空間（モデル空間）から、反射用カメラのプロジェクション空間（厳密にはクリップ空間）に変換
                o.reflPos = mul(_ReflW2V2P, mul(_ReflL2W, v.vertex));
                o.worldPos = mul(_ReflL2W, v.vertex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                //常時小波発生用の法線マップからx,y方向の歪みを取得.（波を遷移させるため、_Timeを掛ける）
			    float3 bump = UnpackNormal(tex2D( _BumpMap, i.uv + _Time.x)).rgb;

			    //波動方程式の解を_WaveTexで受け取り、波による歪み具合をbumpに代入.
			    //_WaveTexは波の高さなので、高さの変化量から接ベクトル（接平面）を求め、接ベクトルから法線を求める
			    float2 shiftX = { _WaveTex_TexelSize.x,  0 };
			    float2 shiftZ = { 0, _WaveTex_TexelSize.y };
                //_WaveTexから渡される値（result）はR8（0 <= r <= 1）なので、(-1 <= r <= 1)に変換し、高さ（波）の値を復元.
			    float3 texX = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy + shiftX,0,0)) - 1;
			    float3 texx = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy - shiftX,0,0)) - 1;
			    float3 texZ = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy + shiftZ,0,0)) - 1;
			    float3 texz = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy - shiftZ,0,0)) - 1;
                //XY平面・YZ平面からそれぞれ接ベクトルを求める。ベクトル＝傾きで、x,zの変化量が１の時のyの変化量。
			    float3 du = { 1, _ChangeAmt*(texX.x - texx.x)/2, 0 };
			    float3 dv = { 0, _ChangeAmt*(texZ.x - texz.x)/2, 1 };
                //2つの接ベクトルから法線ベクトルを求める.
			    bump += normalize(cross(du, dv));
                //歪みを適用。なぜ頂点座標のz値に掛けてから足しているのか謎★.歪めばなんでもいいのかな?
                i.reflPos.xy += (bump * i.reflPos.z);

                //クリップ座標空間を正式にプロジェクション空間に変換し、その座標を元にテクスチャを適用.
                fixed4 col = tex2D(_ReflTex, i.reflPos.xy / i.reflPos.w * 0.5 + 0.5);

                //ライトの位置と法線から陰影をつける
                float3 lightDir = normalize(_WorldSpaceLightPos0 - i.worldPos);
                float diff = max(0, dot(bump, lightDir)) + 1.0;
                col *= diff;

                
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float NdotL = dot(bump, lightDir);
                float3 refDir = -lightDir + (2.0 * bump * NdotL);
                //pow関数で非線形にし、より入射ベクトルと反射ベクトルの違いを強調
                float spec = pow(max(0, dot(viewDir, refDir)), 10.0);
                col += spec * 0.0001;
                
                col.a = 0.3;
                return col;
            }
            ENDCG
        }
    }
}
