// This file is generated.
#include<stdlib.h>
#include "rpc.h"

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
int ArrayOfuint32_t_Serialize(cmp_ctx_t* ctx, ArrayOfuint32_t* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  int i = 0;

    result = cmp_write_array(ctx, value->length);
   if (!result) return NMRPC_FATAL_ERROR;

  for (i = 0; i < value->length; i++)
  {
  result = cmp_write_uinteger(ctx, value->array[i]);
   if (!result) return NMRPC_FATAL_ERROR;

  }

    return NMRPC_OK;
}

int ArrayOfuint32_t_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfuint32_t* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  uint32_t array_size;
  uint32_t i;

  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result) return NMRPC_DESERIALIZE_ERR;
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result) return NMRPC_DESERIALIZE_ERR;

  value->length = (int)array_size;
  value->array = (uint32_t*) malloc(sizeof(uint32_t) * array_size);
  for (i = 0; i < array_size; i++)
  {
  result = cmp_read_uint(ctx, &value->array[i]);
   if (!result) return NMRPC_FATAL_ERROR;

  }

    return NMRPC_OK;
}

int ArrayOfuint32_t_Release(ArrayOfuint32_t* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  free((void*) value->array);

  value->length = 0;
  value->array = NULL;
    return NMRPC_OK;
}
int ArrayOfAttrValueFromNative_Serialize(cmp_ctx_t* ctx, ArrayOfAttrValueFromNative* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  int i = 0;

    result = cmp_write_array(ctx, value->length);
   if (!result) return NMRPC_FATAL_ERROR;

  for (i = 0; i < value->length; i++)
  {
  result = AttrValueFromNative_Serialize(ctx, &value->array[i]);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  }

    return NMRPC_OK;
}

int ArrayOfAttrValueFromNative_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfAttrValueFromNative* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  uint32_t array_size;
  uint32_t i;

  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result) return NMRPC_DESERIALIZE_ERR;
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result) return NMRPC_DESERIALIZE_ERR;

  value->length = (int)array_size;
  value->array = (AttrValueFromNative*) malloc(sizeof(AttrValueFromNative) * array_size);
  for (i = 0; i < array_size; i++)
  {
  result = AttrValueFromNative_Deserialize(ctx, NULL, &value->array[i]);
   if (result != NMRPC_OK) return result;

  }

    return NMRPC_OK;
}

int ArrayOfAttrValueFromNative_Release(ArrayOfAttrValueFromNative* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  int i;
  for (i = 0; i < value->length; i++)
  {
      if (AttrValueFromNative_Release(&value->array[i]) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
  }

  free((void*) value->array);

  value->length = 0;
  value->array = NULL;
    return NMRPC_OK;
}
int ArrayOfGetAttributeInputValues_Serialize(cmp_ctx_t* ctx, ArrayOfGetAttributeInputValues* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  int i = 0;

    result = cmp_write_array(ctx, value->length);
   if (!result) return NMRPC_FATAL_ERROR;

  for (i = 0; i < value->length; i++)
  {
  result = GetAttributeInputValues_Serialize(ctx, &value->array[i]);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  }

    return NMRPC_OK;
}

int ArrayOfGetAttributeInputValues_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfGetAttributeInputValues* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  uint32_t array_size;
  uint32_t i;

  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result) return NMRPC_DESERIALIZE_ERR;
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result) return NMRPC_DESERIALIZE_ERR;

  value->length = (int)array_size;
  value->array = (GetAttributeInputValues*) malloc(sizeof(GetAttributeInputValues) * array_size);
  for (i = 0; i < array_size; i++)
  {
  result = GetAttributeInputValues_Deserialize(ctx, NULL, &value->array[i]);
   if (result != NMRPC_OK) return result;

  }

    return NMRPC_OK;
}

int ArrayOfGetAttributeInputValues_Release(ArrayOfGetAttributeInputValues* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  int i;
  for (i = 0; i < value->length; i++)
  {
      if (GetAttributeInputValues_Release(&value->array[i]) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
  }

  free((void*) value->array);

  value->length = 0;
  value->array = NULL;
    return NMRPC_OK;
}
int ArrayOfGetAttributeOutValue_Serialize(cmp_ctx_t* ctx, ArrayOfGetAttributeOutValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  int i = 0;

    result = cmp_write_array(ctx, value->length);
   if (!result) return NMRPC_FATAL_ERROR;

  for (i = 0; i < value->length; i++)
  {
  result = GetAttributeOutValue_Serialize(ctx, &value->array[i]);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  }

    return NMRPC_OK;
}

int ArrayOfGetAttributeOutValue_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfGetAttributeOutValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  uint32_t array_size;
  uint32_t i;

  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result) return NMRPC_DESERIALIZE_ERR;
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result) return NMRPC_DESERIALIZE_ERR;

  value->length = (int)array_size;
  value->array = (GetAttributeOutValue*) malloc(sizeof(GetAttributeOutValue) * array_size);
  for (i = 0; i < array_size; i++)
  {
  result = GetAttributeOutValue_Deserialize(ctx, NULL, &value->array[i]);
   if (result != NMRPC_OK) return result;

  }

    return NMRPC_OK;
}

int ArrayOfGetAttributeOutValue_Release(ArrayOfGetAttributeOutValue* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  int i;
  for (i = 0; i < value->length; i++)
  {
      if (GetAttributeOutValue_Release(&value->array[i]) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
  }

  free((void*) value->array);

  value->length = 0;
  value->array = NULL;
    return NMRPC_OK;
}
int AppIdentification_Serialize(cmp_ctx_t* ctx, AppIdentification* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->AppName, (uint32_t)strlen(value->AppName));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->AppNonce, (uint32_t)strlen(value->AppNonce));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Pid);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int AppIdentification_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, AppIdentification* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_nullable_str(ctx, &value->AppName);
   if (result != NMRPC_OK) return result;
   if (value->AppName == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmph_read_nullable_str(ctx, &value->AppNonce);
   if (result != NMRPC_OK) return result;
   if (value->AppNonce == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmp_read_ulong(ctx, &value->Pid);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int AppIdentification_Release(AppIdentification* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->AppName != NULL)
 {
     free((void*) value->AppName);
     value->AppName = NULL;
 }
 if (value->AppNonce != NULL)
 {
     free((void*) value->AppNonce);
     value->AppNonce = NULL;
 }
    return NMRPC_OK;
}
int ExtendedClientInfo_Serialize(cmp_ctx_t* ctx, ExtendedClientInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->CkUlongSize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PointerSize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->Platform, (uint32_t)strlen(value->Platform));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->LibVersion, (uint32_t)strlen(value->LibVersion));
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int ExtendedClientInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, ExtendedClientInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->CkUlongSize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PointerSize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_str(ctx, &value->Platform);
   if (result != NMRPC_OK) return result;
   if (value->Platform == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmph_read_nullable_str(ctx, &value->LibVersion);
   if (result != NMRPC_OK) return result;
   if (value->LibVersion == NULL) return NMRPC_DESERIALIZE_ERR;

    return NMRPC_OK;
}

