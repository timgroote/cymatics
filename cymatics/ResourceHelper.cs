using System;
using System.IO;
using System.Reflection;

namespace cymatics
{
    static class ResourceHelper
    {
        public static string LoadTextFromRecource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            //Console.Out.WriteLine(String.Join(Environment.NewLine,assembly.GetManifestResourceNames()));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
