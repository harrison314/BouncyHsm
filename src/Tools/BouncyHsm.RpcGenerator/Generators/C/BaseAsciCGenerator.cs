using BouncyHsm.RpcGenerator.Schema;
using System.Text;

namespace BouncyHsm.RpcGenerator.Generators.C;

internal abstract class BaseAsciCGenerator : IRpcGenerator
{
    private readonly StringBuilder header;
    private readonly StringBuilder body;
    public string Name
    {
        get;
    }

    public BaseAsciCGenerator(string name)
    {
        this.Name = name;
        this.header = new StringBuilder();
        this.body = new StringBuilder();
    }

    public void Init(RpcDefinition definition)
    {
        this.CreateHeader(definition);
        this.CreateBody(definition);
    }


    public void WriteToFolder(string path)
    {
        File.WriteAllText(Path.Combine(path, $"{this.Name}.h"), this.header.ToString());
        File.WriteAllText(Path.Combine(path, $"{this.Name}.c"), this.body.ToString());
    }

    protected virtual void ReleaseGenerator(StringBuilder sb, string name, MessageDefinition md)
    {

        sb.AppendFormat("int {0}_Release({0}* value)", name).AppendLine();
        sb.AppendLine("{");
        sb.AppendLine("     if (value == NULL) return NMRPC_BAD_ARGUMENT;");
        sb.AppendLine();
        foreach ((string fieldNameRaw, string fieldType) in md.Fields)
        {

            CDeclaredType declaredType = new CDeclaredType(fieldType);
            //string fieldName = this.EscapeFieldName(fieldNameRaw);
            string fieldName = fieldNameRaw;

            if (declaredType.IsArray)
            {
                sb.AppendLine($"  if({this.GetArrayReleaseFnName(declaredType)}(&value->{fieldName}) != NMRPC_OK)");
                sb.AppendLine("   {");
                sb.AppendLine("       return NMRPC_FATAL_ERROR;");
                sb.AppendLine("   }");
                continue;
            }

            if (declaredType.BaseDefinition == DeclaredType.BinaryName)
            {
                if (declaredType.IsNullable)
                {
                    sb.AppendLine($"  if (value->{fieldName} != NULL)");
                    sb.AppendLine("  {");
                    sb.AppendLine($"      Binary_Release(value->{fieldName});");
                    sb.AppendLine($"      free((void*)value->{fieldName});");
                    sb.AppendLine($"      value->{fieldName} = NULL;");
                    sb.AppendLine("  }");
                }
                else
                {
                    sb.AppendLine($"  Binary_Release(&value->{fieldName});");
                }

                continue;
            }

            if (declaredType.IsNullable || declaredType.BaseDefinition == DeclaredType.StringName)
            {
                if (declaredType.IsBaseType)
                {
                    string fn = $$"""
                               if (value->{{fieldName}} != NULL)
                               {
                                   free((void*) value->{{fieldName}});
                                   value->{{fieldName}} = NULL;
                               }
                              """;
                    sb.Append(fn).AppendLine();
                }
                else
                {
                    if (declaredType.IsNullable)
                    {
                        string fn = $$"""
                               if (value->{{fieldName}} != NULL) 
                               {
                                   if ({{declaredType.BaseDefinition}}_Release(value->{{fieldName}}) != NMRPC_OK)
                                   {
                                      return NMRPC_FATAL_ERROR;
                                   }
                                   free((void*) value->{{fieldName}});
                                   value->{{fieldName}} = NULL;
                               }
                              """;
                        sb.Append(fn).AppendLine();
                    }
                    else
                    {

                        string fn = $$"""
                                   if ({{declaredType.BaseDefinition}}_Release(&value->{{fieldName}}) != NMRPC_OK)
                                   {
                                      return NMRPC_FATAL_ERROR;
                                   }
                              """;
                        sb.Append(fn).AppendLine();
                    }
                }
            }


        }

        sb.AppendLine("    return NMRPC_OK;");
        sb.AppendLine("}");
    }

    protected virtual void CreateBody(RpcDefinition definition)
    {
        this.body.AppendLine("// This file is generated.");
        this.body.AppendLine("#include<stdlib.h>");
        this.body.AppendLine($"#include \"{this.Name}.h\"");
        this.body.AppendLine();

        this.AppendPrebodyFunctions(this.body);

        // todo Binary copy to
        this.body.AppendLine("""
           
            """);

        this.body.AppendLine("""
            int Binary_Release(Binary* value)
            {
                if (value == NULL) return NMRPC_BAD_ARGUMENT;
                if (value->data != NULL)
                {
                  free((void*) value->data);
                  value->data = NULL;
                  value->size = 0;
                }

                return NMRPC_OK;
            }
            """);

        foreach (CDeclaredType arrayTypes in this.GetArrayDefinitions(definition))
        {
            this.ArraySerializeGenerator(this.body, arrayTypes);
            this.ArrayDeserializeGenerator(this.body, arrayTypes);
            this.ArrayReleaseGenerator(this.body, arrayTypes);
        }

        foreach (KeyValuePair<string, MessageDefinition> a in definition.Messages)
        {
            this.SerializeGenerator(this.body, a.Key, a.Value);
            this.DeserializeGenerator(this.body, a.Key, a.Value);
            this.ReleaseGenerator(this.body, a.Key, a.Value);
        }

        foreach (KeyValuePair<string, RpcMethodDefinition> rpc in definition.Rpc)
        {
            this.RpcGenerator(this.body, rpc.Key, rpc.Value);
        }
    }

