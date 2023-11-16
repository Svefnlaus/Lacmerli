using System.Collections;
using UnityEngine;

public class Magic : MonoBehaviour
{
    [SerializeField] private GameObject sprite;
    [SerializeField] private GameObject secondBlade;

    private ParticleSystem particle;

    public float damage;
    public float chargeTime;

    public bool processing;

    private void Start()
    {
        particle = GetComponentInChildren<ParticleSystem>();

        sprite.GetComponent<Hitbox>().damage = damage;
        if (secondBlade != null) secondBlade.GetComponent<Hitbox>().damage = damage / 3;

        var charge = particle.main;
        charge.duration = chargeTime;
    }

    private void Update()
    {
        if (processing) return;
        sprite.SetActive(false);
        if (secondBlade != null) secondBlade.SetActive(false);
        StartCoroutine(ChargeAndShoot());
    }

    public IEnumerator ChargeAndShoot()
    {
        processing = true;
        particle.Play();
        yield return new WaitForSeconds (chargeTime * 2);
        sprite.SetActive(true);
        if (secondBlade != null) secondBlade.SetActive(true);
    }
}
