using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterFlame : MonoBehaviour
{
    public UnityAction<Character,HPBar> OnTouch;
    public CharacterBase Base;
    private Character prefab;
    public Image icon;
    private HPBar uiHpBar;

    private void Start()
    {
        uiHpBar = GetComponentInChildren<HPBar> ();
        
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
        if (prefab == null) { return; }
        if (OnTouch != null)
        {
            Debug.Log("OnTouch has subscribers."); // イベントにリスナーが登録されている
            OnTouch.Invoke(prefab,uiHpBar); // イベントを呼び出す
        }
        else
        {
            Debug.Log("OnTouch has no subscribers."); // イベントにリスナーが登録されていない
        }
    }
}
