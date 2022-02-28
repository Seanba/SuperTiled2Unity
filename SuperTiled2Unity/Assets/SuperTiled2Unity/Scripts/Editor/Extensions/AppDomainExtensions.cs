using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SuperTiled2Unity.Editor
{
    public static class AppDomainExtensions
    {
        public static ICollection<Type> GetMatchingTypesInAssembly(this Assembly assembly, Predicate<Type> predicate)
        {
            ICollection<Type> types = new List<Type>();
            try
            {
                types = assembly.GetTypes().Where(i => i != null && predicate(i) && i.Assembly == assembly).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Type theType in ex.Types)
                {
                    try
                    {
                        if (theType != null && predicate(theType) && theType.Assembly == assembly)
                        {
                            types.Add(theType);
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Ignore
                    }
                }
            }
            return types;
        }

        public static IEnumerable<Type> GetAllDerivedTypes<T>(this AppDomain appDomain)
        {
            var baseType = typeof(T);
            var derivedTypes = new List<Type>();

            //  Fix adapted from: https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes/12906892
            foreach (var assembly in appDomain.GetAssemblies())
            {
                if (assembly != null)
                {
                    derivedTypes.AddRange(assembly.GetMatchingTypesInAssembly(t => t.IsSubclassOf(baseType)));
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
