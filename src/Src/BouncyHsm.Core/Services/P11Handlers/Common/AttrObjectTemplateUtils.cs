using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class AttrObjectTemplateUtils
{
    public static IReadOnlyDictionary<CKA, IAttributeValue> MergeTemplates(StorageObject storageObject,
        IReadOnlyDictionary<CKA, IAttributeValue> storageObjectTemplate,
        IReadOnlyDictionary<CKA, IAttributeValue> template,
        ILogger? logger = null)
    {
        if (storageObjectTemplate.Count == 0)
        {
            return template;
        }

        Dictionary<CKA, IAttributeValue> internalTemplate = new Dictionary<CKA, IAttributeValue>(template);
        List<CKA>? conflicts = null;

        foreach ((CKA attrType, IAttributeValue attrValue) in storageObjectTemplate)
        {
            if (internalTemplate.TryGetValue(attrType, out IAttributeValue? originalValue))
            {
                if (!attrValue.Equals(originalValue))
                {
                    if (conflicts == null)
                    {
                        conflicts = new List<CKA>();
                    }

                    conflicts.Add(attrType);
                }
            }
            else
            {
                internalTemplate.Add(attrType, attrValue);
                logger?.LogTrace("Add {AttributeType} attribute to template.", attrType);
            }
        }

        if (conflicts != null && conflicts.Count > 0)
        {
            string conflictString = string.Join(", ", conflicts);
            logger?.LogError("The specified template is inconsistent (problem attributes: {ConflictString}) with the template of object {StorageObject}.", conflictString, storageObject);
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                $"The specified template is inconsistent (problem attributes: {conflictString}) with the template of object {storageObject}.");
        }

        return internalTemplate;
    }
}
