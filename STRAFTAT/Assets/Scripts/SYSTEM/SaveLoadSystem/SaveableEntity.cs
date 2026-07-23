using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id;
    public string Id => id;

    [ContextMenu("Generate Id")] 
    private void GenerateId()
    {
        id = Guid.NewGuid().ToString();
    }

    // find all ISaveable components on gameobject

    public object SaveState()
    {
        var state = new Dictionary<string, object>();
        foreach (var saveable in GetComponents<ISaveable>())
        {
            state[saveable.GetType().ToString()] = saveable.SaveState();
        }
        return state;
    }

    public void LoadState(JContainer state)
    {
        Dictionary<string, JContainer> stateDictionary = state.ToObject<Dictionary<string, JContainer>>();

        foreach (var saveable in GetComponents<ISaveable>())
        {
            string typeName = saveable.GetType().ToString();
            if (stateDictionary.TryGetValue(typeName, out JContainer savedState))
            {
                saveable.LoadState(savedState);
            }
        }
    }
}
