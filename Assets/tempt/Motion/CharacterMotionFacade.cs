using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharaMover))]
public class CharacterMotionFacade : MonoBehaviour
{
    private CharacterAnimator anim;   // アニメーション制御
    private CharaMover charaMover;    // 移動制御
    private bool isAnimating = false;

    public void Initialize(UnityAction HitAttack, UnityAction Dead)
    {
        // コンポーネントの取得
        anim = GetComponentInChildren<CharacterAnimator>();
        anim.Initialize();
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
        DOTween.Kill(this);
    }

    public void DamageAnimation(SpriteRenderer[] spriteRenderers)
    {
        if (isAnimating) return; // アニメーション中なら処理をスキップ

        isAnimating = true; // フラグを設定
        int animationsRemaining = spriteRenderers.Length; // アニメーションが終了するスプライトの数をカウント

        foreach (var spriteRenderer in spriteRenderers)
        {
            // 元の色を保持
            Color originalColor = spriteRenderer.color;

            // 点滅アニメーション (黒に変化させて戻る)
            spriteRenderer.DOColor(Color.black, 0.1f)
                .SetLoops(4, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    spriteRenderer.color = originalColor;

                    // 全てのアニメーションが終了したらフラグをリセット
                    animationsRemaining--;
                    if (animationsRemaining == 0)
                    {
                        isAnimating = false;
                    }
                });
        }
    }





    public void RunMotion(float speed, bool isPlayer)
    {
        anim.PlayAnimation(AnimType.Run,speed ); // 走行アニメーションの再生
        charaMover.Move(speed * 2, isPlayer);  // キャラクターを移動させる
    }
    
    public void PlayAnim(AnimType animType, float speed)
    {
        anim.PlayAnimation(animType,speed);
    }
    public void PlayAnim(AnimType animType, float speed,int attackType)
    {
        anim.PlayAnimation(animType,speed,attackType);
    }
}