    protected virtual void AppendPrebodyFunctions(StringBuilder body)
    {
        // NOP
    }

    protected abstract void ArraySerializeGenerator(StringBuilder body, CDeclaredType arrayTypes);
    protected abstract void ArrayDeserializeGenerator(StringBuilder body, CDeclaredType arrayTypes);

    protected abstract void SerializeGenerator(StringBuilder body, string key, MessageDefinition value);
    protected abstract void DeserializeGenerator(StringBuilder body, string key, MessageDefinition value);


    protected virtual void ArrayReleaseGenerator(StringBuilder sb, CDeclaredType type)
    {
        sb.AppendFormat("int {0}_Release({0}* value)", type.CType).AppendLine();
        sb.AppendLine("{");
        sb.AppendLine("     if (value == NULL) return NMRPC_BAD_ARGUMENT;");
        sb.AppendLine();

        if (type.GetTypeFromAray() == CDeclaredType.StringName)
        {
            sb.AppendLine("""
                  int i;
                  for (i = 0; i < value->length; i++)
                  {
                     if (value->array[i]) free((void*) value->array[i]);
                  }

                  free((void*) value->array);

                  value->length = 0;
                  value->array = NULL;
                """);
        }
        else if (type.IsBaseType)
        {
            sb.AppendLine("""
                  free((void*) value->array);

                  value->length = 0;
                  value->array = NULL;
                """);
        }
        else
        {
            sb.AppendLine($$"""
                  int i;
                  for (i = 0; i < value->length; i++)
                  {
                      if ({{type.GetTypeFromAray()}}_Release(&value->array[i]) != NMRPC_OK)
                     {
                        return NMRPC_FATAL_ERROR;
                     }
                  }

                  free((void*) value->array);

                  value->length = 0;
                  value->array = NULL;
                """);
        }

        sb.AppendLine("    return NMRPC_OK;");
        sb.AppendLine("}");
    }

    protected virtual void CreateHeader(RpcDefinition definition)
    {
        this.header.AppendLine("// This file is generated.");
        this.header.AppendLine($"#ifndef NMRPC_{this.Name}");
        this.header.AppendLine($"#define NMRPC_{this.Name}");

        this.header.AppendLine("#include <stddef.h>");
        this.header.AppendLine("#include <stdbool.h>");
        this.header.AppendLine("#include <stdint.h>");
        this.AddHeaderDefinitions(this.header);

        this.header.AppendLine();
        this.header.AppendLine($"#define NMRPC_OK 0");
        this.header.AppendLine($"#define NMRPC_BAD_ARGUMENT 1");
        this.header.AppendLine($"#define NMRPC_DESERIALIZE_ERR 2");
        this.header.AppendLine($"#define NMRPC_FATAL_ERROR 3");

        this.header.AppendLine();

        foreach (KeyValuePair<string, MessageDefinition> type in definition.Messages)
        {
            header.AppendFormat("typedef struct _{0} {0};", type.Key);
            header.AppendLine();
        }

        header.AppendLine();

        header.AppendLine("typedef struct _Binary Binary;");
        header.AppendLine();

        //header.AppendLine("#ifndef NMRPC_LOG_ERR_FIELD");
        //header.AppendLine("#define NMRPC_LOG_ERR_FIELD(field) log_err_field(__FILE__, __LINE__, __FUNCTION__, field)");
        //header.AppendLine("#endif");


        foreach (CDeclaredType type in this.GetArrayDefinitions(definition))
        {
            header.AppendFormat("typedef struct _{0} {0};", type.CType.TrimEnd('*'));
            header.AppendLine();
        }

        header.AppendLine();

        header.AppendLine("""
            typedef struct _Binary {
              uint8_t* data;
              size_t size;
            } Binary;
            """);
        header.AppendLine();
        header.AppendLine("int Binary_Release(Binary* value);");
        header.AppendLine();
        header.AppendLine("typedef int (*SerializeFnPtr_t)(cmp_ctx_t* ctx, void* data);");
        header.AppendLine("int nmrpc_writeAsBinary(void *data, SerializeFnPtr_t serialize, Binary** outBinary);");
        header.AppendLine();

        foreach (CDeclaredType type in this.GetArrayDefinitions(definition))
        {
            header.AppendLine($$"""
                typedef struct _{{type.CType.TrimEnd('*')}} 
                {
                    {{type.GetTypeFromAray()}}* array;
                    int length;
                } {{type.CType.TrimEnd('*')}};
                """);

            header.AppendFormat("{0};", this.GetArraySerializeFnDeclaration(type)).AppendLine();
            header.AppendFormat("{0};", this.GetArrayDeserializeFnDeclaration(type)).AppendLine();
            header.AppendFormat("{0};", this.GetArrayReleaseFnDeclaration(type)).AppendLine();

            header.AppendLine();
        }

        header.AppendLine();

        foreach (KeyValuePair<string, MessageDefinition> type in definition.Messages)
        {
            CDeclaredType ctype = new CDeclaredType(type.Key);
            header.AppendFormat("typedef struct _{0}", type.Key).AppendLine();
            header.AppendLine("{");

            foreach ((string name, string fieldType) in type.Value.Fields)
            {
                CDeclaredType declaredType = new CDeclaredType(fieldType);

                header.AppendFormat("    {0} {1};",
                    declaredType.CType,
                    name).AppendLine();
            }

            header.Append('}').AppendFormat(" {0};", type.Key).AppendLine();
            header.AppendLine();

            header.AppendFormat("{0};", this.GetSerializeFnDeclaration(ctype)).AppendLine();
            header.AppendFormat("{0};", this.GetDeserializeFnDeclaration(ctype)).AppendLine();
            header.AppendFormat("{0};", this.GetReleaseFnDeclaration(ctype)).AppendLine();

            header.AppendLine();
        }

        this.GenerateRpcHeaders(this.header, definition);

        header.AppendLine($"#endif // NMRPC_{this.Name}");
    }



