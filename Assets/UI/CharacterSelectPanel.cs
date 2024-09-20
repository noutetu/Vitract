using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPanel : MonoBehaviour
{
    [SerializeField] List<CharacterBase> characterBases;  // キャラクターの基本データリスト
    [SerializeField] List<CharacterFlame> characterFlames; // キャラクターを表示する枠

    private void Awake()
    {
        // リストのサイズをチェック
        if (characterBases.Count < 1 || characterBases.Count > 9)
        {
            Debug.LogError("Character list must have between 1 and 9 elements.");
            return;
        }

        if (characterFlames.Count < characterBases.Count)
        {
            Debug.LogError("CharacterFlame count must be at least equal to the CharacterBase count.");
            return;
        }

        // 各フレームにCharacterBaseを割り当てるメソッドを呼び出す
        AssignCharactersToFlames();
    }

    private void AssignCharactersToFlames()
    {
        for (int i = 0; i < characterBases.Count; i++)
        {
            if (characterBases[i] == null) { return; }
            // 各フレームに対応するCharacterBaseを設定
            characterFlames[i].Base = characterBases[i];
            characterFlames[i].icon.sprite = characterBases[i].Sprite;
        }
    }
}
