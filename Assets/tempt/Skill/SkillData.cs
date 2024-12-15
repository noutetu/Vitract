using UnityEngine;
using UniRx;
using System;

[Serializable]
public enum SkillType
{
    Attack,
    Heal,
    Buff,
    Debuff,
}
public interface ISkillStrategy
{
    void Activate(Character character, IDamageable target);
    IReadOnlyReactiveProperty<bool> CanUseSkill { get; }
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill/BasicSkill")]
public abstract class SkillData : ScriptableObject, ISkillStrategy
{
    [Header("基本情報")]
    [Tooltip("スキルの名前")]
    [SerializeField] private SkillType skillType;
    public SkillType SkillType { get => skillType;}

    [Tooltip("スキル説明")]
    [TextArea]
    [SerializeField] string skillDescription;

    [Tooltip("スキルの倍率や影響度")]
    [SerializeField] private float value;
    public float Value { get => value;}

    [Tooltip("スキルのクールダウン時間")]
    [SerializeField] private float cooldownTime;
    public float CooldownTime { get => cooldownTime; set => cooldownTime = value; }

    [Tooltip("スキルのアイコン")]
    [SerializeField] private Sprite icon;
    public Sprite Icon { get => icon; set => icon = value; }

    [Tooltip("スキルの発動時の音")]
    [SerializeField] private AudioClip attackSound;
    public AudioClip AttackSound { get => attackSound; set => attackSound = value; }


    [Tooltip("スキルの発動時アニメーション")]
    [SerializeField] private int attackType;
    public int AttackType { get => attackType; }

    [Tooltip("スキルのアニメーションタイプ")]
    [SerializeField] private AnimType animType;
    public AnimType AnimType { get => animType; }


    [Header("基本情報")]
    [Tooltip("スキルの種類")]
    [SerializeField] private string skillName;
    public string SkillName { get => skillName; set => skillName = value; }
    
    [Header("状態管理")]
    [Tooltip("スキルが使用可能かどうか")]
    private ReactiveProperty<bool> canUseSkill = new ReactiveProperty<bool>(true);
    public IReadOnlyReactiveProperty<bool> CanUseSkill => canUseSkill;

    [Header("クールダウン管理")]
    [Tooltip("クールダウン中の管理用")]
    private CompositeDisposable cooldownDisposables = new CompositeDisposable();

    /// <summary>
    /// スキルをアクティブ化する抽象メソッド
    /// </summary>
    /// <param name="character">スキルを使用するキャラクター</param>
    /// <param name="target">スキルのターゲット</param>
    public abstract void Activate(Character character, IDamageable target);

    /// <summary>
    /// クールダウンを開始する
    /// </summary>
    protected void StartCoolDown()
    {
        if (!canUseSkill.Value) return; // 既にクールダウン中の場合は何もしない
        canUseSkill.Value = false;
        cooldownDisposables.Clear(); // 既存のタイマーをクリア

        Observable.Timer(TimeSpan.FromSeconds(cooldownTime / GameManager.Instance.gameSpeed))
            .Subscribe(_ =>
            {
                canUseSkill.Value = true;
            })
            .AddTo(cooldownDisposables);
    }
}
