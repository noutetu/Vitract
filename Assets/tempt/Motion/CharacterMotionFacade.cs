using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharaMover))]
public class CharacterMotionFacade : MonoBehaviour
{
    private CharacterAnimator anim;   // アニメーション制御
    private CharaMover charaMover;    // 移動制御

    public void Initialize(UnityAction HitAttack, UnityAction Dead)
    {
        // コンポーネントの取得
        anim = GetComponentInChildren<CharacterAnimator>();
        charaMover = GetComponent<CharaMover>();

        // アニメーションイベントの登録
        anim.OnAttack += HitAttack;
        anim.OnDead += Dead;
    }

    public void DeInitialize(UnityAction HitAttack, UnityAction Dead)
    {
        // アニメーションイベントの解除
        anim.OnAttack -= HitAttack;
        anim.OnDead -= Dead;
    }


    public void RunMotion(float speed,bool isPlayer)
    {
        anim.RunAnim(speed / 2); // 走行アニメーションの再生
        charaMover.Move(speed * 2, isPlayer);  // キャラクターを移動させる
    }

    public void DeathMotion()
    {
        anim.DeadAnim();        // 死亡アニメーションの再生
    }

    public void IdleMotion()
    {
        anim.IdleAnim();
    }
    public void NormalAttackMotion(float attackSpeed)
    {
        anim.NormalAttackAnim(attackSpeed);
    }
}
