using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PixelGenesis.ECS;

public sealed class PGStructYamlConverter : IYamlTypeConverter
{
    public readonly static PGStructYamlConverter Instance = new PGStructYamlConverter();

    public bool Accepts(Type type) => !type.IsPrimitive && !type.IsEnum && type.IsValueType && !type.IsAssignableTo(typeof(IEnumerable));

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var fields = type.GetFields();

        parser.Consume<MappingStart>();

        var instance = Activator.CreateInstance(type);

        while(!parser.Accept<MappingEnd>(out _))
        {
            var name = parser.Consume<Scalar>().Value;
            var field = fields.FirstOrDefault(f => f.Name == name);
            if(field is null)
            {
                continue;
            }
            
            field.SetValue(instance, rootDeserializer(field.FieldType));
        }

        parser.MoveNext();
        return instance;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var fields = type.GetFields();

        emitter.Emit(new MappingStart());
        foreach (var field in fields) 
        { 
            var fieldValue = field.GetValue(value);
            emitter.Emit(new Scalar(field.Name));
            serializer(fieldValue);
        }

        emitter.Emit(new MappingEnd());
    }
}
