using System;
using UnityEngine;

public class Skill
{
    public string Name { get; }
    public Action<Character, IDamageable> Effect { get; }
    public float CoolTime { get; }
    public float Value { get; }
    public Sprite Icon { get; }
    public int Number { get; }

    // SkillBaseからSkillを生成するためのコンストラクタ
    public Skill(SkillBase skillBase, Action<Character, IDamageable> effect)
    {
        Name = skillBase.SkillName; // スキル名の初期化
        CoolTime = skillBase.CoolTime; // クールタイムの初期化
        Value = skillBase.Value; // スキルの値を初期化
        Icon = skillBase.Icon; // スキルのアイコンを初期化
        Number = skillBase.Number; // スキル番号を初期化
        Effect = effect; // スキル効果を代入
    }

    // クールタイムと効果を指定するコンストラクタ
    public Skill(float coolTime, Action<Character, IDamageable> effect)
    {
        CoolTime = coolTime; // クールタイムを設定
        Effect = effect; // 効果を設定
    }

    // 攻撃を実行する際、AttackerとTargetの情報を渡す
    public void Activate(Character attacker, IDamageable target)
    {
        Debug.Log($"{Name}を発動します！");
        Effect?.Invoke(attacker, target);
    }
}

public class NormalAttack : Skill
{
    // NormalAttackのコンストラクタ
    public NormalAttack(float coolTime) : base(coolTime, (attacker, target) =>
    {
        // 通常攻撃の効果を定義
        float damage = attacker.Atk; // Attackerの攻撃力を利用
        if(target != null)
        {
            target.TakeDamage(damage); // Targetにダメージを与える
        }
    })
    {
        // 追加の初期化が必要ならここに書く
    }
}
