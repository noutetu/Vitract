using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// --------------- 通常攻撃 ---------------
[CreateAssetMenu(fileName = "NormalAttack", menuName = "Skill/NormalAttack")]
public class NormalAttack : SkillData
{

    public override void Activate(Character character, IDamageable target)
    {
        
            float attackValue = character.Atk * Value / 100;
            CooldownTime = character.AttackCoolTime;  // キャラクターの攻撃速度を取得

            StartCoolDown();
            target.TakeDamage(attackValue);
            Debug.Log($"{SkillName}を使って{target}に{attackValue}のダメージを与えました。");
        
    }
}