    protected virtual void AddHeaderDefinitions(StringBuilder header)
    {
        //
    }

    protected abstract string GetSerializeFnName(CDeclaredType type);
    protected abstract string GetSerializeFnDeclaration(CDeclaredType type);

    protected abstract string GetDeserializeFnName(CDeclaredType type);
    protected abstract string GetDeserializeFnDeclaration(CDeclaredType type);

    protected virtual string GetReleaseFnName(CDeclaredType type)
    {
        return $"{type.BaseDefinition}_Release";
    }

    protected virtual string GetReleaseFnDeclaration(CDeclaredType type)
    {
        return $"int {type.BaseDefinition}_Release({type.BaseDefinition}* value)";
    }


    protected virtual string GetArrayReleaseFnDeclaration(CDeclaredType type)
    {
        return $"int {type.CType.TrimEnd('*')}_Release({type.CType.TrimEnd('*')}* value)";
    }

    protected virtual string GetArrayReleaseFnName(CDeclaredType type)
    {
        return $"{type.CType.TrimEnd('*')}_Release";
    }

    protected abstract string GetArrayDeserializeFnDeclaration(CDeclaredType type);

    protected abstract string GetArraySerializeFnDeclaration(CDeclaredType type);


    protected abstract void RpcGenerator(StringBuilder body, string rpcName, RpcMethodDefinition rpcDef);

    private void GenerateRpcHeaders(StringBuilder header, RpcDefinition definition)
    {
        header.AppendLine("""

            typedef void* (*nmrpc_malloc_fn_t)(size_t size);
            typedef void (*nmrpc_free_fn_t)(void* ptr);
            typedef void* (*nmrpc_realloc_fn_t)(void* ptr, size_t new_size);
            
            typedef int (*nmrpc_writerequest_fn_t)(void* user_ctx, void* request_data, size_t request_data_size);
            typedef int (*nmrpc_flush_fn_t)(void* user_ctx);
            typedef size_t (*nmrpc_readresponse_fn_t)(void* user_ctx, void* response_data, size_t response_data_size);
            typedef int (*nmrpc_readclose_fn_t)(void* user_ctx);

            typedef struct _nmrpc_global_context {
                void* user_ctx;

                nmrpc_writerequest_fn_t write;
                nmrpc_readresponse_fn_t read;
                nmrpc_flush_fn_t flush;
                nmrpc_readclose_fn_t close;

                char *tag;
            } nmrpc_global_context_t;


            int nmrpc_global_context_init(nmrpc_global_context_t* ctx, void* user_ctx, nmrpc_writerequest_fn_t write, nmrpc_readresponse_fn_t read, nmrpc_readclose_fn_t close, nmrpc_flush_fn_t flush);

            """);


        foreach (KeyValuePair<string, RpcMethodDefinition> rpc in definition.Rpc)
        {
            string callName = rpc.Key;
            string requestType = rpc.Value.Request;
            string responseType = rpc.Value.Response;

            header.AppendLine($"int nmrpc_call_{callName}(nmrpc_global_context_t* ctx, {requestType}* request, {responseType}* response);");
        }

        header.AppendLine();
    }

    protected HashSet<CDeclaredType> GetArrayDefinitions(RpcDefinition definition)
    {
        IEnumerable<CDeclaredType> arrays = definition.Messages.SelectMany(t => t.Value.Fields)
             .Select(t => new CDeclaredType(t.Value))
             .Where(t => t.IsArray);

        return new HashSet<CDeclaredType>(arrays);
    }
}
