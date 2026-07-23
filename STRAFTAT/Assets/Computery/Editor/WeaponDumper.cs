using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDump {
    public Dictionary<string, int> WeaponCounts { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> VendingMachineOutputs { get; set; } = new Dictionary<string, int>();
}

public static class WeaponDumper {
    [MenuItem("Computery/Dump Weapons")]
    public static void DumpWeapons() {
        string path = EditorUtility.SaveFilePanel("Save Weapon Dump", "", "weapon_dump.txt", "txt");
        if (string.IsNullOrEmpty(path)) { Debug.Log("Weapon dump cancelled."); return; }
        
        Dictionary<string, SceneDump> sceneWeaponCounts = new Dictionary<string, SceneDump>();
        
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        for (int i = 6; i < totalScenes; i++) {
            EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(i));
            
            SceneDump dump = new SceneDump();
            WeaponDropper[] droppers = Object.FindObjectsOfType<WeaponDropper>();
            foreach (WeaponDropper dropper in droppers) {
                foreach (GameObject weapon in dropper.itemsToSpawn) {
                    if (!dump.WeaponCounts.TryAdd(weapon.name, 1)) { dump.WeaponCounts[weapon.name]++; }
                }
            }
            
            ItemSpawner[] spawners = Object.FindObjectsOfType<ItemSpawner>();
            foreach (ItemSpawner spawner in spawners) {
                GameObject weapon = spawner.itemToSpawn;
                if (!dump.WeaponCounts.TryAdd(weapon.name, 1)) { dump.WeaponCounts[weapon.name]++; }
            }
            
            ItemDispenser[] dispensers = Object.FindObjectsOfType<ItemDispenser>();
            foreach (ItemDispenser dispenser in dispensers) {
                foreach (GameObject weapon in dispenser.itemsToSpawn) {
                    if (!dump.VendingMachineOutputs.TryAdd(weapon.name, 1)) { dump.VendingMachineOutputs[weapon.name]++; }
                }
            }
            
            sceneWeaponCounts[SceneManager.GetActiveScene().name] = dump;
        }

        string json = JsonConvert.SerializeObject(sceneWeaponCounts, Formatting.Indented);
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"Weapon dump saved to {path}");
    }
}