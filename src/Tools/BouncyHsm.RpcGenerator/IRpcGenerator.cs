using BouncyHsm.RpcGenerator.Schema;

namespace BouncyHsm.RpcGenerator;

public interface IRpcGenerator
{
    void Init(RpcDefinition definition);

    void WriteToFolder(string path);
}