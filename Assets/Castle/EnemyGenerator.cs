using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] CharacterBase unit000;
    [SerializeField] CharacterBase unit001;
    [SerializeField] CharacterBase unit002;

    public void MakeUnit000()
    {
        MakeUnit(unit000);
    }

    public void MakeUnit001()
    {
        MakeUnit(unit001);
    }
    public void MakeUnit002()
    {
        MakeUnit(unit002);
    }

    private void MakeUnit(CharacterBase unit)
    {
        Character enemyCharacter = Instantiate(unit.Prefab, transform);
        enemyCharacter.gameObject.tag = "Enemy";
        enemyCharacter.gameObject.layer = LayerMask.NameToLayer("Enemy");
        enemyCharacter.isPlayer = false;
        SmoothApper(enemyCharacter);
    }

    private static void SmoothApper(Character enemyCharacter)
    {
        // 透明からフェードインさせるために全てのSpriteRendererの透明度を設定
        SpriteRenderer[] spriteRenderers = enemyCharacter.GetComponentsInChildren<SpriteRenderer>();

        foreach (var spriteRenderer in spriteRenderers)
        {
            // 最初は完全に透明に設定
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            // 透明度を1（完全に表示）まで1秒かけてフェードイン
            spriteRenderer.DOFade(1f, 2f);  // 1秒かけてフェードイン
        }
    }
}
