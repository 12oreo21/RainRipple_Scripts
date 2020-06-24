using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentRipple : MonoBehaviour
{
	[SerializeField]
	private Camera MainCamera;

	private new Renderer renderer;
	private Material material;

	private void Start()
	{
		renderer = GetComponent<Renderer>();
		material = renderer.material;
	}

	private void OnWillRenderObject()
	{
		var cam = Camera.current;
		if (cam == MainCamera)
		{
			//反射用カメラの変換行列をマテリアルにセット
			var transL2W = renderer.localToWorldMatrix;
			material.SetMatrix("_TranslL2W", transL2W);
		}
	}
}
