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

    private void InstantiateCharacter(Character prefab, HPBar uiHpBar)
{
    // キャラクターを180度回転して生成
    Character playerCharacter = Instantiate(prefab, transform.position, Quaternion.Euler(0, 180, 0), transform);

    playerCharacter.uiHpBar = uiHpBar;
    playerCharacter.SetHpBar();  // SetHpBarをここで呼び出すことで、生成直後にHPバーを設定します。
    
    playerCharacter.gameObject.tag = "Player";
    playerCharacter.gameObject.layer = LayerMask.NameToLayer("Player");
    playerCharacter.isPlayer = true;
}

}
