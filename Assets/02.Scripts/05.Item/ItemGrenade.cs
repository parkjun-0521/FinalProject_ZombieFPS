using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemGrenade : ItemController {
    public float second;
    public GameObject Explosionrange;       // ���� ����
    public ParticleSystem explosionEffect;

    void Awake() {
        collider = GetComponent<SphereCollider>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start() {
        itemID = 3;         // ������ ID
        damege = 0.2f;      // �ۼ�Ʈ ������
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
