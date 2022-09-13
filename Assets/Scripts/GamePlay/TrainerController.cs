﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    public string Name
    {
        get => name;
    }
    public Sprite Sprite
    {
        get => sprite;
    }
}
