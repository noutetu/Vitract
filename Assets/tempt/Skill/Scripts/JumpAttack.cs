using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --------------- ジャンプアタック ---------------
[CreateAssetMenu(fileName = "JumpAttack", menuName = "Skill/JumpAttack")]
public class JumpAttack : SkillData
{
    public override void Activate(Character character, IDamageable target)
    {
        float attackValue = character.Atk * value / 100;
        target.TakeDamage(attackValue);
        Debug.Log($"{skillName}を使って{target}に{attackValue}のダメージを与えました。");
        StartCoolDown();
    }
}
