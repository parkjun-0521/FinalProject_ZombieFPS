using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemGrenade : MonoBehaviour {
    [HideInInspector]
    public Rigidbody rigid;
    [HideInInspector]
    public new Collider collider;       // 각 아이템의 콜라이더 

    public float second;
    public float explosionRadius = 5;
    public GameObject Explosionrange;       // 폭발 범위
    public ParticleSystem explosionEffect;

    
    public ItemController scriptableObject;
    void Awake() {
        collider = GetComponent<SphereCollider>();
        rigid = GetComponent<Rigidbody>();
    }

    void OnDrawGizmos()                                              //범위 확인용 추후 삭제/////////////////////////        
    {
         Gizmos.color = Color.red;                                   // 색상 설정
         Gizmos.DrawWireSphere(transform.position, explosionRadius); // 원형 범위 표시
    }



    void OnEnable() {
        explosionEffect.gameObject.SetActive(false);    // 파티클 비활성화 
        Explosionrange.gameObject.SetActive(false);     // 폭발 범위 비활성화
        StartCoroutine(OnExplosionItem());
    }

    // 폭발 로직 
    IEnumerator OnExplosionItem() {
        
        yield return new WaitForSeconds(second);
        explosionEffect.gameObject.SetActive(true);     // 파티클 활성화 
        Explosionrange.gameObject.SetActive(true);      // 범위 활성화 
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));
        
        foreach (Collider item in colls)
        {
            item.GetComponent<EnemyController>().Hp = -scriptableObject.damage;
        }

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);                    // 1초뒤 오브젝트 비활성화
    }
}
