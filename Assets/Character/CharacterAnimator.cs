using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterAnimator : MonoBehaviour
{

    private Animator anim = null;


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void RunAnim(float speed)
    {
        anim.SetFloat("RunState", speed);
    }

    public void IdleAnim()
    {
        anim.SetFloat("RunState", 0.0f);
    }

    public void NormalAttackAnim()
    {
        anim.SetTrigger("Attack");
    }
    public void SkillAttackAnim()
    {
        anim.SetFloat("AttackState", 2);
        anim.SetTrigger("Attack");
    }
    public void DeadAnim()
    {
        anim.SetTrigger("Die");
    }
    public void DebuffAnim()
    {

    }
}

