using DG.Tweening;  // DOTweenを使用するための名前空間
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{
    [SerializeField] List<CharacterFlame> characterFlames;

    private void Start()
    {
        for (int i = 0; i < characterFlames.Count; i++)
        {
            characterFlames[i].OnTouch += InstantiateCharacter;
        }
    }

    private void InstantiateCharacter(Character prefab)
    {
        // キャラクターを180度回転して生成
        Character playerCharacter = Instantiate(prefab, transform.position, Quaternion.Euler(0, 180, 0), transform);
        playerCharacter.gameObject.tag = "Player";
        playerCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
        playerCharacter.isPlayer = true;

        SmoothApper(playerCharacter);
    }
    private static void SmoothApper(Character playerCharacter)
    {
        // 透明からフェードインさせるために全てのSpriteRendererの透明度を設定
        SpriteRenderer[] spriteRenderers = playerCharacter.GetComponentsInChildren<SpriteRenderer>();

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
