using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class AssemblyHelper
    {
        public static string GetExecutingAssemblyPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        [NotNull]
        public static string ReadEmbeddedResource([NotNull] this Assembly assembly, [NotNull] string resourceName)
        {
            if (null == assembly)
                throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentNullException(nameof(resourceName));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (null == stream)
                    throw new Exception($"The resource '{resourceName}' must exist in assembly '{assembly.FullName}'.");

                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(result))
                        throw new Exception($"The resource '{resourceName}' has an empty value in assembly '{assembly.FullName}'.");
                    return result;
                }
            }
        }
    }
}