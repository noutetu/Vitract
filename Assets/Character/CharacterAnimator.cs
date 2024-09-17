using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterAnimator : MonoBehaviour
{
    /// <summary>
    /// 
    /// AttackState => 0 => NormalAttack
    ///                1 => SkillAttack
    ///                
    /// NormalState => 0 => SwordMan
    ///                0.5 => BowMan
    ///                1 => Magician
    ///                
    /// NormalState => 0 => SwordMan
    ///                0.5 => BowMan
    ///                1 => Magician
    ///                
    /// RunState    => 0 => Idle
    ///             => 0.1~1.2 => Run
    ///             => 1.3~ => Debuff
    ///                
    /// </summary>


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

    public void NormalAttackAnim(float attackSpeed)
    {
        anim.SetTrigger("Attack");
        anim.SetFloat("AttackSpeed",attackSpeed);
    }

    
    public void SkillAttackAnim()
    {
        anim.SetFloat("AttackState", 0);
        anim.SetTrigger("Attack");
    }
    public void DeadAnim()
    {
        anim.SetTrigger("Die");
    }
    public void DebuffAnim()
    {
        anim.SetFloat ("RunState", 3);
    }
}

