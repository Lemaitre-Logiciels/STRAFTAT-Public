using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Sirenix.OdinInspector;

[System.Serializable]
public class ProgressInstance
{
    [BoxGroup("Progress Instance", centerLabel: true)] public int xpToUnlock;
    [BoxGroup("Progress Instance", centerLabel: true)] public CosmeticInstance cosmetic;
    [BoxGroup("Progress Instance", centerLabel: true)] public string[] maps;
    [BoxGroup("Progress Instance", centerLabel: true)] public bool unlocked;
    [BoxGroup("Progress Instance", centerLabel: true)] public bool dlcExlusive;
    [HideInInspector] public int index;

}
