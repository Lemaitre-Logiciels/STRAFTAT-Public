using System.Collections.Generic;
using System.Reflection;

namespace ComputerysModdingUtilities {
    public static partial class AssemblyScanner {
        private static readonly Dictionary<Assembly, List<Assembly>> ForeignAssemblyDependencyTree = new();

        private static void BuildTree(List<Assembly> assemblyScope) {
            foreach (Assembly assembly in assemblyScope) { BuildTreeForAssembly(assembly, assemblyScope); }
        }
        
        private static void BuildTreeForAssembly(Assembly assembly, List<Assembly> assemblyScope) {
            AssemblyName assemblyName = assembly.GetName();
            string assemblyNameString = assemblyName.FullName;
            if (ForeignAssemblyDependencyTree.ContainsKey(assembly)) { return; }
        
            AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
            List<Assembly> list = new List<Assembly> { };
            ForeignAssemblyDependencyTree[assembly] = list;
        
            foreach (AssemblyName referencedAssembly in referencedAssemblies) {
                string referencedAssemblyName = referencedAssembly.FullName;
                if (!AssemblyNameToAssembly.TryGetValue(referencedAssemblyName, out Assembly loadedAssembly)) { continue; }
                if (!assemblyScope.Contains(loadedAssembly)) { continue; }
                list.Add(loadedAssembly);
                BuildTreeForAssembly(loadedAssembly, assemblyScope);
            }
        }
    
        private static void GetAllDependencies(Assembly assemblyName, List<Assembly> dependencies) {
            if (dependencies.Contains(assemblyName) || !ForeignAssemblyDependencyTree.TryGetValue(assemblyName, out List<Assembly> assemblyDependencies)) { return; }
        
            dependencies.Add(assemblyName);
        
            foreach (Assembly dependency in assemblyDependencies) {
                if (dependencies.Contains(dependency)) { continue; }
                GetAllDependencies(dependency, dependencies);
            }
        } 
    }
}