using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --------------- ジャンプアタック ---------------
[CreateAssetMenu(fileName = "JumpAttack", menuName = "Skill/JumpAttack")]
public class JumpAttack : SkillData
{
    public override void Activate(Character character, IDamageable target)
    {
        float attackValue = character.Atk * Value / 100;
        target.TakeDamage(attackValue);
        StartCoolDown();

    }
}
