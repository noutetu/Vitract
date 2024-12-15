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
    Conecntrate,        // バフ    
}

public class CharacterAnimator : MonoBehaviour
{
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

            case AnimType.Conecntrate:
                anim.SetTrigger("Conecntrate");
                break;

            default:
                Debug.LogWarning("指定されたアニメーションタイプが無効です");
                break;
        }
    }
    public void PlayAnimation(AnimType animType, float speed,int attackType)
    {
        if (anim == null)
        {
            Debug.LogWarning("Animatorが初期化されていません");
            return;
        }

        // 速度設定
        anim.speed = speed * 2f *GameManager.Instance.gameSpeed;

        // スイッチによるアニメーション制御
        switch (animType)
        {
            case AnimType.Attack:
                anim.SetTrigger("2_Attack");
                anim.SetFloat("AttackType",attackType);
                break;
        }
    }
       //---------------------------unity animationEventで呼び出すメソッド---------------------------------
    public void TriggerAttackEvent()
    {
        if (OnAttack == null)
        {
            Debug.Log("OnAttack event is null. No subscribers to invoke.");
        }
        else
        {
            Debug.Log("OnAttack is called");
            Debug.Log($"Number of subscribers: {(OnAttack?.GetInvocationList()?.Length ?? 0)}");

            OnAttack?.Invoke();
        }
    }


    public void TriggerDeadAction()
    {
        OnDead?.Invoke();
    }
}
