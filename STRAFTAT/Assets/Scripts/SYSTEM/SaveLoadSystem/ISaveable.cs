using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public interface ISaveable
{
    object SaveState();
    void LoadState(JContainer state);
}
