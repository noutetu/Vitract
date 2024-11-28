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
    public bool CanUseSkill { get; private set; } = true;

    // Unityのライフサイクルにおける初期化
    protected virtual void Awake()
    {
        if (skillBase != null)
        {
            Initialize(skillBase);
        }
    }

    // SkillBaseからSkillを初期化するメソッド
    public void Initialize(SkillBase skillBase)
    {
        SkillName = skillBase.SkillName;
        SkillCoolTime = skillBase.CoolTime;
        SkillValue = skillBase.Value;
        SkillIcon = skillBase.Icon;
        SkillNumber = skillBase.Number;

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

    public void Initialize(float attackCoolTime,Action<Character, IDamageable> action)
    {
         // 派生クラスでエフェクトを設定する場合、このように設定を呼び出す
        SkillEffect = action;
        SkillCoolTime = attackCoolTime;
    }

    // スキルの発動メソッド
    public void Activate(Character attacker, IDamageable target)
    {
        if (CanUseSkill)
        {
            Debug.Log($"{SkillName}を発動します！");
            SkillEffect?.Invoke(attacker, target);
            StartCooldown();
        }
        else
        {
            Debug.Log($"{SkillName}はクールダウン中です。");
        }
    }

    // クールダウン処理
    private void StartCooldown()
    {
        CanUseSkill = false;
        Observable.Timer(TimeSpan.FromSeconds(SkillCoolTime))
            .Subscribe(_ =>
            {
                CanUseSkill = true;
                Debug.Log($"{SkillName}の攻撃が再び可能です。");
            })
            .AddTo(disposables);
    }

    // クリーンアップ処理
    protected virtual void OnDestroy()
    {
        disposables.Dispose();
    }
}