using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

public class EmbeddedAssembly
{
    private static Dictionary<string, Assembly> dic;

    public static byte[] ExtractRessource(string ZippedFile)
    {
        if (ZippedFile.EndsWith(".zip"))
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ZippedFile);
            using (System.IO.Compression.ZipArchive z = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read))
            {
                using (MemoryStream TempArrray = new MemoryStream())
                {
                    z.Entries[0].Open().CopyTo(TempArrray);
                    return TempArrray.ToArray();
                }
            }
        }
        else
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ZippedFile);
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string dllToLoad);

    public static void Load(string embeddedResource, string fileName)
    {
        if (dic == null)
        {
            dic = new Dictionary<string, Assembly>();
        }
        byte[] array = null;
        Assembly assembly = null;
        using (Stream stream = new MemoryStream(ExtractRessource(embeddedResource)))
        {
            if (stream == null)
            {
                throw new Exception(embeddedResource + " is not found in Embedded Resources.");
            }
            array = new byte[(int)stream.Length];
            stream.Read(array, 0, (int)stream.Length);
            try
            {
                if (fileName == "bnscompression.dll")
                {
                    File.WriteAllBytes(Path.GetTempPath() + "\\bnscompression.dll", array);
                    LoadLibrary(Path.GetTempPath() + "\\bnscompression.dll");
                }
                else
                {
                    assembly = Assembly.Load(array);
                    dic.Add(assembly.FullName, assembly);
                    return;
                }
            }
            catch
            {
            }
        }
        bool flag = false;
        string path = "";
        using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
        {
            string a = BitConverter.ToString(sHA1CryptoServiceProvider.ComputeHash(array)).Replace("-", string.Empty);
            path = Path.GetTempPath() + fileName;
            if (File.Exists(path))
            {
                byte[] buffer = File.ReadAllBytes(path);
                string b = BitConverter.ToString(sHA1CryptoServiceProvider.ComputeHash(buffer)).Replace("-", string.Empty);
                flag = ((a == b) ? true : false);
            }
            else
            {
                flag = false;
            }
        }
        if (!flag)
        {
            File.WriteAllBytes(path, array);
        }
        if (!path.Contains("bnscompression.dll"))
        {
            assembly = Assembly.LoadFile(path);
            dic.Add(assembly.FullName, assembly);
        }
    }

    public static Assembly Get(string assemblyFullName)
    {
        if (dic == null || dic.Count == 0)
        {
            return null;
        }
        if (dic.ContainsKey(assemblyFullName))
        {
            return dic[assemblyFullName];
        }
        return null;
    }
}
