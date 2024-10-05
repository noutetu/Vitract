using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;


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
    public UnityAction OnAttack;
    public UnityAction OnDead;


    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("animatorが見つかりません");
            return;
        }
        Debug.Log("animatorが見つかりました");

    }

    public void RunAnim(float speed)
    {
        anim.speed = speed * 3.5f;
        anim.SetFloat("RunState", speed);
    }

    public void IdleAnim()
    {
        anim.speed = 1;
        anim.SetFloat("RunState", 0f);
    }

    public void NormalAttackAnim(float attackSpeed)
    {
        anim.speed = attackSpeed;
        anim.SetTrigger("Attack");
    }
    
    public void SkillAttackAnim(float attackSpeed)
    {
        anim.speed = attackSpeed;
        anim.SetFloat("AttackState", 0);
        anim.SetTrigger("Attack");
    }
    
    public void DeadAnim()
    {
        anim.speed = 0.6f;
        anim.SetTrigger("Die");
    }

    public void DebuffAnim()
    {
        anim.speed = 1;
        anim.SetFloat ("RunState", 3);
    }

    public void StopAnimator()
    {
        anim.speed = 0;
    }

//---------------------------unity animationEventで呼び出すメソッド---------------------------------
    public void TriggerAttackEvent()
    {
        OnAttack?.Invoke();
    }
    public void TriggerDeadAction()
    {
        OnDead?.Invoke();
    }
//------------------------------------------------------------------------------------------------
}

