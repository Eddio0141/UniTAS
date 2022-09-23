using Mono.Cecil;
using System.Collections.Generic;

namespace UniTASPatcher
{
    public static class Patcher
    {
        static bool patchedAsyncOperation = false;

        public static IEnumerable<string> TargetDLLs { get; } = new string[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll" };

        public static void Patch(AssemblyDefinition assembly)
        {
            switch (assembly.Name.Name)
            {
                case "UnityEngine.CoreModule":
                case "UnityEngine":
                    {
                        PatchAsyncOperation(assembly);
                    }
                    break;
                default:
                    break;
            }
        }

        static void PatchAsyncOperation(AssemblyDefinition assembly)
        {
            if (patchedAsyncOperation)
                return;
            var type = assembly.MainModule.GetType("UnityEngine", "AsyncOperation");
            if (type == null)
                return;
            // add long UID field
            var field = new FieldDefinition("__UniTAS_UID", FieldAttributes.Private, assembly.MainModule.ImportReference(typeof(ulong)));
            type.Fields.Add(field);
            patchedAsyncOperation = true;
        }
    }
}