using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Sirenix.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ComputerysModdingUtilities {
    public static partial class AssemblyScanner {
        private static readonly Dictionary<string, Assembly> AssemblyNameToAssembly = new();
        private static readonly string[] BepInExAssemblyNames = {
            "0Harmony",
            "0Harmony20",
            "BepInEx",
            "BepInEx.Harmony",
            "BepInEx.Preloader",
            "HarmonyXInterop",
            "Mono.Cecil",
            "Mono.Cecil.Mdb",
            "Mono.Cecil.Pdb",
            "Mono.Cecil.Rocks",
            "MonoMod.RuntimeDetour",
            "MonoMod.Utils"
        };
        
        #if UNITY_STANDALONE_OSX
        private static readonly string ManagedFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "Resources", "Data", "Managed"));
        #else
        private static readonly string ManagedFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "Managed"));
        #endif
        
        private static List<Assembly> GetForeignAssemblies() {
            List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            for (int i = assemblies.Count - 1; i >= 0; i--) {    
                Assembly assembly = assemblies[i];
                
                if (assembly.IsDynamic) { assemblies.RemoveAt(i); continue; }
                
                string assemblyLocation = assembly.Location;
                string assemblyFullName = assembly.GetName().FullName;
                string assemblyName = assembly.GetName().Name;

                if (string.IsNullOrEmpty(assemblyLocation) || assemblyLocation.StartsWith(ManagedFolder) || BepInExAssemblyNames.Contains(assemblyName)) {
                    assemblies.RemoveAt(i);
                    continue;
                }
                
                AssemblyNameToAssembly[assemblyFullName] = assembly;
            }
            return assemblies;
        }

        public static string MatchMakingKey { get; private set; } = string.Empty;
        public static string[] AllModNames = Array.Empty<string>();
        public static string[] IncompatibleAssemblyNames = Array.Empty<string>();
        public static string[] IncompatibleAssemblyNamesTrimmed = Array.Empty<string>();
        public static void CreateMatchMakingKey() {
            #if UNITY_EDITOR
                return; // We don't need to check or do anything in the editor.
            #endif

            List<Assembly> foreignAssemblies = GetForeignAssemblies();
            BuildTree(foreignAssemblies);
            
            AllModNames = foreignAssemblies
                .Select(assembly => assembly.GetName().Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray();
            
            List<Assembly> incompatibleAssemblies = new(foreignAssemblies); // we could just iternatr backwards over the foreign assemblies, but this readable
            List<Assembly> compatibleAssemblies = new();
      
            foreach (Assembly assembly in foreignAssemblies) {
                StraftatModAttribute modAttribute = assembly.GetCustomAttribute<StraftatModAttribute>();
                
                if (modAttribute == null || !modAttribute.IsVanillaCompatible) { continue; } // Check for incompatibility. 
                
                // If the assembly is marked as compatible with the vanilla game, we need also allow all its dependencies to be compatible.
                if (AssemblyNameToAssembly.TryGetValue(assembly.GetName().FullName, out Assembly loadedAssembly)) { GetAllDependencies(loadedAssembly, compatibleAssemblies); }
            }
            
            foreach (Assembly removedAssembly in compatibleAssemblies) {
                if (incompatibleAssemblies.Contains(removedAssembly)) { incompatibleAssemblies.Remove(removedAssembly); }
            }
            
            if (incompatibleAssemblies.Count == 0) {
                Debug.LogWarning("No incompatible assemblies found!");
                return;
            }
            
            string[] assemblyIdentifiers = new string[incompatibleAssemblies.Count];
            IncompatibleAssemblyNames = new string[incompatibleAssemblies.Count];
            IncompatibleAssemblyNamesTrimmed = new string[incompatibleAssemblies.Count];
            
            StringBuilder logMessage = new StringBuilder("Incompatible assemblies found: ");
            for (int index = 0; index < incompatibleAssemblies.Count; index++) {
                Assembly assembly = incompatibleAssemblies[index];
                AssemblyName name = assembly.GetName();
                string fullName = name.FullName;
                
                IncompatibleAssemblyNames[index] = fullName;
                IncompatibleAssemblyNamesTrimmed[index] = name.Name;
                
                string identifier = $"{fullName}@{name.Version}";
                assemblyIdentifiers[index] = identifier;

                logMessage.Append($"{identifier}, ");
            }

            logMessage.Append($"Total: {assemblyIdentifiers.Length}");
            Debug.LogWarning(logMessage.ToString());
            
            Array.Sort(IncompatibleAssemblyNames, StringComparer.Ordinal); // Sort the names to ensure consistent hashing
            
            string concatenatedIdentifiers = string.Join("", assemblyIdentifiers);
            MatchMakingKey = ComputeMatchMakingKeyHash(concatenatedIdentifiers);
        }

        private static string ComputeMatchMakingKeyHash(string input) {
            using (SHA256 sha256 = SHA256.Create()) {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                
                // https://stackoverflow.com/questions/16999361/obtain-sha-256-string-of-a-string
                StringBuilder builder = new StringBuilder();
                foreach (byte t in hashBytes) { builder.Append(t.ToString("x2")); }
                return builder.ToString();
            }
        }
    }
}