using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReflectionRipple : MonoBehaviour
{
	[SerializeField]
	private Camera ReflectionCamera;

	private new Renderer renderer;
	private Material sharedMaterial;

	private readonly int ReflectionTex = Shader.PropertyToID("_ReflTex");

	private void Start()
	{
		//反射用カメラにスクリーンと同サイズのバッファを設定し、反射用テクスチャとしてマテリアルにセットする
		renderer = GetComponent<Renderer>();
		sharedMaterial = renderer.material; //sharedMaterialという変数名にした理由が謎★
		ReflectionCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 16); //深度値16が謎★
		ReflectionCamera.targetTexture.wrapMode = TextureWrapMode.Repeat;
		sharedMaterial.SetTexture(ReflectionTex, ReflectionCamera.targetTexture);
	}

	private void OnWillRenderObject()
	{
		var cam = Camera.current;
		if (cam == ReflectionCamera)
		{
			//反射用カメラの変換行列をマテリアルにセット
			var reflL2W = renderer.localToWorldMatrix;
			var reflW2VMatrix = cam.worldToCameraMatrix;
			var reflV2PMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
			var refW2V2P = reflV2PMatrix * reflW2VMatrix;
			sharedMaterial.SetMatrix("_ReflL2W", reflL2W);
			sharedMaterial.SetMatrix("_ReflW2V2P", refW2V2P);

			if (Screen.width != ReflectionCamera.targetTexture.width || Screen.height != ReflectionCamera.targetTexture.height)
			{
				ReflectionCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 16);
				sharedMaterial.SetTexture(ReflectionTex, ReflectionCamera.targetTexture);
			}

			if (!Application.isPlaying && sharedMaterial.GetTexture(ReflectionTex) == null)
			{
				sharedMaterial.SetTexture(ReflectionTex, ReflectionCamera.targetTexture);
			}
		}
	}
}
