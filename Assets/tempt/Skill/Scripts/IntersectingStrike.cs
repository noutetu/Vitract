using System.Collections;
using UnityEngine;

// --------------- 交錯の一撃 ---------------
[CreateAssetMenu(fileName = "IntersectingStrike", menuName = "Skill/IntersectingStrike")]
public class IntersectingStrike : SkillData
{
    public float moveDistance = 2.0f; // 前進距離
    public float retreatDistance = 1.0f; // 後退距離
    public float moveSpeed = 10.0f; // キャラクターの移動速度

    public override void Activate(Character character, IDamageable target)
    {
        if (character == null) return;

        // ダメージ計算
        float attackValue = character.Atk * Value/100;

        // コルーチンを用いて移動処理とダメージを適用
        character.StartCoroutine(PerformIntersectingStrike(character, target, attackValue));

        // クールタイム開始
        StartCoolDown();
    }

    private IEnumerator PerformIntersectingStrike(Character character, IDamageable target, float attackValue)
    {
        Vector3 startPosition = character.transform.position; // 元の位置を保存
        Vector3 forwardPosition = startPosition + character.transform.right * moveDistance; // 一定距離前進
        Vector3 retreatPosition = startPosition - character.transform.right * retreatDistance; // 一定距離後退

        // 前進
        yield return MoveCharacter(character, forwardPosition);

        // ダメージを与える（敵が存在する場合のみ）
        if (target != null)
        {
            target.TakeDamage(attackValue);
            Debug.Log($"{SkillName}を使って{target}に{attackValue}のダメージを与えました。");
        }

        // 後退
        yield return MoveCharacter(character, retreatPosition);
    }

    private IEnumerator MoveCharacter(Character character, Vector3 targetPosition)
    {
        while (Vector3.Distance(character.transform.position, targetPosition) > 0.1f)
        {
            character.transform.position = Vector3.MoveTowards(
                character.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null; // フレームを待機
        }
    }
}
