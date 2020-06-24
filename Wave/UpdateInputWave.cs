using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateInputWave : MonoBehaviour
{
	[SerializeField]
	private Camera MainCamera;
	private ParticleSystem particle;
	private List<ParticleCollisionEvent> collisionEventList = new List<ParticleCollisionEvent>();
    [SerializeField]
	private Material waveMaterial;
	[SerializeField]
	private GameObject waterPlaneForInputWave;
	private float waterPlaneSize;
	public float radius;
	private int i = 0;
	Vector4 pos2uv;
	Vector4[] posArray = new Vector4[15];

	void Awake()
    {
        
	}

    void Start()
	{
		particle = GetComponent<ParticleSystem>();
		radius /= waterPlaneForInputWave.transform.localScale.x;
		waveMaterial.SetFloat("_Radius", radius);
		waterPlaneSize = waterPlaneForInputWave.transform.localScale.x;
	}
	Vector4 a;

	void OnParticleCollision(GameObject other)
	{
		if (other.tag == "WaterPlane")
		{
			//　イベントの取得
			particle.GetCollisionEvents(other, collisionEventList);

			//パーティクルの衝突地点を取得し、ワールド座標からWaterPlane（厳密には全く同サイズのWaterPlaneForInputWave）のUV座標に変換
			//例えば、WaterPlaneのScale(x,z)=(1,1)の時、ワールド座標では1辺の長さが10になる
			//つまりこの例でいくと、WaterPlaneのZ軸がワールド座標のZ軸と同じ時、ワールド座標(-5,-5)(5,5)が、WaterPlaneのUV座標(0,0)(1,1)になる
			foreach (var collisionEvent in collisionEventList)
			{
				//WaterPlane（厳密にはWaterPlaneForInputWave）座標系の衝突地点（ローカル座標）
				Vector3 pos = collisionEvent.intersection - waterPlaneForInputWave.transform.position;
                if (MainCamera.transform.position.y >= 0)
                {
					pos2uv = new Vector4(1f - (pos.x + (waterPlaneSize * 5f)) / (waterPlaneSize * 10f), 1f - (pos.z + (waterPlaneSize * 5f)) / (waterPlaneSize * 10f),0,0); 

				}
                else if (MainCamera.transform.position.y < 0)
                {
					//pos2uv = new Vector4((pos.x + (waterPlaneSize * 5f)) / (waterPlaneSize * 10f), (pos.z + (waterPlaneSize * 5f)) / (waterPlaneSize * 10f),0,0);
					pos2uv = new Vector4(1f - (pos.x + (waterPlaneSize * 5f)) / (waterPlaneSize * 10f), 1f - (pos.z + (waterPlaneSize * 5f)) / (waterPlaneSize * 10f), 0, 0);
				}
				posArray[i] = pos2uv;
				i += 1;
                if (i >= posArray.Length)
                {
					waveMaterial.SetVectorArray("_HitPos2UVArray", posArray);
					i = 0;
				}	
			}
		}
	}
}