int ExtendedClientInfo_Release(ExtendedClientInfo* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Platform != NULL)
 {
     free((void*) value->Platform);
     value->Platform = NULL;
 }
 if (value->LibVersion != NULL)
 {
     free((void*) value->LibVersion);
     value->LibVersion = NULL;
 }
    return NMRPC_OK;
}
int PingRequest_Serialize(cmp_ctx_t* ctx, PingRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int PingRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, PingRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int PingRequest_Release(PingRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int PingEnvelope_Serialize(cmp_ctx_t* ctx, PingEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int PingEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, PingEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int PingEnvelope_Release(PingEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int InitializeRequest_Serialize(cmp_ctx_t* ctx, InitializeRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsMutexFnSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->LibraryCantCreateOsThreads);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->OsLockingOk);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ExtendedClientInfo_Serialize(ctx, &value->ClientInfo);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int InitializeRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, InitializeRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsMutexFnSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->LibraryCantCreateOsThreads);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->OsLockingOk);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ExtendedClientInfo_Deserialize(ctx, NULL, &value->ClientInfo);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int InitializeRequest_Release(InitializeRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int InitializeEnvelope_Serialize(cmp_ctx_t* ctx, InitializeEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int InitializeEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, InitializeEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int InitializeEnvelope_Release(InitializeEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int FinalizeRequest_Serialize(cmp_ctx_t* ctx, FinalizeRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FinalizeRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FinalizeRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FinalizeRequest_Release(FinalizeRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int FinalizeEnvelope_Serialize(cmp_ctx_t* ctx, FinalizeEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FinalizeEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FinalizeEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FinalizeEnvelope_Release(FinalizeEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetInfoRequest_Serialize(cmp_ctx_t* ctx, GetInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GetInfoRequest_Release(GetInfoRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->ManufacturerID, (uint32_t)strlen(value->ManufacturerID));
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_str(ctx, &value->ManufacturerID);
   if (result != NMRPC_OK) return result;
   if (value->ManufacturerID == NULL) return NMRPC_DESERIALIZE_ERR;

    return NMRPC_OK;
}

int GetInfoEnvelope_Release(GetInfoEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->ManufacturerID != NULL)
 {
     free((void*) value->ManufacturerID);
     value->ManufacturerID = NULL;
 }
    return NMRPC_OK;
}
int GetSlotListRequest_Serialize(cmp_ctx_t* ctx, GetSlotListRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsTokenPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsSlotListPointerPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSlotListRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetSlotListRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsTokenPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsSlotListPointerPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSlotListRequest_Release(GetSlotListRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetSlotListEnvelope_Serialize(cmp_ctx_t* ctx, GetSlotListEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfuint32_t_Serialize(ctx, &value->Slots);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSlotListEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetSlotListEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfuint32_t_Deserialize(ctx, NULL, &value->Slots);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GetSlotListEnvelope_Release(GetSlotListEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfuint32_t_Release(&value->Slots) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int GetSlotInfoRequest_Serialize(cmp_ctx_t* ctx, GetSlotInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSlotInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetSlotInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSlotInfoRequest_Release(GetSlotInfoRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CkVersion_Serialize(cmp_ctx_t* ctx, CkVersion* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_integer(ctx, value->Major);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_integer(ctx, value->Minor);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkVersion_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkVersion* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_int(ctx, &value->Major);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_int(ctx, &value->Minor);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkVersion_Release(CkVersion* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CkSpecialUint_Serialize(cmp_ctx_t* ctx, CkSpecialUint* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->UnavailableInformation);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->EffectivelyInfinite);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->InformationSensitive);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Value);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkSpecialUint_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkSpecialUint* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_bool(ctx, &value->UnavailableInformation);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->EffectivelyInfinite);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->InformationSensitive);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->Value);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkSpecialUint_Release(CkSpecialUint* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SlotInfo_Serialize(cmp_ctx_t* ctx, SlotInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 7);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->SlotDescription, (uint32_t)strlen(value->SlotDescription));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->ManufacturerID, (uint32_t)strlen(value->ManufacturerID));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->FlagsTokenPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->FlagsRemovableDevice);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->FlagsHwSlot);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkVersion_Serialize(ctx, &value->HardwareVersion);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkVersion_Serialize(ctx, &value->FirmwareVersion);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SlotInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SlotInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 7) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_nullable_str(ctx, &value->SlotDescription);
   if (result != NMRPC_OK) return result;
   if (value->SlotDescription == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmph_read_nullable_str(ctx, &value->ManufacturerID);
   if (result != NMRPC_OK) return result;
   if (value->ManufacturerID == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmp_read_bool(ctx, &value->FlagsTokenPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->FlagsRemovableDevice);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->FlagsHwSlot);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkVersion_Deserialize(ctx, NULL, &value->HardwareVersion);
   if (result != NMRPC_OK) return result;

  result = CkVersion_Deserialize(ctx, NULL, &value->FirmwareVersion);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int SlotInfo_Release(SlotInfo* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->SlotDescription != NULL)
 {
     free((void*) value->SlotDescription);
     value->SlotDescription = NULL;
 }
 if (value->ManufacturerID != NULL)
 {
     free((void*) value->ManufacturerID);
     value->ManufacturerID = NULL;
 }
    return NMRPC_OK;
}
int GetSlotInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetSlotInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? SlotInfo_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSlotInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetSlotInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (SlotInfo*) malloc(sizeof(SlotInfo));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = SlotInfo_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetSlotInfoEnvelope_Release(GetSlotInfoEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (SlotInfo_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int GetTokenInfoRequest_Serialize(cmp_ctx_t* ctx, GetTokenInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetTokenInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetTokenInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetTokenInfoRequest_Release(GetTokenInfoRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int TokenInfo_Serialize(cmp_ctx_t* ctx, TokenInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 18);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->Label, (uint32_t)strlen(value->Label));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->ManufacturerId, (uint32_t)strlen(value->ManufacturerId));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->Model, (uint32_t)strlen(value->Model));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->SerialNumber, (uint32_t)strlen(value->SerialNumber));
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->MaxSessionCount);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->SessionCount);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->MaxRwSessionCount);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->RwSessionCount);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MaxPinLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MinPinLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->TotalPublicMemory);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->FreePublicMemory);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->TotalPrivateMemory);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->FreePrivateMemory);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkVersion_Serialize(ctx, &value->HardwareVersion);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = CkVersion_Serialize(ctx, &value->FirmwareVersion);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_str(ctx, value->UtcTime, (uint32_t)strlen(value->UtcTime));
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int TokenInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, TokenInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 18) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_nullable_str(ctx, &value->Label);
   if (result != NMRPC_OK) return result;
   if (value->Label == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmph_read_nullable_str(ctx, &value->ManufacturerId);
   if (result != NMRPC_OK) return result;
   if (value->ManufacturerId == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmph_read_nullable_str(ctx, &value->Model);
   if (result != NMRPC_OK) return result;
   if (value->Model == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmph_read_nullable_str(ctx, &value->SerialNumber);
   if (result != NMRPC_OK) return result;
   if (value->SerialNumber == NULL) return NMRPC_DESERIALIZE_ERR;

  result = cmp_read_uint(ctx, &value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->MaxSessionCount);
   if (result != NMRPC_OK) return result;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->SessionCount);
   if (result != NMRPC_OK) return result;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->MaxRwSessionCount);
   if (result != NMRPC_OK) return result;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->RwSessionCount);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->MaxPinLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->MinPinLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->TotalPublicMemory);
   if (result != NMRPC_OK) return result;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->FreePublicMemory);
   if (result != NMRPC_OK) return result;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->TotalPrivateMemory);
   if (result != NMRPC_OK) return result;

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->FreePrivateMemory);
   if (result != NMRPC_OK) return result;

  result = CkVersion_Deserialize(ctx, NULL, &value->HardwareVersion);
   if (result != NMRPC_OK) return result;

  result = CkVersion_Deserialize(ctx, NULL, &value->FirmwareVersion);
   if (result != NMRPC_OK) return result;

  result = cmph_read_nullable_str(ctx, &value->UtcTime);
   if (result != NMRPC_OK) return result;
   if (value->UtcTime == NULL) return NMRPC_DESERIALIZE_ERR;

    return NMRPC_OK;
}

