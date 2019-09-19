using System;
using System.Collections.Generic;

namespace SuperTiled2Unity.Editor
{
    public static class AppDomainExtensions
    {
        public static IEnumerable<Type> GetAllDerivedTypes<T>(this AppDomain appDomain)
        {
            var baseType = typeof(T);
            var derivedTypes = new List<Type>();

            foreach (var assembly in appDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseType))
                    {
                        derivedTypes.Add(type);
                    }
                }
            }

            return derivedTypes;
        }

        public static Type GetTypeFromName(this AppDomain appDomain, string className)
        {
            foreach (var assembly in appDomain.GetAssemblies())
            {
                var type = assembly.GetType(className);

                if (type != null)
                {
                    return type;
                }
            }

            // Didn't find the type in any assemblies
            return null;
        }
    }
}
