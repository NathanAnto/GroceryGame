﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float speed = 6f;
    private CharacterController controller;
    private Animator animator;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 5;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;

        if (dir.magnitude >= 0.1f)
        {
            controller.Move(dir * speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
            animator.Play("Running");
        }
        else animator.Play("Idle");

        if (timer <= 0) {
            StopCoroutine(StartRegisterTimer());
            timer = 0;
        }
}
    
    public IEnumerator StartRegisterTimer()
    {
        print(timer);
        timer--;
        yield return new WaitForSeconds(1);
    }
}
