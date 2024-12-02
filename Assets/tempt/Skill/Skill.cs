using System;
using UniRx;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] private SkillBase skillBase; // Unity上で設定するSkillBase
    private CompositeDisposable disposables = new CompositeDisposable();

    public string SkillName { get; private set; }
    public Action<Character, IDamageable> SkillEffect { get; protected set; }
    public float SkillCoolTime { get; private set; }
    public float SkillValue { get; private set; }
    public Sprite SkillIcon { get; private set; }
    public int SkillNumber { get; private set; }
    public ReactiveProperty<bool> CanUseSkill { get; private set; } = new ReactiveProperty<bool>(true);

    // Unityのライフサイクルにおける初期化
    public virtual void Awake()
    {
        if (skillBase != null)
        {
            SpecialInitialize(skillBase);
        }
    }

    // SkillBaseからSkillを初期化するメソッド
    public void SpecialInitialize(SkillBase skillBase)
    {
        SkillName = skillBase.SkillName;
        SkillCoolTime = skillBase.CoolTime;
        SkillValue = skillBase.Value;
        SkillIcon = skillBase.Icon;
        SkillNumber = skillBase.Number;

        Debug.Log($"Special Initialize は{SkillCoolTime}");

        // 派生クラスでエフェクトを設定する場合、このように設定を呼び出す
        SkillEffect = (attacker, target) =>
        {
            float damage = skillBase.Value;
            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log($"{SkillName}が{target}に{damage}のダメージを与えました。");
            }
        };

    }

    public void NormalInitialize(float attackCoolTime,Action<Character, IDamageable> action)
    {
         // 派生クラスでエフェクトを設定する場合、このように設定を呼び出す
        SkillEffect = action;
        SkillCoolTime = attackCoolTime;
        Debug.Log($"Normal Initialize は{SkillCoolTime}");
    }

    // スキルの発動メソッド
    public void Activate(Character attacker, IDamageable target)
    {
        if (CanUseSkill.Value)
        {
            Debug.Log($"{SkillName}を発動します！");
            StartCooldown();
            SkillEffect?.Invoke(attacker, target);
        }
        else
        {
            Debug.Log($"{SkillName}はクールダウン中です。");
        }
    }

    // クールダウン処理
private void StartCooldown()
{
    // スキルを使用不可能に設定
    CanUseSkill.Value = false;
    Debug.Log($"CanUseSkillは{CanUseSkill}です");

    // クールダウンが完了したときに攻撃可能にする購読
    Observable.Timer(TimeSpan.FromSeconds(SkillCoolTime / GameManager.Instance.gameSpeed))
        .Subscribe(_ =>
        {
            // スキルを使用可能に設定
            CanUseSkill.Value = true;
            Debug.Log("攻撃が再び可能です");
        })
        .AddTo(this); // 購読を破棄管理に追加
}


    // クリーンアップ処理
    protected virtual void OnDestroy()
    {
        disposables.Dispose();
    }
}