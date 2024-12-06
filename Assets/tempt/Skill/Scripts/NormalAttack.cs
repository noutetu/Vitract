using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --------------- 通常攻撃 ---------------
[CreateAssetMenu(fileName = "NormalAttack", menuName = "Skill/NormalAttack")]
public class NormalAttack : SkillData
{
    
    public override void Activate(Character character, IDamageable target)
    {
        if (CanUseSkill.Value && target is not null)
        {
            float attackValue = character.Atk * value / 100;
            cooldownTime = character.AttackCoolTime;  // キャラクターの攻撃速度を取得

            StartCoolDown();
            target.TakeDamage(attackValue);
            Debug.Log($"{skillName}を使って{target}に{attackValue}のダメージを与えました。");
        }
    }
}
