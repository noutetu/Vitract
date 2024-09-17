using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterAnimator : MonoBehaviour 
{
  
    private Animator anim = null;


    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            anim.SetFloat("RunState", 0.5f);

        }

        if (Input.GetKey(KeyCode.S))
        {
            anim.SetFloat("RunState", 0.0f);

        }

        if (Input.GetKey(KeyCode.B))
        {
            anim.SetTrigger("Attack");
        }

    }
}

         