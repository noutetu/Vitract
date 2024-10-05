using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : MonoBehaviour,IDamageable
{
    private bool isDead;

    [SerializeField] float hp;
    [SerializeField] int cost;
    private float currentHp;
    [SerializeField] private HPBar hpBar;

    private void Start()
    {
        currentHp = hp;
        hpBar.SetHP(currentHp / hp);
    }
    public bool TakeDamageAndCheckDead(float damage)
    {
        if(isDead) {return true;}
        currentHp = Mathf.Max(currentHp - damage, 0);
        hpBar.UpdateHP(currentHp / hp);

        if (currentHp <= 0)
        {
            isDead = true;
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
