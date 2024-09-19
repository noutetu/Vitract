using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterFlame : MonoBehaviour
{
    public UnityAction<Character> OnTouch;
    public CharacterBase Base;
    private Character prefab;

    private void Start()
    {
        if (Base == null)
        {
            Debug.LogError("Base is not assigned in CharacterFlame.");
            return;
        }
        if (Base.Prefab == null)
        {
            Debug.LogError("prefab is not assigned in CharacterFlame.");
            return;
        }
        prefab = Base.Prefab;

    }
    public void PressButton()
    {
        Debug.Log("タッチできなかった");
        if (prefab == null) { return; }
        Debug.Log("タッチできた");
        OnTouch(prefab);
    }
}