int TokenInfo_Release(TokenInfo* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Label != NULL)
 {
     free((void*) value->Label);
     value->Label = NULL;
 }
 if (value->ManufacturerId != NULL)
 {
     free((void*) value->ManufacturerId);
     value->ManufacturerId = NULL;
 }
 if (value->Model != NULL)
 {
     free((void*) value->Model);
     value->Model = NULL;
 }
 if (value->SerialNumber != NULL)
 {
     free((void*) value->SerialNumber);
     value->SerialNumber = NULL;
 }
 if (value->UtcTime != NULL)
 {
     free((void*) value->UtcTime);
     value->UtcTime = NULL;
 }
    return NMRPC_OK;
}
int GetTokenInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetTokenInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? TokenInfo_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetTokenInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetTokenInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (TokenInfo*) malloc(sizeof(TokenInfo));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = TokenInfo_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetTokenInfoEnvelope_Release(GetTokenInfoEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (TokenInfo_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int GetMechanismListRequest_Serialize(cmp_ctx_t* ctx, GetMechanismListRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsMechanismListPointerPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetMechanismListRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetMechanismListRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsMechanismListPointerPresent);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetMechanismListRequest_Release(GetMechanismListRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int MechanismList_Serialize(cmp_ctx_t* ctx, MechanismList* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfuint32_t_Serialize(ctx, &value->MechanismTypes);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int MechanismList_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, MechanismList* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->PullCount);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfuint32_t_Deserialize(ctx, NULL, &value->MechanismTypes);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int MechanismList_Release(MechanismList* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfuint32_t_Release(&value->MechanismTypes) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int GetMechanismListEnvelope_Serialize(cmp_ctx_t* ctx, GetMechanismListEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? MechanismList_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetMechanismListEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetMechanismListEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (MechanismList*) malloc(sizeof(MechanismList));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = MechanismList_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetMechanismListEnvelope_Release(GetMechanismListEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (MechanismList_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int GetMechanismInfoRequest_Serialize(cmp_ctx_t* ctx, GetMechanismInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MechanismType);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetMechanismInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetMechanismInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->MechanismType);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetMechanismInfoRequest_Release(GetMechanismInfoRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int MechanismInfo_Serialize(cmp_ctx_t* ctx, MechanismInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MechanismType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MinKeySize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MaxKeySize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int MechanismInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, MechanismInfo* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->MechanismType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->MinKeySize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->MaxKeySize);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int MechanismInfo_Release(MechanismInfo* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetMechanismInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetMechanismInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? MechanismInfo_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetMechanismInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetMechanismInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (MechanismInfo*) malloc(sizeof(MechanismInfo));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = MechanismInfo_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetMechanismInfoEnvelope_Release(GetMechanismInfoEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (MechanismInfo_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int OpenSessionRequest_Serialize(cmp_ctx_t* ctx, OpenSessionRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsPtrApplicationSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsNotifySet);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int OpenSessionRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, OpenSessionRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsPtrApplicationSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsNotifySet);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int OpenSessionRequest_Release(OpenSessionRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int OpenSessionEnvelope_Serialize(cmp_ctx_t* ctx, OpenSessionEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int OpenSessionEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, OpenSessionEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int OpenSessionEnvelope_Release(OpenSessionEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CloseSessionRequest_Serialize(cmp_ctx_t* ctx, CloseSessionRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseSessionRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CloseSessionRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseSessionRequest_Release(CloseSessionRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CloseSessionEnvelope_Serialize(cmp_ctx_t* ctx, CloseSessionEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseSessionEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CloseSessionEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseSessionEnvelope_Release(CloseSessionEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CloseAllSessionsRequest_Serialize(cmp_ctx_t* ctx, CloseAllSessionsRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseAllSessionsRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CloseAllSessionsRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseAllSessionsRequest_Release(CloseAllSessionsRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CloseAllSessionsEnvelope_Serialize(cmp_ctx_t* ctx, CloseAllSessionsEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseAllSessionsEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CloseAllSessionsEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CloseAllSessionsEnvelope_Release(CloseAllSessionsEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetSessionInfoRequest_Serialize(cmp_ctx_t* ctx, GetSessionInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSessionInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetSessionInfoRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSessionInfoRequest_Release(GetSessionInfoRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SessionInfoData_Serialize(cmp_ctx_t* ctx, SessionInfoData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->State);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->DeviceError);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SessionInfoData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SessionInfoData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->SlotId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->State);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->Flags);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->DeviceError);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SessionInfoData_Release(SessionInfoData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetSessionInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetSessionInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? SessionInfoData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetSessionInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetSessionInfoEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (SessionInfoData*) malloc(sizeof(SessionInfoData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = SessionInfoData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetSessionInfoEnvelope_Release(GetSessionInfoEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (SessionInfoData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int LoginRequest_Serialize(cmp_ctx_t* ctx, LoginRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->UserType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Utf8Pin != NULL)? cmp_write_bin(ctx, value->Utf8Pin->data, (uint32_t)value->Utf8Pin->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LoginRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, LoginRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->UserType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->Utf8Pin);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int LoginRequest_Release(LoginRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->Utf8Pin != NULL)
  {
      Binary_Release(value->Utf8Pin);
      free((void*)value->Utf8Pin);
      value->Utf8Pin = NULL;
  }
    return NMRPC_OK;
}
int LoginEnvelope_Serialize(cmp_ctx_t* ctx, LoginEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LoginEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, LoginEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LoginEnvelope_Release(LoginEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int LogoutRequest_Serialize(cmp_ctx_t* ctx, LogoutRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LogoutRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, LogoutRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LogoutRequest_Release(LogoutRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int LogoutEnvelope_Serialize(cmp_ctx_t* ctx, LogoutEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LogoutEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, LogoutEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int LogoutEnvelope_Release(LogoutEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SeedRandomRequest_Serialize(cmp_ctx_t* ctx, SeedRandomRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Seed.data, (uint32_t)value->Seed.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SeedRandomRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SeedRandomRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Seed);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int SeedRandomRequest_Release(SeedRandomRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Seed);
    return NMRPC_OK;
}
int SeedRandomEnvelope_Serialize(cmp_ctx_t* ctx, SeedRandomEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SeedRandomEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SeedRandomEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SeedRandomEnvelope_Release(SeedRandomEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GenerateRandomRequest_Serialize(cmp_ctx_t* ctx, GenerateRandomRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->RandomLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateRandomRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateRandomRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->RandomLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateRandomRequest_Release(GenerateRandomRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GenerateRandomEnvelope_Serialize(cmp_ctx_t* ctx, GenerateRandomEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? cmp_write_bin(ctx, value->Data->data, (uint32_t)value->Data->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateRandomEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateRandomEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GenerateRandomEnvelope_Release(GenerateRandomEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->Data != NULL)
  {
      Binary_Release(value->Data);
      free((void*)value->Data);
      value->Data = NULL;
  }
    return NMRPC_OK;
}
int MechanismValue_Serialize(cmp_ctx_t* ctx, MechanismValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MechanismType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->MechanismParamMp != NULL)? cmp_write_bin(ctx, value->MechanismParamMp->data, (uint32_t)value->MechanismParamMp->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int MechanismValue_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, MechanismValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->MechanismType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->MechanismParamMp);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int MechanismValue_Release(MechanismValue* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->MechanismParamMp != NULL)
  {
      Binary_Release(value->MechanismParamMp);
      free((void*)value->MechanismParamMp);
      value->MechanismParamMp = NULL;
  }
    return NMRPC_OK;
}
int DigestInitRequest_Serialize(cmp_ctx_t* ctx, DigestInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int DigestInitRequest_Release(DigestInitRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DigestInitEnvelope_Serialize(cmp_ctx_t* ctx, DigestInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestInitEnvelope_Release(DigestInitEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DigestRequest_Serialize(cmp_ctx_t* ctx, DigestRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsDigestPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PulDigestLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsDigestPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PulDigestLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestRequest_Release(DigestRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int DigestValue_Serialize(cmp_ctx_t* ctx, DigestValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PulDigestLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? cmp_write_bin(ctx, value->Data->data, (uint32_t)value->Data->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestValue_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->PulDigestLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int DigestValue_Release(DigestValue* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->Data != NULL)
  {
      Binary_Release(value->Data);
      free((void*)value->Data);
      value->Data = NULL;
  }
    return NMRPC_OK;
}
int DigestEnvelope_Serialize(cmp_ctx_t* ctx, DigestEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? DigestValue_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (DigestValue*) malloc(sizeof(DigestValue));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = DigestValue_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int DigestEnvelope_Release(DigestEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (DigestValue_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int DigestUpdateRequest_Serialize(cmp_ctx_t* ctx, DigestUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int DigestUpdateRequest_Release(DigestUpdateRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int DigestUpdateEnvelope_Serialize(cmp_ctx_t* ctx, DigestUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestUpdateEnvelope_Release(DigestUpdateEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DigestKeyRequest_Serialize(cmp_ctx_t* ctx, DigestKeyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestKeyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestKeyRequest_Release(DigestKeyRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DigestKeyEnvelope_Serialize(cmp_ctx_t* ctx, DigestKeyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestKeyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestKeyEnvelope_Release(DigestKeyEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DigestFinalRequest_Serialize(cmp_ctx_t* ctx, DigestFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsDigestPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PulDigestLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsDigestPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PulDigestLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestFinalRequest_Release(DigestFinalRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DigestFinalEnvelope_Serialize(cmp_ctx_t* ctx, DigestFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? DigestValue_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DigestFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DigestFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (DigestValue*) malloc(sizeof(DigestValue));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = DigestValue_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int DigestFinalEnvelope_Release(DigestFinalEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (DigestValue_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int AttrValueFromNative_Serialize(cmp_ctx_t* ctx, AttrValueFromNative* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 6);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->AttributeType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_integer(ctx, value->ValueTypeHint);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->ValueRawBytes.data, (uint32_t)value->ValueRawBytes.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->ValueBool);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ValueCkUlong);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->ValueCkDate != NULL)? cmp_write_str(ctx, value->ValueCkDate, (uint32_t)strlen(value->ValueCkDate)) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int AttrValueFromNative_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, AttrValueFromNative* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 6) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->AttributeType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_int(ctx, &value->ValueTypeHint);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->ValueRawBytes);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->ValueBool);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ValueCkUlong);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_str(ctx, &value->ValueCkDate);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int AttrValueFromNative_Release(AttrValueFromNative* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->ValueRawBytes);
 if (value->ValueCkDate != NULL)
 {
     free((void*) value->ValueCkDate);
     value->ValueCkDate = NULL;
 }
    return NMRPC_OK;
}
int CreateObjectRequest_Serialize(cmp_ctx_t* ctx, CreateObjectRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Serialize(ctx, &value->Template);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CreateObjectRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CreateObjectRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Deserialize(ctx, NULL, &value->Template);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int CreateObjectRequest_Release(CreateObjectRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfAttrValueFromNative_Release(&value->Template) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int CreateObjectEnvelope_Serialize(cmp_ctx_t* ctx, CreateObjectEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CreateObjectEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CreateObjectEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CreateObjectEnvelope_Release(CreateObjectEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DestroyObjectRequest_Serialize(cmp_ctx_t* ctx, DestroyObjectRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DestroyObjectRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DestroyObjectRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DestroyObjectRequest_Release(DestroyObjectRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DestroyObjectEnvelope_Serialize(cmp_ctx_t* ctx, DestroyObjectEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DestroyObjectEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DestroyObjectEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DestroyObjectEnvelope_Release(DestroyObjectEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int FindObjectsInitRequest_Serialize(cmp_ctx_t* ctx, FindObjectsInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Serialize(ctx, &value->Template);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Deserialize(ctx, NULL, &value->Template);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int FindObjectsInitRequest_Release(FindObjectsInitRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfAttrValueFromNative_Release(&value->Template) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int FindObjectsInitEnvelope_Serialize(cmp_ctx_t* ctx, FindObjectsInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsInitEnvelope_Release(FindObjectsInitEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int FindObjectsRequest_Serialize(cmp_ctx_t* ctx, FindObjectsRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MaxObjectCount);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->MaxObjectCount);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsRequest_Release(FindObjectsRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int FindObjectsData_Serialize(cmp_ctx_t* ctx, FindObjectsData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullObjectCount);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfuint32_t_Serialize(ctx, &value->Objects);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->PullObjectCount);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfuint32_t_Deserialize(ctx, NULL, &value->Objects);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int FindObjectsData_Release(FindObjectsData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfuint32_t_Release(&value->Objects) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int FindObjectsEnvelope_Serialize(cmp_ctx_t* ctx, FindObjectsEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? FindObjectsData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (FindObjectsData*) malloc(sizeof(FindObjectsData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = FindObjectsData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int FindObjectsEnvelope_Release(FindObjectsEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (FindObjectsData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int FindObjectsFinalRequest_Serialize(cmp_ctx_t* ctx, FindObjectsFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsFinalRequest_Release(FindObjectsFinalRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int FindObjectsFinalEnvelope_Serialize(cmp_ctx_t* ctx, FindObjectsFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, FindObjectsFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int FindObjectsFinalEnvelope_Release(FindObjectsFinalEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetObjectSizeRequest_Serialize(cmp_ctx_t* ctx, GetObjectSizeRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetObjectSizeRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetObjectSizeRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetObjectSizeRequest_Release(GetObjectSizeRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetObjectSizeEnvelope_Serialize(cmp_ctx_t* ctx, GetObjectSizeEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? CkSpecialUint_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetObjectSizeEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetObjectSizeEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (CkSpecialUint*) malloc(sizeof(CkSpecialUint));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = CkSpecialUint_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetObjectSizeEnvelope_Release(GetObjectSizeEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (CkSpecialUint_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int GetAttributeInputValues_Serialize(cmp_ctx_t* ctx, GetAttributeInputValues* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->AttributeType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsValuePtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ValueLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetAttributeInputValues_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetAttributeInputValues* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->AttributeType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsValuePtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ValueLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetAttributeInputValues_Release(GetAttributeInputValues* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GetAttributeValueRequest_Serialize(cmp_ctx_t* ctx, GetAttributeValueRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfGetAttributeInputValues_Serialize(ctx, &value->InTemplate);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetAttributeValueRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetAttributeValueRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfGetAttributeInputValues_Deserialize(ctx, NULL, &value->InTemplate);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GetAttributeValueRequest_Release(GetAttributeValueRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfGetAttributeInputValues_Release(&value->InTemplate) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int GetAttributeOutValue_Serialize(cmp_ctx_t* ctx, GetAttributeOutValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 6);
   if (!result) return NMRPC_FATAL_ERROR;

  result = CkSpecialUint_Serialize(ctx, &value->ValueLen);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_integer(ctx, value->ValueType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->ValueUint);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->ValueBool);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->ValueBytes.data, (uint32_t)value->ValueBytes.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->ValueCkDate != NULL)? cmp_write_str(ctx, value->ValueCkDate, (uint32_t)strlen(value->ValueCkDate)) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetAttributeOutValue_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetAttributeOutValue* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 6) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = CkSpecialUint_Deserialize(ctx, NULL, &value->ValueLen);
   if (result != NMRPC_OK) return result;

  result = cmp_read_int(ctx, &value->ValueType);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->ValueUint);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->ValueBool);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->ValueBytes);
   if (result != NMRPC_OK) return result;

  result = cmph_read_nullable_str(ctx, &value->ValueCkDate);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GetAttributeOutValue_Release(GetAttributeOutValue* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->ValueBytes);
 if (value->ValueCkDate != NULL)
 {
     free((void*) value->ValueCkDate);
     value->ValueCkDate = NULL;
 }
    return NMRPC_OK;
}
int GetAttributeOutValues_Serialize(cmp_ctx_t* ctx, GetAttributeOutValues* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfGetAttributeOutValue_Serialize(ctx, &value->OutTemplate);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetAttributeOutValues_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetAttributeOutValues* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = ArrayOfGetAttributeOutValue_Deserialize(ctx, NULL, &value->OutTemplate);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GetAttributeOutValues_Release(GetAttributeOutValues* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfGetAttributeOutValue_Release(&value->OutTemplate) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int GetAttributeValueEnvelope_Serialize(cmp_ctx_t* ctx, GetAttributeValueEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? GetAttributeOutValues_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GetAttributeValueEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GetAttributeValueEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (GetAttributeOutValues*) malloc(sizeof(GetAttributeOutValues));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = GetAttributeOutValues_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GetAttributeValueEnvelope_Release(GetAttributeValueEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (GetAttributeOutValues_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int GenerateKeyPairRequest_Serialize(cmp_ctx_t* ctx, GenerateKeyPairRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Serialize(ctx, &value->PublicKeyTemplate);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Serialize(ctx, &value->PrivateKeyTemplate);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyPairRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateKeyPairRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = ArrayOfAttrValueFromNative_Deserialize(ctx, NULL, &value->PublicKeyTemplate);
   if (result != NMRPC_OK) return result;

  result = ArrayOfAttrValueFromNative_Deserialize(ctx, NULL, &value->PrivateKeyTemplate);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GenerateKeyPairRequest_Release(GenerateKeyPairRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfAttrValueFromNative_Release(&value->PublicKeyTemplate) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
  if(ArrayOfAttrValueFromNative_Release(&value->PrivateKeyTemplate) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int GenerateKeyPairData_Serialize(cmp_ctx_t* ctx, GenerateKeyPairData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PublicKeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PrivateKeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyPairData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateKeyPairData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->PublicKeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PrivateKeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyPairData_Release(GenerateKeyPairData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GenerateKeyPairEnvelope_Serialize(cmp_ctx_t* ctx, GenerateKeyPairEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? GenerateKeyPairData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyPairEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateKeyPairEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (GenerateKeyPairData*) malloc(sizeof(GenerateKeyPairData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = GenerateKeyPairData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GenerateKeyPairEnvelope_Release(GenerateKeyPairEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (GenerateKeyPairData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int SignInitRequest_Serialize(cmp_ctx_t* ctx, SignInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignInitRequest_Release(SignInitRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SignInitEnvelope_Serialize(cmp_ctx_t* ctx, SignInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignInitEnvelope_Release(SignInitEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SignRequest_Serialize(cmp_ctx_t* ctx, SignRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsSignaturePtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullSignatureLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsSignaturePtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullSignatureLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignRequest_Release(SignRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int SignatureData_Serialize(cmp_ctx_t* ctx, SignatureData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullSignatureLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Signature.data, (uint32_t)value->Signature.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignatureData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignatureData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->PullSignatureLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Signature);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int SignatureData_Release(SignatureData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Signature);
    return NMRPC_OK;
}
int SignEnvelope_Serialize(cmp_ctx_t* ctx, SignEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? SignatureData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (SignatureData*) malloc(sizeof(SignatureData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = SignatureData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int SignEnvelope_Release(SignEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (SignatureData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int SignUpdateRequest_Serialize(cmp_ctx_t* ctx, SignUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int SignUpdateRequest_Release(SignUpdateRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int SignUpdateEnvelope_Serialize(cmp_ctx_t* ctx, SignUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignUpdateEnvelope_Release(SignUpdateEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SignFinalRequest_Serialize(cmp_ctx_t* ctx, SignFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsSignaturePtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullSignatureLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsSignaturePtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullSignatureLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignFinalRequest_Release(SignFinalRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int SignFinalEnvelope_Serialize(cmp_ctx_t* ctx, SignFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? SignatureData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int SignFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, SignFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (SignatureData*) malloc(sizeof(SignatureData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = SignatureData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int SignFinalEnvelope_Release(SignFinalEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (SignatureData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int VerifyInitRequest_Serialize(cmp_ctx_t* ctx, VerifyInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyInitRequest_Release(VerifyInitRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int VerifyInitEnvelope_Serialize(cmp_ctx_t* ctx, VerifyInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyInitEnvelope_Release(VerifyInitEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int VerifyRequest_Serialize(cmp_ctx_t* ctx, VerifyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Signature.data, (uint32_t)value->Signature.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

  result = cmph_read_binary(ctx, &value->Signature);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int VerifyRequest_Release(VerifyRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
  Binary_Release(&value->Signature);
    return NMRPC_OK;
}
int VerifyEnvelope_Serialize(cmp_ctx_t* ctx, VerifyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyEnvelope_Release(VerifyEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int VerifyUpdateRequest_Serialize(cmp_ctx_t* ctx, VerifyUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int VerifyUpdateRequest_Release(VerifyUpdateRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int VerifyUpdateEnvelope_Serialize(cmp_ctx_t* ctx, VerifyUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyUpdateEnvelope_Release(VerifyUpdateEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int VerifyFinalRequest_Serialize(cmp_ctx_t* ctx, VerifyFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Signature.data, (uint32_t)value->Signature.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Signature);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int VerifyFinalRequest_Release(VerifyFinalRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Signature);
    return NMRPC_OK;
}
int VerifyFinalEnvelope_Serialize(cmp_ctx_t* ctx, VerifyFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, VerifyFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int VerifyFinalEnvelope_Release(VerifyFinalEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GenerateKeyRequest_Serialize(cmp_ctx_t* ctx, GenerateKeyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Serialize(ctx, &value->Template);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateKeyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = ArrayOfAttrValueFromNative_Deserialize(ctx, NULL, &value->Template);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int GenerateKeyRequest_Release(GenerateKeyRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfAttrValueFromNative_Release(&value->Template) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int GenerateKeyData_Serialize(cmp_ctx_t* ctx, GenerateKeyData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->KeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateKeyData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->KeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyData_Release(GenerateKeyData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int GenerateKeyEnvelope_Serialize(cmp_ctx_t* ctx, GenerateKeyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? GenerateKeyData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int GenerateKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, GenerateKeyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (GenerateKeyData*) malloc(sizeof(GenerateKeyData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = GenerateKeyData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int GenerateKeyEnvelope_Release(GenerateKeyEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (GenerateKeyData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int DeriveKeyRequest_Serialize(cmp_ctx_t* ctx, DeriveKeyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->BaseKeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Serialize(ctx, &value->Template);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DeriveKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DeriveKeyRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->BaseKeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

  result = ArrayOfAttrValueFromNative_Deserialize(ctx, NULL, &value->Template);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int DeriveKeyRequest_Release(DeriveKeyRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if(ArrayOfAttrValueFromNative_Release(&value->Template) != NMRPC_OK)
   {
       return NMRPC_FATAL_ERROR;
   }
    return NMRPC_OK;
}
int DeriveKeyData_Serialize(cmp_ctx_t* ctx, DeriveKeyData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->KeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DeriveKeyData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DeriveKeyData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->KeyHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DeriveKeyData_Release(DeriveKeyData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DeriveKeyEnvelope_Serialize(cmp_ctx_t* ctx, DeriveKeyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? DeriveKeyData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DeriveKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DeriveKeyEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (DeriveKeyData*) malloc(sizeof(DeriveKeyData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = DeriveKeyData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int DeriveKeyEnvelope_Release(DeriveKeyEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (DeriveKeyData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int EncryptInitRequest_Serialize(cmp_ctx_t* ctx, EncryptInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptInitRequest_Release(EncryptInitRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int EncryptInitEnvelope_Serialize(cmp_ctx_t* ctx, EncryptInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptInitEnvelope_Release(EncryptInitEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int EncryptRequest_Serialize(cmp_ctx_t* ctx, EncryptRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsEncryptedDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->EncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsEncryptedDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->EncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptRequest_Release(EncryptRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int EncryptData_Serialize(cmp_ctx_t* ctx, EncryptData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullEncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->EncryptedData.data, (uint32_t)value->EncryptedData.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->PullEncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->EncryptedData);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int EncryptData_Release(EncryptData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->EncryptedData);
    return NMRPC_OK;
}
int EncryptEnvelope_Serialize(cmp_ctx_t* ctx, EncryptEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? EncryptData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (EncryptData*) malloc(sizeof(EncryptData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = EncryptData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int EncryptEnvelope_Release(EncryptEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (EncryptData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int EncryptUpdateRequest_Serialize(cmp_ctx_t* ctx, EncryptUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->PartData.data, (uint32_t)value->PartData.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsEncryptedDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->EncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->PartData);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsEncryptedDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->EncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptUpdateRequest_Release(EncryptUpdateRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->PartData);
    return NMRPC_OK;
}
int EncryptUpdateEnvelope_Serialize(cmp_ctx_t* ctx, EncryptUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? EncryptData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (EncryptData*) malloc(sizeof(EncryptData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = EncryptData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int EncryptUpdateEnvelope_Release(EncryptUpdateEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (EncryptData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int EncryptFinalRequest_Serialize(cmp_ctx_t* ctx, EncryptFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsEncryptedDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->EncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsEncryptedDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->EncryptedDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptFinalRequest_Release(EncryptFinalRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int EncryptFinalEnvelope_Serialize(cmp_ctx_t* ctx, EncryptFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? EncryptData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int EncryptFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, EncryptFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (EncryptData*) malloc(sizeof(EncryptData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = EncryptData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int EncryptFinalEnvelope_Release(EncryptFinalEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (EncryptData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int DecryptInitRequest_Serialize(cmp_ctx_t* ctx, DecryptInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Serialize(ctx, &value->Mechanism);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptInitRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = MechanismValue_Deserialize(ctx, NULL, &value->Mechanism);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->KeyObjectHandle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptInitRequest_Release(DecryptInitRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DecryptInitEnvelope_Serialize(cmp_ctx_t* ctx, DecryptInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptInitEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptInitEnvelope_Release(DecryptInitEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DecryptRequest_Serialize(cmp_ctx_t* ctx, DecryptRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->EncryptedData.data, (uint32_t)value->EncryptedData.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->EncryptedData);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptRequest_Release(DecryptRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->EncryptedData);
    return NMRPC_OK;
}
int DecryptData_Serialize(cmp_ctx_t* ctx, DecryptData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptData_Release(DecryptData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int DecryptEnvelope_Serialize(cmp_ctx_t* ctx, DecryptEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? DecryptData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (DecryptData*) malloc(sizeof(DecryptData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = DecryptData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int DecryptEnvelope_Release(DecryptEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (DecryptData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int DecryptUpdateRequest_Serialize(cmp_ctx_t* ctx, DecryptUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 5);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->EncryptedData.data, (uint32_t)value->EncryptedData.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptUpdateRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 5) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_binary(ctx, &value->EncryptedData);
   if (result != NMRPC_OK) return result;

  result = cmp_read_bool(ctx, &value->IsDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptUpdateRequest_Release(DecryptUpdateRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->EncryptedData);
    return NMRPC_OK;
}
int DecryptUpdateEnvelope_Serialize(cmp_ctx_t* ctx, DecryptUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? DecryptData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptUpdateEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (DecryptData*) malloc(sizeof(DecryptData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = DecryptData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int DecryptUpdateEnvelope_Release(DecryptUpdateEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (DecryptData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int DecryptFinalRequest_Serialize(cmp_ctx_t* ctx, DecryptFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = AppIdentification_Serialize(ctx, &value->AppId);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bool(ctx, value->IsDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptFinalRequest* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = AppIdentification_Deserialize(ctx, NULL, &value->AppId);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->SessionId);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_bool(ctx, &value->IsDataPtrSet);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->PullDataLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptFinalRequest_Release(DecryptFinalRequest* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int DecryptFinalEnvelope_Serialize(cmp_ctx_t* ctx, DecryptFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Data != NULL)? DecryptData_Serialize(ctx, value->Data) : cmp_write_nil(ctx);
   if (result != NMRPC_OK) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int DecryptFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, DecryptFinalEnvelope* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Rv);
   if (!result) return NMRPC_FATAL_ERROR;

  cmp_read_object(ctx, &tmp_obj);
  if (cmp_object_is_nil(&tmp_obj))
  {
      value->Data = NULL;
  }
  else
  {
     value->Data = (DecryptData*) malloc(sizeof(DecryptData));
     if (value->Data == NULL) return NMRPC_FATAL_ERROR;
     result = DecryptData_Deserialize(ctx, &tmp_obj, value->Data);
      if (result != NMRPC_OK) return result;
  }

    return NMRPC_OK;
}

int DecryptFinalEnvelope_Release(DecryptFinalEnvelope* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

 if (value->Data != NULL) 
 {
     if (DecryptData_Release(value->Data) != NMRPC_OK)
     {
        return NMRPC_FATAL_ERROR;
     }
     free((void*) value->Data);
     value->Data = NULL;
 }
    return NMRPC_OK;
}
int CkP_MacGeneralParams_Serialize(cmp_ctx_t* ctx, CkP_MacGeneralParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Value);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_MacGeneralParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkP_MacGeneralParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Value);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_MacGeneralParams_Release(CkP_MacGeneralParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CkP_ExtractParams_Serialize(cmp_ctx_t* ctx, CkP_ExtractParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Value);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_ExtractParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkP_ExtractParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Value);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_ExtractParams_Release(CkP_ExtractParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CkP_RsaPkcsPssParams_Serialize(cmp_ctx_t* ctx, CkP_RsaPkcsPssParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->HashAlg);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Mgf);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->SLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_RsaPkcsPssParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkP_RsaPkcsPssParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->HashAlg);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->Mgf);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_read_uint(ctx, &value->SLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_RsaPkcsPssParams_Release(CkP_RsaPkcsPssParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int CkP_RawDataParams_Serialize(cmp_ctx_t* ctx, CkP_RawDataParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Value.data, (uint32_t)value->Value.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_RawDataParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkP_RawDataParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_binary(ctx, &value->Value);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int CkP_RawDataParams_Release(CkP_RawDataParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Value);
    return NMRPC_OK;
}
int CkP_KeyDerivationStringData_Serialize(cmp_ctx_t* ctx, CkP_KeyDerivationStringData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 2);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->Data.data, (uint32_t)value->Data.size);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Len);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_KeyDerivationStringData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkP_KeyDerivationStringData* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 2) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_binary(ctx, &value->Data);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->Len);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_KeyDerivationStringData_Release(CkP_KeyDerivationStringData* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  Binary_Release(&value->Data);
    return NMRPC_OK;
}
int CkP_CkObjectHandle_Serialize(cmp_ctx_t* ctx, CkP_CkObjectHandle* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 1);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Handle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_CkObjectHandle_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, CkP_CkObjectHandle* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 1) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Handle);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int CkP_CkObjectHandle_Release(CkP_CkObjectHandle* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

    return NMRPC_OK;
}
int Ckp_CkEcdh1DeriveParams_Serialize(cmp_ctx_t* ctx, Ckp_CkEcdh1DeriveParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 3);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->Kdf);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->SharedData != NULL)? cmp_write_bin(ctx, value->SharedData->data, (uint32_t)value->SharedData->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_bin(ctx, value->PublicData.data, (uint32_t)value->PublicData.size);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int Ckp_CkEcdh1DeriveParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, Ckp_CkEcdh1DeriveParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 3) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->Kdf);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->SharedData);
   if (result != NMRPC_OK) return result;

  result = cmph_read_binary(ctx, &value->PublicData);
   if (result != NMRPC_OK) return result;

    return NMRPC_OK;
}

int Ckp_CkEcdh1DeriveParams_Release(Ckp_CkEcdh1DeriveParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->SharedData != NULL)
  {
      Binary_Release(value->SharedData);
      free((void*)value->SharedData);
      value->SharedData = NULL;
  }
  Binary_Release(&value->PublicData);
    return NMRPC_OK;
}
int Ckp_CkGcmParams_Serialize(cmp_ctx_t* ctx, Ckp_CkGcmParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Iv != NULL)? cmp_write_bin(ctx, value->Iv->data, (uint32_t)value->Iv->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->IvBits);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Aad != NULL)? cmp_write_bin(ctx, value->Aad->data, (uint32_t)value->Aad->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->TagBits);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int Ckp_CkGcmParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, Ckp_CkGcmParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmph_read_nullable_binary(ctx, &value->Iv);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->IvBits);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->Aad);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->TagBits);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int Ckp_CkGcmParams_Release(Ckp_CkGcmParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->Iv != NULL)
  {
      Binary_Release(value->Iv);
      free((void*)value->Iv);
      value->Iv = NULL;
  }
  if (value->Aad != NULL)
  {
      Binary_Release(value->Aad);
      free((void*)value->Aad);
      value->Aad = NULL;
  }
    return NMRPC_OK;
}
int Ckp_CkCcmParams_Serialize(cmp_ctx_t* ctx, Ckp_CkCcmParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;

    result = cmp_write_array(ctx, 4);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->DataLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Nonce != NULL)? cmp_write_bin(ctx, value->Nonce->data, (uint32_t)value->Nonce->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

  result = (value->Aad != NULL)? cmp_write_bin(ctx, value->Aad->data, (uint32_t)value->Aad->size) : cmp_write_nil(ctx);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmp_write_uinteger(ctx, value->MacLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int Ckp_CkCcmParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj_ptr, Ckp_CkCcmParams* value)
{
  if (ctx == NULL || value == NULL) return NMRPC_BAD_ARGUMENT;
  int result = 0;
  cmp_object_t start_obj;
  cmp_object_t tmp_obj;
  uint32_t array_size;

   USE_VARIABLE(tmp_obj);
  if (start_obj_ptr == NULL)
  {
    result = cmp_read_object(ctx, &start_obj);
    if (!result){ NMRPC_LOG_ERR_TEXT("Can not read token."); return NMRPC_DESERIALIZE_ERR; }
    start_obj_ptr = &start_obj;
  }

  result = cmp_object_as_array(start_obj_ptr, &array_size);
  if (!result || array_size != 4) { NMRPC_LOG_ERR_TEXT("Incorect field count."); return NMRPC_DESERIALIZE_ERR; }

  result = cmp_read_uint(ctx, &value->DataLen);
   if (!result) return NMRPC_FATAL_ERROR;

  result = cmph_read_nullable_binary(ctx, &value->Nonce);
   if (result != NMRPC_OK) return result;

  result = cmph_read_nullable_binary(ctx, &value->Aad);
   if (result != NMRPC_OK) return result;

  result = cmp_read_uint(ctx, &value->MacLen);
   if (!result) return NMRPC_FATAL_ERROR;

    return NMRPC_OK;
}

int Ckp_CkCcmParams_Release(Ckp_CkCcmParams* value)
{
     if (value == NULL) return NMRPC_BAD_ARGUMENT;

  if (value->Nonce != NULL)
  {
      Binary_Release(value->Nonce);
      free((void*)value->Nonce);
      value->Nonce = NULL;
  }
  if (value->Aad != NULL)
  {
      Binary_Release(value->Aad);
      free((void*)value->Aad);
      value->Aad = NULL;
  }
    return NMRPC_OK;
}
int nmrpc_call_Ping(nmrpc_global_context_t* ctx, PingRequest* request, PingEnvelope* response)
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


    memset((void*) response, 0, sizeof(PingEnvelope));

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
    cmp_write_str(&write_head_ctx, "Ping", 4);

    result = PingRequest_Serialize(&write_body_ctx, request);
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

    result = PingEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Initialize(nmrpc_global_context_t* ctx, InitializeRequest* request, InitializeEnvelope* response)
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


    memset((void*) response, 0, sizeof(InitializeEnvelope));

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
    cmp_write_str(&write_head_ctx, "Initialize", 10);

    result = InitializeRequest_Serialize(&write_body_ctx, request);
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

    result = InitializeEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Finalize(nmrpc_global_context_t* ctx, FinalizeRequest* request, FinalizeEnvelope* response)
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


    memset((void*) response, 0, sizeof(FinalizeEnvelope));

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
    cmp_write_str(&write_head_ctx, "Finalize", 8);

    result = FinalizeRequest_Serialize(&write_body_ctx, request);
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

    result = FinalizeEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetInfo(nmrpc_global_context_t* ctx, GetInfoRequest* request, GetInfoEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetInfoEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetInfo", 7);

    result = GetInfoRequest_Serialize(&write_body_ctx, request);
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

    result = GetInfoEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetSlotList(nmrpc_global_context_t* ctx, GetSlotListRequest* request, GetSlotListEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetSlotListEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetSlotList", 11);

    result = GetSlotListRequest_Serialize(&write_body_ctx, request);
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

    result = GetSlotListEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetSlotInfo(nmrpc_global_context_t* ctx, GetSlotInfoRequest* request, GetSlotInfoEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetSlotInfoEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetSlotInfo", 11);

    result = GetSlotInfoRequest_Serialize(&write_body_ctx, request);
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

    result = GetSlotInfoEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetTokenInfo(nmrpc_global_context_t* ctx, GetTokenInfoRequest* request, GetTokenInfoEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetTokenInfoEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetTokenInfo", 12);

    result = GetTokenInfoRequest_Serialize(&write_body_ctx, request);
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

    result = GetTokenInfoEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetMechanismList(nmrpc_global_context_t* ctx, GetMechanismListRequest* request, GetMechanismListEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetMechanismListEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetMechanismList", 16);

    result = GetMechanismListRequest_Serialize(&write_body_ctx, request);
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

    result = GetMechanismListEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetMechanismInfo(nmrpc_global_context_t* ctx, GetMechanismInfoRequest* request, GetMechanismInfoEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetMechanismInfoEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetMechanismInfo", 16);

    result = GetMechanismInfoRequest_Serialize(&write_body_ctx, request);
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

    result = GetMechanismInfoEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_OpenSession(nmrpc_global_context_t* ctx, OpenSessionRequest* request, OpenSessionEnvelope* response)
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


    memset((void*) response, 0, sizeof(OpenSessionEnvelope));

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
    cmp_write_str(&write_head_ctx, "OpenSession", 11);

    result = OpenSessionRequest_Serialize(&write_body_ctx, request);
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

    result = OpenSessionEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_CloseSession(nmrpc_global_context_t* ctx, CloseSessionRequest* request, CloseSessionEnvelope* response)
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


    memset((void*) response, 0, sizeof(CloseSessionEnvelope));

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
    cmp_write_str(&write_head_ctx, "CloseSession", 12);

    result = CloseSessionRequest_Serialize(&write_body_ctx, request);
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

    result = CloseSessionEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_CloseAllSessions(nmrpc_global_context_t* ctx, CloseAllSessionsRequest* request, CloseAllSessionsEnvelope* response)
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


    memset((void*) response, 0, sizeof(CloseAllSessionsEnvelope));

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
    cmp_write_str(&write_head_ctx, "CloseAllSessions", 16);

    result = CloseAllSessionsRequest_Serialize(&write_body_ctx, request);
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

    result = CloseAllSessionsEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetSessionInfo(nmrpc_global_context_t* ctx, GetSessionInfoRequest* request, GetSessionInfoEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetSessionInfoEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetSessionInfo", 14);

    result = GetSessionInfoRequest_Serialize(&write_body_ctx, request);
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

    result = GetSessionInfoEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Login(nmrpc_global_context_t* ctx, LoginRequest* request, LoginEnvelope* response)
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


    memset((void*) response, 0, sizeof(LoginEnvelope));

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
    cmp_write_str(&write_head_ctx, "Login", 5);

    result = LoginRequest_Serialize(&write_body_ctx, request);
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

    result = LoginEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Logout(nmrpc_global_context_t* ctx, LogoutRequest* request, LogoutEnvelope* response)
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


    memset((void*) response, 0, sizeof(LogoutEnvelope));

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
    cmp_write_str(&write_head_ctx, "Logout", 6);

    result = LogoutRequest_Serialize(&write_body_ctx, request);
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

    result = LogoutEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_SeedRandom(nmrpc_global_context_t* ctx, SeedRandomRequest* request, SeedRandomEnvelope* response)
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


    memset((void*) response, 0, sizeof(SeedRandomEnvelope));

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
    cmp_write_str(&write_head_ctx, "SeedRandom", 10);

    result = SeedRandomRequest_Serialize(&write_body_ctx, request);
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

    result = SeedRandomEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GenerateRandom(nmrpc_global_context_t* ctx, GenerateRandomRequest* request, GenerateRandomEnvelope* response)
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


    memset((void*) response, 0, sizeof(GenerateRandomEnvelope));

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
    cmp_write_str(&write_head_ctx, "GenerateRandom", 14);

    result = GenerateRandomRequest_Serialize(&write_body_ctx, request);
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

    result = GenerateRandomEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DigestInit(nmrpc_global_context_t* ctx, DigestInitRequest* request, DigestInitEnvelope* response)
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


    memset((void*) response, 0, sizeof(DigestInitEnvelope));

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
    cmp_write_str(&write_head_ctx, "DigestInit", 10);

    result = DigestInitRequest_Serialize(&write_body_ctx, request);
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

    result = DigestInitEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Digest(nmrpc_global_context_t* ctx, DigestRequest* request, DigestEnvelope* response)
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


    memset((void*) response, 0, sizeof(DigestEnvelope));

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
    cmp_write_str(&write_head_ctx, "Digest", 6);

    result = DigestRequest_Serialize(&write_body_ctx, request);
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

    result = DigestEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DigestUpdate(nmrpc_global_context_t* ctx, DigestUpdateRequest* request, DigestUpdateEnvelope* response)
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


    memset((void*) response, 0, sizeof(DigestUpdateEnvelope));

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
    cmp_write_str(&write_head_ctx, "DigestUpdate", 12);

    result = DigestUpdateRequest_Serialize(&write_body_ctx, request);
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

    result = DigestUpdateEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DigestKey(nmrpc_global_context_t* ctx, DigestKeyRequest* request, DigestKeyEnvelope* response)
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


    memset((void*) response, 0, sizeof(DigestKeyEnvelope));

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
    cmp_write_str(&write_head_ctx, "DigestKey", 9);

    result = DigestKeyRequest_Serialize(&write_body_ctx, request);
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

    result = DigestKeyEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DigestFinal(nmrpc_global_context_t* ctx, DigestFinalRequest* request, DigestFinalEnvelope* response)
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


    memset((void*) response, 0, sizeof(DigestFinalEnvelope));

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
    cmp_write_str(&write_head_ctx, "DigestFinal", 11);

    result = DigestFinalRequest_Serialize(&write_body_ctx, request);
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

    result = DigestFinalEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_CreateObject(nmrpc_global_context_t* ctx, CreateObjectRequest* request, CreateObjectEnvelope* response)
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


    memset((void*) response, 0, sizeof(CreateObjectEnvelope));

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
    cmp_write_str(&write_head_ctx, "CreateObject", 12);

    result = CreateObjectRequest_Serialize(&write_body_ctx, request);
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

    result = CreateObjectEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DestroyObject(nmrpc_global_context_t* ctx, DestroyObjectRequest* request, DestroyObjectEnvelope* response)
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


    memset((void*) response, 0, sizeof(DestroyObjectEnvelope));

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
    cmp_write_str(&write_head_ctx, "DestroyObject", 13);

    result = DestroyObjectRequest_Serialize(&write_body_ctx, request);
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

    result = DestroyObjectEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_FindObjectsInit(nmrpc_global_context_t* ctx, FindObjectsInitRequest* request, FindObjectsInitEnvelope* response)
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


    memset((void*) response, 0, sizeof(FindObjectsInitEnvelope));

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
    cmp_write_str(&write_head_ctx, "FindObjectsInit", 15);

    result = FindObjectsInitRequest_Serialize(&write_body_ctx, request);
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

    result = FindObjectsInitEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_FindObjects(nmrpc_global_context_t* ctx, FindObjectsRequest* request, FindObjectsEnvelope* response)
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


    memset((void*) response, 0, sizeof(FindObjectsEnvelope));

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
    cmp_write_str(&write_head_ctx, "FindObjects", 11);

    result = FindObjectsRequest_Serialize(&write_body_ctx, request);
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

    result = FindObjectsEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_FindObjectsFinal(nmrpc_global_context_t* ctx, FindObjectsFinalRequest* request, FindObjectsFinalEnvelope* response)
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


    memset((void*) response, 0, sizeof(FindObjectsFinalEnvelope));

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
    cmp_write_str(&write_head_ctx, "FindObjectsFinal", 16);

    result = FindObjectsFinalRequest_Serialize(&write_body_ctx, request);
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

    result = FindObjectsFinalEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetObjectSize(nmrpc_global_context_t* ctx, GetObjectSizeRequest* request, GetObjectSizeEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetObjectSizeEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetObjectSize", 13);

    result = GetObjectSizeRequest_Serialize(&write_body_ctx, request);
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

    result = GetObjectSizeEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GetAttributeValue(nmrpc_global_context_t* ctx, GetAttributeValueRequest* request, GetAttributeValueEnvelope* response)
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


    memset((void*) response, 0, sizeof(GetAttributeValueEnvelope));

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
    cmp_write_str(&write_head_ctx, "GetAttributeValue", 17);

    result = GetAttributeValueRequest_Serialize(&write_body_ctx, request);
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

    result = GetAttributeValueEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GenerateKeyPair(nmrpc_global_context_t* ctx, GenerateKeyPairRequest* request, GenerateKeyPairEnvelope* response)
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


    memset((void*) response, 0, sizeof(GenerateKeyPairEnvelope));

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
    cmp_write_str(&write_head_ctx, "GenerateKeyPair", 15);

    result = GenerateKeyPairRequest_Serialize(&write_body_ctx, request);
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

    result = GenerateKeyPairEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_SignInit(nmrpc_global_context_t* ctx, SignInitRequest* request, SignInitEnvelope* response)
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


    memset((void*) response, 0, sizeof(SignInitEnvelope));

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
    cmp_write_str(&write_head_ctx, "SignInit", 8);

    result = SignInitRequest_Serialize(&write_body_ctx, request);
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

    result = SignInitEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Sign(nmrpc_global_context_t* ctx, SignRequest* request, SignEnvelope* response)
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


    memset((void*) response, 0, sizeof(SignEnvelope));

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
    cmp_write_str(&write_head_ctx, "Sign", 4);

    result = SignRequest_Serialize(&write_body_ctx, request);
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

    result = SignEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_SignUpdate(nmrpc_global_context_t* ctx, SignUpdateRequest* request, SignUpdateEnvelope* response)
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


    memset((void*) response, 0, sizeof(SignUpdateEnvelope));

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
    cmp_write_str(&write_head_ctx, "SignUpdate", 10);

    result = SignUpdateRequest_Serialize(&write_body_ctx, request);
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

    result = SignUpdateEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_SignFinal(nmrpc_global_context_t* ctx, SignFinalRequest* request, SignFinalEnvelope* response)
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


    memset((void*) response, 0, sizeof(SignFinalEnvelope));

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
    cmp_write_str(&write_head_ctx, "SignFinal", 9);

    result = SignFinalRequest_Serialize(&write_body_ctx, request);
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

    result = SignFinalEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_VerifyInit(nmrpc_global_context_t* ctx, VerifyInitRequest* request, VerifyInitEnvelope* response)
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


    memset((void*) response, 0, sizeof(VerifyInitEnvelope));

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
    cmp_write_str(&write_head_ctx, "VerifyInit", 10);

    result = VerifyInitRequest_Serialize(&write_body_ctx, request);
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

    result = VerifyInitEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Verify(nmrpc_global_context_t* ctx, VerifyRequest* request, VerifyEnvelope* response)
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


    memset((void*) response, 0, sizeof(VerifyEnvelope));

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
    cmp_write_str(&write_head_ctx, "Verify", 6);

    result = VerifyRequest_Serialize(&write_body_ctx, request);
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

    result = VerifyEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_VerifyUpdate(nmrpc_global_context_t* ctx, VerifyUpdateRequest* request, VerifyUpdateEnvelope* response)
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


    memset((void*) response, 0, sizeof(VerifyUpdateEnvelope));

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
    cmp_write_str(&write_head_ctx, "VerifyUpdate", 12);

    result = VerifyUpdateRequest_Serialize(&write_body_ctx, request);
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

    result = VerifyUpdateEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_VerifyFinal(nmrpc_global_context_t* ctx, VerifyFinalRequest* request, VerifyFinalEnvelope* response)
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


    memset((void*) response, 0, sizeof(VerifyFinalEnvelope));

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
    cmp_write_str(&write_head_ctx, "VerifyFinal", 11);

    result = VerifyFinalRequest_Serialize(&write_body_ctx, request);
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

    result = VerifyFinalEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_GenerateKey(nmrpc_global_context_t* ctx, GenerateKeyRequest* request, GenerateKeyEnvelope* response)
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


    memset((void*) response, 0, sizeof(GenerateKeyEnvelope));

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
    cmp_write_str(&write_head_ctx, "GenerateKey", 11);

    result = GenerateKeyRequest_Serialize(&write_body_ctx, request);
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

    result = GenerateKeyEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DeriveKey(nmrpc_global_context_t* ctx, DeriveKeyRequest* request, DeriveKeyEnvelope* response)
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


    memset((void*) response, 0, sizeof(DeriveKeyEnvelope));

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
    cmp_write_str(&write_head_ctx, "DeriveKey", 9);

    result = DeriveKeyRequest_Serialize(&write_body_ctx, request);
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

    result = DeriveKeyEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_EncryptInit(nmrpc_global_context_t* ctx, EncryptInitRequest* request, EncryptInitEnvelope* response)
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


    memset((void*) response, 0, sizeof(EncryptInitEnvelope));

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
    cmp_write_str(&write_head_ctx, "EncryptInit", 11);

    result = EncryptInitRequest_Serialize(&write_body_ctx, request);
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

    result = EncryptInitEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Encrypt(nmrpc_global_context_t* ctx, EncryptRequest* request, EncryptEnvelope* response)
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


    memset((void*) response, 0, sizeof(EncryptEnvelope));

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
    cmp_write_str(&write_head_ctx, "Encrypt", 7);

    result = EncryptRequest_Serialize(&write_body_ctx, request);
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

    result = EncryptEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_EncryptUpdate(nmrpc_global_context_t* ctx, EncryptUpdateRequest* request, EncryptUpdateEnvelope* response)
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


    memset((void*) response, 0, sizeof(EncryptUpdateEnvelope));

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
    cmp_write_str(&write_head_ctx, "EncryptUpdate", 13);

    result = EncryptUpdateRequest_Serialize(&write_body_ctx, request);
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

    result = EncryptUpdateEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_EncryptFinal(nmrpc_global_context_t* ctx, EncryptFinalRequest* request, EncryptFinalEnvelope* response)
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


    memset((void*) response, 0, sizeof(EncryptFinalEnvelope));

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
    cmp_write_str(&write_head_ctx, "EncryptFinal", 12);

    result = EncryptFinalRequest_Serialize(&write_body_ctx, request);
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

    result = EncryptFinalEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DecryptInit(nmrpc_global_context_t* ctx, DecryptInitRequest* request, DecryptInitEnvelope* response)
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


    memset((void*) response, 0, sizeof(DecryptInitEnvelope));

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
    cmp_write_str(&write_head_ctx, "DecryptInit", 11);

    result = DecryptInitRequest_Serialize(&write_body_ctx, request);
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

    result = DecryptInitEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_Decrypt(nmrpc_global_context_t* ctx, DecryptRequest* request, DecryptEnvelope* response)
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


    memset((void*) response, 0, sizeof(DecryptEnvelope));

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
    cmp_write_str(&write_head_ctx, "Decrypt", 7);

    result = DecryptRequest_Serialize(&write_body_ctx, request);
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

    result = DecryptEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DecryptUpdate(nmrpc_global_context_t* ctx, DecryptUpdateRequest* request, DecryptUpdateEnvelope* response)
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


    memset((void*) response, 0, sizeof(DecryptUpdateEnvelope));

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
    cmp_write_str(&write_head_ctx, "DecryptUpdate", 13);

    result = DecryptUpdateRequest_Serialize(&write_body_ctx, request);
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

    result = DecryptUpdateEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

int nmrpc_call_DecryptFinal(nmrpc_global_context_t* ctx, DecryptFinalRequest* request, DecryptFinalEnvelope* response)
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


    memset((void*) response, 0, sizeof(DecryptFinalEnvelope));

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
    cmp_write_str(&write_head_ctx, "DecryptFinal", 12);

    result = DecryptFinalRequest_Serialize(&write_body_ctx, request);
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

    result = DecryptFinalEnvelope_Deserialize(&read_body_ctx, NULL, response);
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

