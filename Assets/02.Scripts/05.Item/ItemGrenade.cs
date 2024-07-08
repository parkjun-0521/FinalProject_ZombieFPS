using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemGrenade : ItemController {
    public GameObject Explosionrange;       // Æø¹ß ¹üÀ§
    public ParticleSystem explosionEffect;

    void Awake() {
        collider = GetComponent<SphereCollider>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start() {
        itemID = 3;
        damege = 100f;
    }

    void OnEnable() {
        explosionEffect.gameObject.SetActive(false);
        Explosionrange.gameObject.SetActive(false);
        StartCoroutine(OnExplosionItem());
    }

    IEnumerator OnExplosionItem() {
        yield return new WaitForSeconds(5f);
        explosionEffect.gameObject.SetActive(true);
        Explosionrange.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        rigid.velocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
