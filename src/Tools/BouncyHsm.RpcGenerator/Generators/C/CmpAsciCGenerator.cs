using BouncyHsm.RpcGenerator.Schema;
using System.Text;

namespace BouncyHsm.RpcGenerator.Generators.C;

internal class CmpAsciCGenerator : BaseAsciCGenerator
{
    public CmpAsciCGenerator(string name) : base(name)
    {
    }


    protected override string GetDeserializeFnDeclaration(CDeclaredType type)
    {
        return $"int {type.BaseDefinition}_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, {type.BaseDefinition}* value)";
    }

    protected override string GetDeserializeFnName(CDeclaredType type)
    {
        return $"{type.BaseDefinition}_Deserialize";
    }

    protected override string GetSerializeFnDeclaration(CDeclaredType type)
    {
        return $"int {type.BaseDefinition}_Serialize(cmp_ctx_t* ctx, {type.BaseDefinition}* value)";
    }

    protected override string GetSerializeFnName(CDeclaredType type)
    {
        return $"{type.BaseDefinition}_Serialize";
    }

    protected override void AppendPrebodyFunctions(StringBuilder body)
    {
        body.AppendLine("""
            #include <stdio.h>
            #include <string.h>
            #include <stdlib.h>
            #include <stdbool.h>
            #include <stddef.h>
            #include <stdint.h>
            #include "../logger.h"

            #define USE_VARIABLE(x) (void)(x)
            #define NMRPC_LOG_ERR_FIELD(field) log_message(LOG_LEVEL_ERROR, "Error in function %s (line %i) with filed: %s", __FUNCTION__, __LINE__, field)
            #define NMRPC_LOG_ERR_TEXT(msg) log_message(LOG_LEVEL_ERROR, "Error in function %s (line %i) %s", __FUNCTION__, __LINE__, msg)
            #define NMRPC_LOG_FAILED_CLOSE_SOCKET() log_message(LOG_LEVEL_INFO, "Closing socket failed in function %s (line %i)",__FUNCTION__, __LINE__)

            static int cmph_read_nullable_str(cmp_ctx_t* ctx, char** ptr)
            {
              cmp_object_t obj;
              if (!cmp_read_object(ctx, &obj))
              {
                 log_message(LOG_LEVEL_ERROR, "Error in %s. Can not read messagepack object.", __FUNCTION__);
                 return NMRPC_DESERIALIZE_ERR;
              }

              if (cmp_object_is_nil(&obj))
              {
                  *ptr = NULL;
                  return NMRPC_OK;
              }

              if (!cmp_object_is_str(&obj))
              {
                NMRPC_LOG_ERR_TEXT("value is not null or string");
                return NMRPC_DESERIALIZE_ERR;
              }
              
              uint32_t size = 0;
              if (!cmp_object_as_str(&obj, &size))
              {
                 NMRPC_LOG_ERR_TEXT("Require str.");
                 return NMRPC_FATAL_ERROR;
              }

              char * buff = (char*) malloc(sizeof(char) * (size + 1));
              if (buff == NULL) return NMRPC_FATAL_ERROR;
              if (!ctx->read(ctx, (void*) buff, size))
              {
                  NMRPC_LOG_ERR_TEXT("Error during reading.");
                  return NMRPC_DESERIALIZE_ERR;
              }

              buff[size] = 0;
              *ptr = buff;

              return NMRPC_OK;
            }

            static int cmph_read_binary(cmp_ctx_t* ctx, Binary* value)
            {
              cmp_object_t obj;
              if (!cmp_read_object(ctx, &obj))
              {
                 NMRPC_LOG_ERR_TEXT("Require binary token.");
                 return NMRPC_DESERIALIZE_ERR;
              }

              if (!cmp_object_is_bin(&obj))
              {
                  NMRPC_LOG_ERR_TEXT("Require binary token.");
                  return NMRPC_DESERIALIZE_ERR;
              }

              uint32_t size = 0;
              cmp_object_as_bin(&obj, &size);

              uint8_t* buff = (uint8_t*) malloc(size);
              if (buff == NULL) return NMRPC_FATAL_ERROR;
              if (!ctx->read(ctx, (void*) buff, size))
              {
                  NMRPC_LOG_ERR_TEXT("Error during deserializing.");
                  return NMRPC_DESERIALIZE_ERR;
              }

              value->data = buff;
              value->size = size;

              return NMRPC_OK;
            }

            static int cmph_read_nullable_binary(cmp_ctx_t* ctx, Binary** ptr)
            {
              cmp_object_t obj;
              if (!cmp_read_object(ctx, &obj))
              {
                 NMRPC_LOG_ERR_TEXT("Error during reading.");
                 return NMRPC_DESERIALIZE_ERR;
              }
            
              if (cmp_object_is_nil(&obj))
              {
                  *ptr = NULL;
                  return NMRPC_OK;
              }

              if (!cmp_object_is_bin(&obj))
              {
                  NMRPC_LOG_ERR_TEXT("Require binary token.");
                  return NMRPC_DESERIALIZE_ERR;
              }
            
              uint32_t size = 0;
              cmp_object_as_bin(&obj, &size);
            
              uint8_t* buff = (uint8_t*) malloc(size);
              if (buff == NULL) return NMRPC_FATAL_ERROR;
              if (!ctx->read(ctx, (void*) buff, size))
              {
                  NMRPC_LOG_ERR_TEXT("Error during deserilizing.");
                  return NMRPC_DESERIALIZE_ERR;
              }
            
              Binary* binary = (Binary*) malloc(sizeof(Binary));
              if (binary == NULL) return NMRPC_FATAL_ERROR;
              
              binary->data = buff;
              binary->size = size;
              
              *ptr = binary;
            
              return NMRPC_OK;
            }

            static int nmrpc_flush_empty(void* user_ctx)
            {
                return NMRPC_OK;
            }

            static int nmrpc_close_empty(void* user_ctx)
            {
                return NMRPC_OK;
            }

            int nmrpc_global_context_init(nmrpc_global_context_t* ctx, void* user_ctx, nmrpc_writerequest_fn_t write, nmrpc_readresponse_fn_t read, nmrpc_readclose_fn_t close, nmrpc_flush_fn_t flush)
            {
                if (ctx == NULL) return NMRPC_BAD_ARGUMENT;
                if (write == NULL) return NMRPC_BAD_ARGUMENT;
                if (read == NULL) return NMRPC_BAD_ARGUMENT;

                ctx->user_ctx = user_ctx;
                ctx->write = write;
                ctx->read = read;
                ctx->flush = (flush != NULL)? flush : &nmrpc_flush_empty;
                ctx->close = (close != NULL)? close : &nmrpc_close_empty;

                ctx->mallocPtr = &malloc;
                ctx->freePtr = &free;
                ctx->reallocPtr = &realloc;

                return NMRPC_OK;
            }

            static bool mnrpc_empty_file_reader(cmp_ctx_t *ctx, void *data, size_t limit)
            {
                USE_VARIABLE(ctx);    
                USE_VARIABLE(data);
                USE_VARIABLE(limit);
            
                return false;
            }

            static bool mnrpc_empty_file_skipper(cmp_ctx_t *ctx, size_t count)
            {
                USE_VARIABLE(ctx);    
                USE_VARIABLE(count);

                return false;
            }

            static size_t mnrpc_empty_file_writer(cmp_ctx_t *ctx, const void *data, size_t count)
            {
                USE_VARIABLE(ctx);    
                USE_VARIABLE(data);   
                USE_VARIABLE(count);
            
                return 0;
            }

            typedef struct _InternalBuffer {
                uint8_t* buffer;
                size_t size;
                size_t capacity;
            } InternalBuffer_t;

            static int InternalBuffer_init(InternalBuffer_t* buffer, size_t capacity)
            {
               buffer->size = 0;
               buffer->capacity = capacity;
               buffer->buffer = (uint8_t*) malloc(capacity);

               if (buffer->buffer == NULL)
               {
                  return NMRPC_FATAL_ERROR;
               }

               return NMRPC_OK;
            }

            static void InternalBuffer_free(InternalBuffer_t* buffer)
            {
               if (buffer->buffer != NULL)
               {
                   free((void*)buffer->buffer);
                   buffer->buffer = NULL;
                   buffer->size = 0;
                   buffer->capacity = 0;
               }
            }

            static int InternalBuffer_append(InternalBuffer_t* buffer, const void* data, size_t size)
            {
               if (buffer == NULL || data == NULL) return NMRPC_BAD_ARGUMENT;

               if (size == 0) return NMRPC_OK;

               if (buffer->capacity <= buffer->size + size)
               {
                   size_t new_capacity = buffer->capacity * 2;
                   while (new_capacity < buffer->size + size)
                   {
                   	   new_capacity *= 2;
                   }

                   void* new_buffer = realloc(buffer->buffer, new_capacity);
                   if (new_buffer == NULL)
                   {
                      return NMRPC_FATAL_ERROR;
                   }

                   buffer->buffer = (uint8_t*) new_buffer;
                   buffer->capacity = new_capacity;
               }

               memcpy(buffer->buffer + buffer->size, data, size);
               buffer->size += size;

               return NMRPC_OK;
            }

            static size_t mnrpc_buffer_file_writer(cmp_ctx_t *ctx, const void *data, size_t count)
            {
                InternalBuffer_t* buffer = (InternalBuffer_t*)ctx->buf;
                if (InternalBuffer_append(buffer, data, count) != NMRPC_OK)
                {
                    return 0;
                }

                return count;
            }

            typedef struct _InternalBufferReader {
                void* buffer;
                size_t size;
                size_t position;
            } InternalBufferReader_t;

            static bool mnrpc_bufferReader_file_reader(cmp_ctx_t *ctx, void *data, size_t limit)
            {
                InternalBufferReader_t* bufferReader = (InternalBufferReader_t*)ctx->buf;
                
                if (bufferReader->position + limit > bufferReader->size)
                {
                    return false;
                }

                memcpy(data, ((uint8_t*) bufferReader->buffer) + bufferReader->position, limit);
                bufferReader->position += limit;

                return true;
            }
            
            static bool mnrpc_bufferReader_file_skipper(cmp_ctx_t *ctx, size_t count)
            {
                InternalBufferReader_t* bufferReader = (InternalBufferReader_t*)ctx->buf;
                if (bufferReader->position + count > bufferReader->size)
                {
                    return false;
                }
            
                bufferReader->position += count;
            
                return true;
            }


            int nmrpc_writeAsBinary(void *data, SerializeFnPtr_t serialize, Binary** outBinary)
            {
                if (data == NULL || serialize == NULL || outBinary == NULL) return NMRPC_BAD_ARGUMENT;

                int result = NMRPC_OK;

                cmp_ctx_t ctx = { 0 };
                InternalBuffer_t buffer = { 0 };

                result = InternalBuffer_init(&buffer, 256);
                if (result != NMRPC_OK)
                {
                    return result;
                }

                cmp_init(&ctx, &buffer, mnrpc_empty_file_reader, mnrpc_empty_file_skipper, mnrpc_buffer_file_writer);

                result = serialize(&ctx, data);
                if (result != NMRPC_OK)
                {
                    return result;
                }

                Binary* rData = (Binary*) malloc(sizeof(Binary));
                if (rData == NULL)
                {
                   return NMRPC_FATAL_ERROR;
                }

                rData->data = buffer.buffer;
                rData->size = buffer.size;

                *outBinary = rData;

                return NMRPC_OK;
            }
            """);
        body.AppendLine();
    }
    protected override void SerializeGenerator(StringBuilder body, string mainTypeName, MessageDefinition value)
    {
        body.AppendLine($"int {mainTypeName}_Serialize(cmp_ctx_t* ctx, {mainTypeName}* value)");
        body.AppendLine("{");
        body.AppendLine("  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;");

        body.AppendLine("  int result = 0;");
        body.AppendLine();

        body.AppendLine($"    result = cmp_write_array(ctx, {value.Fields.Count});");
        body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
        body.AppendLine();

        foreach (KeyValuePair<string, string> fieldDef in value.Fields)
        {
            CDeclaredType type = new CDeclaredType(fieldDef.Value);
            string name = fieldDef.Key;

            if (type.IsArray)
            {

                body.AppendLine($"  result = {type.CType}_Serialize(ctx, &value->{name});");
                body.AppendLine("   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;");
                body.AppendLine();

                continue;
            }

            if (type.IsBaseType)
            {
                if (type.BaseDefinition == CDeclaredType.StringName)
                {
                    if (type.IsNullable)
                    {
                        body.AppendLine($"  result = (value->{name} != NULL)? cmp_write_str(ctx, value->{name}, (uint32_t)strlen(value->{name})) : cmp_write_nil(ctx);");
                        body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                        body.AppendLine();
                    }
                    else
                    {
                        body.AppendLine($"  result = cmp_write_str(ctx, value->{name}, (uint32_t)strlen(value->{name}));");
                        body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                        body.AppendLine();
                    }
                }

                if (type.BaseDefinition == CDeclaredType.BinaryName)
                {
                    if (type.IsNullable)
                    {
                        body.AppendLine($"  result = (value->{name} != NULL)? cmp_write_bin(ctx, value->{name}->data, (uint32_t)value->{name}->size) : cmp_write_nil(ctx);");
                        body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                        body.AppendLine();
                    }
                    else
                    {
                        body.AppendLine($"  result = cmp_write_bin(ctx, value->{name}.data, (uint32_t)value->{name}.size);");
                        body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                        body.AppendLine();
                    }
                }

                if (type.BaseDefinition is CDeclaredType.Int32Name or CDeclaredType.Int64Name)
                {
                    body.AppendLine($"  result = cmp_write_integer(ctx, value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                if (type.BaseDefinition is CDeclaredType.UInt32Name or CDeclaredType.UInt64Name)
                {
                    body.AppendLine($"  result = cmp_write_uinteger(ctx, value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                if (type.BaseDefinition == CDeclaredType.BoolName)
                {
                    body.AppendLine($"  result = cmp_write_bool(ctx, value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                continue;
            }


            string writeFnName = this.GetSerializeFnName(type);
            if (type.IsNullable)
            {
                body.AppendLine($"  result = (value->{name} != NULL)? {writeFnName}(ctx, value->{name}) : cmp_write_nil(ctx);");
                body.AppendLine("   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }
            else
            {
                body.AppendLine($"  result = {writeFnName}(ctx, &value->{name});");
                body.AppendLine("   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }
        }


        body.AppendLine("    return NMRPC_OK;");
        body.AppendLine("}");
        body.AppendLine();

    }

    protected override void DeserializeGenerator(StringBuilder body, string mainTypeName, MessageDefinition value)
    {
        body.AppendLine($"int {mainTypeName}_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, {mainTypeName}* value)");
        body.AppendLine("{");
        body.AppendLine("  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;");
        body.AppendLine("  int result = 0;");
        body.AppendLine("  cmp_object_t start_obj;");
        body.AppendLine("  cmp_object_t tmp_obj;");
        body.AppendLine("  uint32_t array_size;");
        body.AppendLine();

        body.AppendLine("   USE_VARIABLE(tmp_obj);");

        body.AppendLine("  if (start_obj_ptr == NULL)");
        body.AppendLine("  {");
        body.AppendLine("    result = cmp_read_object(ctx, &start_obj);");
        body.AppendLine("    if (!result){ NMRPC_LOG_ERR_TEXT(\"Can not read token.\"); return NMRPC_DESERIALIZE_ERR; }");
        body.AppendLine("    start_obj_ptr = &start_obj;");
        body.AppendLine("  }");
        body.AppendLine();
        body.AppendLine("  result = cmp_object_as_array(start_obj_ptr, &array_size);");
        body.AppendLine($"  if (!result || array_size != {value.Fields.Count}) {{ NMRPC_LOG_ERR_TEXT(\"Incorect field count.\"); return NMRPC_DESERIALIZE_ERR; }}");
        body.AppendLine();


        foreach (KeyValuePair<string, string> fieldDef in value.Fields)
        {
            CDeclaredType type = new CDeclaredType(fieldDef.Value);
            string name = fieldDef.Key;

            if (type.IsArray)
            {
                body.AppendLine($"  result = {type.CType.TrimEnd('*')}_Deserialize(ctx, NULL, &value->{name});");
                body.AppendLine("   if (result != NMRPC_OK) return result;");
                body.AppendLine();

                continue;
            }

            if (type.IsBaseType)
            {
                if (type.BaseDefinition == CDeclaredType.StringName)
                {
                    if (type.IsNullable)
                    {
                        body.AppendLine($"  result = cmph_read_nullable_str(ctx, &value->{name});");
                        body.AppendLine("   if (result != NMRPC_OK) return result;");
                        body.AppendLine();
                    }
                    else
                    {
                        body.AppendLine($"  result = cmph_read_nullable_str(ctx, &value->{name});");
                        body.AppendLine("   if (result != NMRPC_OK) return result;");
                        body.AppendLine($"   if (value->{name} == NULL) return NMRPC_DESERIALIZE_ERR;");
                        body.AppendLine();
                    }
                }

                if (type.BaseDefinition == CDeclaredType.BinaryName)
                {
                    if (type.IsNullable)
                    {
                        body.AppendLine($"  result = cmph_read_nullable_binary(ctx, &value->{name});");
                        body.AppendLine("   if (result != NMRPC_OK) return result;");
                        body.AppendLine();
                    }
                    else
                    {
                        body.AppendLine($"  result = cmph_read_binary(ctx, &value->{name});");
                        body.AppendLine("   if (result != NMRPC_OK) return result;");
                        body.AppendLine();
                    }
                }

                if (type.BaseDefinition == CDeclaredType.Int32Name)
                {
                    body.AppendLine($"  result = cmp_read_int(ctx, &value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }
                if (type.BaseDefinition == CDeclaredType.Int64Name)
                {
                    body.AppendLine($"  result = cmp_read_long(ctx, &value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                if (type.BaseDefinition == CDeclaredType.UInt32Name)
                {
                    body.AppendLine($"  result = cmp_read_uint(ctx, &value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                if (type.BaseDefinition == CDeclaredType.UInt64Name)
                {
                    body.AppendLine($"  result = cmp_read_ulong(ctx, &value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                if (type.BaseDefinition == CDeclaredType.BoolName)
                {
                    body.AppendLine($"  result = cmp_read_bool(ctx, &value->{name});");
                    body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                    body.AppendLine();
                }

                continue;
            }


            string readFnName = this.GetDeserializeFnName(type);
            if (type.IsNullable)
            {
                body.AppendLine("  cmp_read_object(ctx, &tmp_obj);");
                body.AppendLine("  if (cmp_object_is_nil(&tmp_obj))");
                body.AppendLine("  {");
                body.AppendLine($"      value->{name} = NULL;");
                body.AppendLine("  }");
                body.AppendLine("  else");
                body.AppendLine("  {");
                body.AppendLine($"     value->{name} = ({type.BaseDefinition}*) malloc(sizeof({type.BaseDefinition}));");
                body.AppendLine($"     if (value->{name} == NULL) return NMRPC_FATAL_ERROR;");
                body.AppendLine($"     result = {readFnName}(ctx, &tmp_obj, value->{name});");
                body.AppendLine("      if (result != NMRPC_OK) return result;");
                body.AppendLine("  }");
                body.AppendLine();
            }
            else
            {
                body.AppendLine($"  result = {readFnName}(ctx, NULL, &value->{name});");
                body.AppendLine("   if (result != NMRPC_OK) return result;");
                body.AppendLine();
            }
        }


        body.AppendLine("    return NMRPC_OK;");
        body.AppendLine("}");
        body.AppendLine();
    }

    protected override string GetArraySerializeFnDeclaration(CDeclaredType type)
    {
        return $"int {type.CType.TrimEnd('*')}_Serialize(cmp_ctx_t* ctx, {type.CType.TrimEnd('*')}* value)";
    }

    protected override string GetArrayDeserializeFnDeclaration(CDeclaredType type)
    {
        return $"int {type.CType.TrimEnd('*')}_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, {type.CType.TrimEnd('*')}* value)";
    }

    protected override void ArraySerializeGenerator(StringBuilder body, CDeclaredType type)
    {
        body.AppendLine($"int {type.CType}_Serialize(cmp_ctx_t* ctx, {type.CType}* value)");
        body.AppendLine("{");
        body.AppendLine("  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;");

        body.AppendLine("  int result = 0;");
        body.AppendLine("  int i = 0;");
        body.AppendLine();

        body.AppendLine($"    result = cmp_write_array(ctx, value->length);");
        body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
        body.AppendLine();

        body.AppendLine("  for (i = 0; i < value->length; i++)");
        body.AppendLine("  {");

        if (type.IsBaseType)
        {
            if (type.BaseDefinition == CDeclaredType.StringName)
            {

                body.AppendLine($"  result = cmp_write_str(ctx, value->array[i], (uint32_t)strlen(value->array[i]));");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.BinaryName)
            {

                body.AppendLine($"  result = cmp_write_bin(ctx, value->array[i].data, (uint32_t)value->array[i].size);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition is CDeclaredType.Int32Name or CDeclaredType.Int64Name)
            {
                body.AppendLine($"  result = cmp_write_integer(ctx, value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition is CDeclaredType.UInt32Name or CDeclaredType.UInt64Name)
            {
                body.AppendLine($"  result = cmp_write_uinteger(ctx, value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.BoolName)
            {
                body.AppendLine($"  result = cmp_write_bool(ctx, value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }
        }
        else
        {

            string writeFnName = this.GetSerializeFnName(type);

            body.AppendLine($"  result = {writeFnName}(ctx, &value->array[i]);");
            body.AppendLine("   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;");
            body.AppendLine();

        }

        body.AppendLine("  }");
        body.AppendLine();

        body.AppendLine("    return NMRPC_OK;");
        body.AppendLine("}");
        body.AppendLine();

    }

    protected override void ArrayDeserializeGenerator(StringBuilder body, CDeclaredType type)
    {
        body.AppendLine($"int {type.CType}_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, {type.CType}* value)");
        body.AppendLine("{");
        body.AppendLine("  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;");

        body.AppendLine("  int result = 0;");
        body.AppendLine("  cmp_object_t start_obj;");
        body.AppendLine("  uint32_t array_size;");
        body.AppendLine("  uint32_t i;");
        body.AppendLine();

        body.AppendLine("  if (start_obj_ptr == NULL)");
        body.AppendLine("  {");
        body.AppendLine("    result = cmp_read_object(ctx, &start_obj);");
        body.AppendLine("    if (!result) return NMRPC_DESERIALIZE_ERR;");
        body.AppendLine("    start_obj_ptr = &start_obj;");
        body.AppendLine("  }");
        body.AppendLine();
        body.AppendLine("  result = cmp_object_as_array(start_obj_ptr, &array_size);");
        body.AppendLine($"  if (!result) return NMRPC_DESERIALIZE_ERR;");
        body.AppendLine();

        body.AppendLine("  value->length = (int)array_size;");
        body.AppendLine($"  value->array = ({type.GetTypeFromAray()}*) malloc(sizeof({type.GetTypeFromAray()}) * array_size);");


        body.AppendLine("  for (i = 0; i < array_size; i++)");
        body.AppendLine("  {");

        if (type.IsBaseType)
        {
            if (type.BaseDefinition == CDeclaredType.StringName)
            {
                body.AppendLine($"  result = cmph_read_nullable_str(ctx, &value->array[i]);");
                body.AppendLine("   if (result != NMRPC_OK) return result;");
                body.AppendLine($"   if (value->array[i] == NULL) return NMRPC_DESERIALIZE_ERR;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.BinaryName)
            {

                body.AppendLine($"  result = cmph_read_binary(ctx, &value->array[i]);");
                body.AppendLine("   if (result != NMRPC_OK) return result;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.Int32Name)
            {
                body.AppendLine($"  result = cmp_read_int(ctx, &value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }
            if (type.BaseDefinition == CDeclaredType.Int64Name)
            {
                body.AppendLine($"  result = cmp_read_long(ctx, &value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.UInt32Name)
            {
                body.AppendLine($"  result = cmp_read_uint(ctx, &value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.UInt64Name)
            {
                body.AppendLine($"  result = cmp_read_ulong(ctx, &value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }

            if (type.BaseDefinition == CDeclaredType.BoolName)
            {
                body.AppendLine($"  result = cmp_read_bool(ctx, &value->array[i]);");
                body.AppendLine("   if (!result) return NMRPC_FATAL_ERROR;");
                body.AppendLine();
            }
        }
        else
        {
            string readFnName = this.GetDeserializeFnName(type);

            body.AppendLine($"  result = {readFnName}(ctx, NULL, &value->array[i]);");
            body.AppendLine("   if (result != NMRPC_OK) return result;");
            body.AppendLine();
        }

        body.AppendLine("  }");

        body.AppendLine();
        body.AppendLine("    return NMRPC_OK;");
        body.AppendLine("}");
        body.AppendLine();

    }

    protected override void AddHeaderDefinitions(StringBuilder header)
    {
        header.AppendLine("// From https://github.com/camgunz/cmp");
        header.AppendLine("#include \"../utils/cmp.h\"");
    }

    protected override void RpcGenerator(StringBuilder body, string rpcName, RpcMethodDefinition rpcDef)
    {
        string requestType = rpcDef.Request;
        string responseType = rpcDef.Response;

        CDeclaredType crequestType = new CDeclaredType(requestType);
        CDeclaredType cresponseType = new CDeclaredType(responseType);

        body.AppendLine($$"""
         int nmrpc_call_{{rpcName}}(nmrpc_global_context_t* ctx, {{requestType}}* request, {{responseType}}* response)
         {
             if (ctx == NULL || request == NULL || response == NULL ) return NMRPC_BAD_ARGUMENT;

             int result = NMRPC_OK;
             uint8_t size_header[8];
             cmp_ctx_t write_body_ctx = {0};   
             cmp_ctx_t write_head_ctx = {0};

             cmp_ctx_t read_body_ctx = {0};
         
             InternalBuffer_t write_body_buffer = {0}; 
             InternalBuffer_t write_head_buffer = {0}; 
             InternalBuffer_t read_body_buffer = {0}; 
             InternalBuffer_t read_head_buffer = {0}; 

             size_t response_header_size;
             size_t response_body_size;
             bool is_connection_open = false;


             memset((void*) response, 0, sizeof({{responseType}}));

             result = InternalBuffer_init(&write_head_buffer, 256);
             if (result != NMRPC_OK)
             {
                 return result;
             }

             result = InternalBuffer_init(&write_body_buffer, 256);
             if (result != NMRPC_OK)
             {
                 return result;
             }

             cmp_init(&write_head_ctx, &write_head_buffer, mnrpc_empty_file_reader, mnrpc_empty_file_skipper, mnrpc_buffer_file_writer);
             cmp_init(&write_body_ctx, &write_body_buffer, mnrpc_empty_file_reader, mnrpc_empty_file_skipper, mnrpc_buffer_file_writer);

             //TODO: tag, nonce,...
             cmp_write_array(&write_head_ctx, 1);
             cmp_write_str(&write_head_ctx, "{{rpcName}}", {{rpcName.Length}});

             result = {{this.GetSerializeFnName(crequestType)}}(&write_body_ctx, request);
             if (result != NMRPC_OK)
             {
                 goto err;
             }

             // header and protocol version
             size_header[0] = 0xBC; // Bouncy Castle
             size_header[1] = 0;

             // header size
             size_header[2] = (write_head_buffer.size >> 8) & 0xFF;
             size_header[3] = write_head_buffer.size & 0xFF;

             // body size
             size_header[4] = (write_body_buffer.size >> 24) & 0xFF;
             size_header[5] = (write_body_buffer.size >> 16) & 0xFF;
             size_header[6] = (write_body_buffer.size >> 8) & 0xFF;
             size_header[7] = write_body_buffer.size & 0xFF;


             result = ctx->write(ctx->user_ctx, (void*)size_header, sizeof(size_header));
             if (result != NMRPC_OK)
             {
                 goto err;
             }

             is_connection_open = true;

             result = ctx->write(ctx->user_ctx, (void*)write_head_buffer.buffer, write_head_buffer.size);
             if (result != NMRPC_OK)
             {
                 goto err;
             }

             result = ctx->write(ctx->user_ctx, (void*)write_body_buffer.buffer, write_body_buffer.size);
             if (result != NMRPC_OK)
             {
                 goto err;
             }

             result = ctx->flush(ctx->user_ctx);
             if (result != NMRPC_OK)
             {
                 goto err;
             }

             // reading response

             memset(size_header, 0, sizeof(size_header));
             result = ctx->read(ctx->user_ctx, (void*)size_header, sizeof(size_header)) == sizeof(size_header);
             if (!result)
             {
                 goto err;
             }

             response_header_size = (size_t)size_header[3];
             response_header_size |= ((size_t)size_header[2]) << 8;

             response_body_size = (size_t)size_header[7];
             response_body_size |= ((size_t)size_header[6]) << 8;
             response_body_size |= ((size_t)size_header[5]) << 16;
             response_body_size |= ((size_t)size_header[4]) << 24;


             result = InternalBuffer_init(&read_head_buffer, response_header_size + 16);
             if (result != NMRPC_OK)
             {
                 return result;
             }

             result = InternalBuffer_init(&read_body_buffer, response_body_size + 16);
             if (result != NMRPC_OK)
             {
                 return result;
             }

             read_head_buffer.size = ctx->read(ctx->user_ctx, (void*)read_head_buffer.buffer, response_header_size);
             if (read_head_buffer.size != response_header_size)
             {
                 goto err;
             }

             read_body_buffer.size = ctx->read(ctx->user_ctx, (void*)read_body_buffer.buffer, response_body_size);
             if (read_body_buffer.size != response_body_size)
             {
                 goto err;
             }

             InternalBufferReader_t body_reader;
             body_reader.position = 0;
             body_reader.size = read_body_buffer.size;
             body_reader.buffer = read_body_buffer.buffer;

             cmp_init(&read_body_ctx, &body_reader, mnrpc_bufferReader_file_reader, mnrpc_bufferReader_file_skipper, mnrpc_empty_file_writer);

             result = {{this.GetDeserializeFnName(cresponseType)}}(&read_body_ctx, NULL, response);
             if (result != NMRPC_OK)
             {
                 goto err;
             }

             err:

             if (is_connection_open)
             {
                if (ctx->close(ctx->user_ctx) != NMRPC_OK)
                {
                   NMRPC_LOG_FAILED_CLOSE_SOCKET();
                }
             }

             InternalBuffer_free(&write_head_buffer);
             InternalBuffer_free(&write_body_buffer);
             InternalBuffer_free(&read_head_buffer);
             InternalBuffer_free(&read_body_buffer);

             return result;
         }
         """);

        body.AppendLine();
    }
}
