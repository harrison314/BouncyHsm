using BouncyHsm.RpcGenerator.Generators.C;
using BouncyHsm.RpcGenerator.Generators.CSharp;
using BouncyHsm.RpcGenerator.Schema;

namespace BouncyHsm.RpcGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine();

        Console.WriteLine("Generating RPC");

        string path = GetDefinitionFile();

        Console.WriteLine("Load {0}", path);
        RpcDefinition definition = RpcDefinition.Load(path);


        CmpAsciCGenerator cGenerator = new CmpAsciCGenerator("rpc");
        cGenerator.Init(definition);
        cGenerator.WriteToFolder(CombineWithDir(path, "Src/BouncyHsm.Pkcs11Lib/rpc"));

        Console.WriteLine("Generated C files.");

        CCharpMpGenerator cCharpMpGenerator = new CCharpMpGenerator("MessagepackRpc");
        cCharpMpGenerator.Init(definition);
        cCharpMpGenerator.WriteToFolder(CombineWithDir(path, "Src/BouncyHsm.Core/Rpc/Generated"));

        Console.WriteLine("Generated C# files.");
    }

    private static string CombineWithDir(string filePath, string anotherPath)
    {
        return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath)!, anotherPath);
    }
    private static string GetDefinitionFile()
    {
        string directory = System.IO.Directory.GetCurrentDirectory();
        while(true) 
        {
            string path = System.IO.Path.Combine(directory, "RpcDefintion.yaml");

            if(File.Exists(path))
            {
                return path;
            }

            directory = System.IO.Directory.GetParent(directory)!.FullName;
        }
    }
}
