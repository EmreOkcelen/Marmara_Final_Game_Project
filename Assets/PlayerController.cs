using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float moveSpeed = 5f; // Hareket hızı
    public float rotationSpeed = 720f; // Dönme hızı
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
        Application.targetFrameRate = 60;
    }
    void Update()
    {
        // Hareket girişi al
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        anim.SetFloat("speed", MathF.Abs(verticalInput)+MathF.Abs(horizontalInput));
        // Yön bul ve hareket et
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput).normalized;
        if (direction.magnitude >= 0.01f)
        {
            // Hareket et
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Dönüşü hesapla
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    }
