﻿using System.Reflection;

namespace API.Utilities
{
    public class AssemblyHelper
    {
        public static IEnumerable<string> GetQADAssemblyAllTypes()
        {
            var sapLibName = "QAD_WSDL_Library";
            var assembly = Assembly.Load(sapLibName);
            var types = assembly.GetTypes().Select(t => t.FullName ?? "");
            return types;
        }
        public static IEnumerable<string> GetAssemblyAllTypes()
        {
            var list = GetQADAssemblyAllTypes().ToList();
            return list;
        }
    }
}
