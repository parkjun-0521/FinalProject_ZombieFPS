using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemGrenade : MonoBehaviour {
    [HideInInspector]
    public Rigidbody rigid;
    [HideInInspector]
    public new Collider collider;       // �� �������� �ݶ��̴� 

    public float second;
    public GameObject Explosionrange;       // ���� ����
    public ParticleSystem explosionEffect;


    void Awake() {
        collider = GetComponent<SphereCollider>();
        rigid = GetComponent<Rigidbody>();
    }

    void OnEnable() {
        explosionEffect.gameObject.SetActive(false);    // ��ƼŬ ��Ȱ��ȭ 
        Explosionrange.gameObject.SetActive(false);     // ���� ���� ��Ȱ��ȭ
        StartCoroutine(OnExplosionItem());
    }

    // ���� ���� 
    IEnumerator OnExplosionItem() {
        
        yield return new WaitForSeconds(second);
        explosionEffect.gameObject.SetActive(true);     // ��ƼŬ Ȱ��ȭ 
        Explosionrange.gameObject.SetActive(true);      // ���� Ȱ��ȭ 

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);                    // 1�ʵ� ������Ʈ ��Ȱ��ȭ
    }
}
