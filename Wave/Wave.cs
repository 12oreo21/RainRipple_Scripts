using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Wave : MonoBehaviour
{
	public Material waveMaterial;
	private Texture2D init;
	private RenderTexture input;
	private RenderTexture prev;
	private RenderTexture prev2;
	private RenderTexture result;

	//private readonly int ShaderPropertyInputTex = Shader.PropertyToID("_InputTex");
	private readonly int ShaderPropertyPrevTex = Shader.PropertyToID("_PrevTex");
	private readonly int ShaderPropertyPrev2Tex = Shader.PropertyToID("_Prev2Tex");
	private readonly int ShaderPropertyWaveTex = Shader.PropertyToID("_WaveTex");

	private new Renderer renderer;
	[SerializeField]
	private GameObject waterPlaneBack;
	private Renderer rendererB;

	[Range(1, 10)]
	public int updateFrameTiming = 3;

	

	private void Awake()
	{
		//初期化処理
		//入力初期化用のテクスチャを作っておく
		//SetPixelの座標を(0,0)に設定してあるが謎。他の座標の値は設定しなくていいの？★
		init = new Texture2D(1, 1);
		init.SetPixel(0, 0, new Color(0, 0, 0, 0));
		init.Apply();

		//入力用テクスチャを取得し、波動方程式を求めるのに必要なバッファを生成
		//高さ（波）の値のみ扱えればいいので、Rチャネルのみ使う（ちなみにR8は、Rチャネルの8ビット（0~256）という意味）
		//RenderTextureのdepthは0で設定してあるが謎★
		input = waveMaterial.GetTexture("_InputTex") as RenderTexture;
		prev = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.R8);
		prev2 = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.R8);
		result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.R8);
        
		//バッファの初期化（初期値の代入）
		//SetPixelの座標を(0,0)に設定してあるが謎。他の座標の値は設定しなくていいの？★
        //波の高さは最終的に(0<=r<-1)で表すため、高さ0とは、r=0.5。
		var r8Init = new Texture2D(1, 1);
		r8Init.SetPixel(0, 0, new Color(0.5f, 0, 0, 1));
		r8Init.Apply();
		Graphics.Blit(r8Init, prev);
		Graphics.Blit(r8Init, prev2);

		renderer = GetComponent<Renderer>();
		rendererB = waterPlaneBack.transform.GetComponent<Renderer>();
	}


	private void OnWillRenderObject()
	{
		WaveUpdate();
	}

	private void WaveUpdate()
	{
		if (Time.frameCount % updateFrameTiming != 0)
        {
			return;
		}
		
		if (input == null)
        {	
			return;
		}

		//waveMaterial.SetTexture(ShaderPropertyInputTex, input);
		waveMaterial.SetTexture(ShaderPropertyPrevTex, prev);
		waveMaterial.SetTexture(ShaderPropertyPrev2Tex, prev2);

		//上記3つのtextureを元にWaveShaderで波動方程式を解いてresultに格納
		Graphics.Blit(null, result, waveMaterial);

		//それぞれ１つ先の時点での高さ（波）の値に更新
		var tmp = prev2;
		prev2 = prev;
		prev = result;
		result = tmp;

		//入力用テクスチャを初期化
		//Graphics.Blit(init, input);

		renderer.sharedMaterial.SetTexture(ShaderPropertyWaveTex, prev);
		rendererB.material.SetTexture(ShaderPropertyWaveTex, prev);
	}
}