using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// アニメーションの種類を表すEnum
[System.Serializable]
public enum AnimType
{
    Idle,               // 待機状態
    Run,                // 移動
    Attack,             // 攻撃
    Dead,               // 死亡
    Debuff,             // デバフ
    Buff,        // バフ    
}

public class CharacterAnimator : MonoBehaviour
{
    private Animator anim = null;
    public UnityAction OnNormal;
    public UnityAction OnSpecial;
    public UnityAction OnDead;

    public void Initialize()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("animatorが見つかりません");
            return;
        }
        Debug.Log("animatorが見つかりました");
    }

    public void PlayAnimation(AnimType animType, float speed)
    {
        if (anim == null)
        {
            Debug.LogWarning("Animatorが初期化されていません");
            return;
        }

        // 速度設定
        anim.speed = speed * GameManager.Instance.gameSpeed;

        // スイッチによるアニメーション制御
        switch (animType)
        {
            case AnimType.Idle:
                anim.SetBool("1_Move", false);
                break;

            case AnimType.Run:
                anim.SetBool("1_Move", true);
                break;

            case AnimType.Dead:
                anim.SetTrigger("4_Death");
                break;

            case AnimType.Debuff:
                anim.SetTrigger("5_Debuff");
                break;

            case AnimType.Buff:
                anim.SetTrigger("Conecntrate");
                break;

            default:
                Debug.LogWarning("指定されたアニメーションタイプが無効です");
                break;
        }
    }
    public void PlayAttackAnimation(float speed,int attackType)
    {
        if (anim == null)
        {
            Debug.LogWarning("Animatorが初期化されていません");
            return;
        }

        // 速度設定
        anim.speed = speed * 2f *GameManager.Instance.gameSpeed;
        anim.SetTrigger("2_Attack");
        anim.SetFloat("AttackType",attackType);
    }
       //---------------------------unity animationEventで呼び出すメソッド---------------------------------
    public void TriggerNormal()
    {
        if (OnNormal == null)
        {
            Debug.Log("OnAttack event is null. No subscribers to invoke.");
        }
        else
        {
            Debug.Log("OnAttack is called");
            Debug.Log($"Number of subscribers: {(OnNormal?.GetInvocationList()?.Length ?? 0)}");

            OnNormal?.Invoke();
        }
    }

    public void TriggerSpecial()
    {
        if (OnNormal == null)
        {
            Debug.Log("OnAttack event is null. No subscribers to invoke.");
        }
        else
        {
            Debug.Log("OnAttack is called");
            Debug.Log($"Number of subscribers: {(OnNormal?.GetInvocationList()?.Length ?? 0)}");

            OnSpecial?.Invoke();
        }
    }


    public void TriggerDeadAction()
    {
        OnDead?.Invoke();
    }
}
