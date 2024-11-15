#if FLEXUS_SERIALIZATION
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flexus.Serialization;
using TriInspector;
using TriInspector.Processors;
using TriInspector.Utilities;
using UnityEngine;

[assembly: RegisterTriTypeProcessor(typeof(FlexusSerializeTriFieldTypeProcessor), 0)]

namespace TriInspector.Processors 
{
    public class FlexusSerializeTriFieldTypeProcessor : TriTypeProcessor
    {
        public override void ProcessType(Type type, List<TriPropertyDefinition> properties)
        {
            const int fieldsOffset = 1;

            properties.AddRange(TriReflectionUtilities
                .GetAllInstanceFieldsInDeclarationOrder(type)
                .Where(IsSerialized)
                .Select((it, ind) => TriPropertyDefinition.CreateForFieldInfo(ind + fieldsOffset, it)));
        }

        private static bool IsSerialized(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<SerializeReference>() == null 
                   && fieldInfo.GetCustomAttribute<SerializeField>() == null 
                   && fieldInfo.GetCustomAttribute<SerializationIncludedAttribute>(false) != null;
        }
    }
}
#endif