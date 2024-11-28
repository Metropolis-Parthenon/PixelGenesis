using System.Collections;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace PixelGenesis.ECS;

public class SerializableObjectYamlConverter(
    AssetManager assetManager, 
    IReadOnlyDictionary<string, Func<int, ISerializableObject>> factories,
    string newAssetsRelativePath) : IYamlTypeConverter
{
    const string AssetTag = "!PGAsset";

    public bool Accepts(Type type) => type.IsAssignableTo(typeof(ISerializableObject));
    
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var mappingStart = parser.Consume<MappingStart>();
        int ownerIndex = -1;
        
        var key = parser.Consume<Scalar>().Value;
        if(key is not "__type")
        {
            throw new InvalidDataException("the first property must be __type");
        }
        var typeName = parser.Consume<Scalar>().Value;

        if(parser.Current is Scalar scalar && scalar.Value is "__ownerIndex")
        {
            parser.MoveNext();
            ownerIndex = int.Parse(parser.Consume<Scalar>().Value);
        }

        var result = factories[typeName](ownerIndex);

        result.SetSerializableValues(EnumerateObjectValues(result, parser, rootDeserializer));

        return result;
    }
        
    IEnumerable<KeyValuePair<string, object?>> EnumerateObjectValues(ISerializableObject obj, IParser parser, ObjectDeserializer rootDeserializer)
    {
        while(!parser.Accept<MappingEnd>(out _))
        {
            var key = parser.Consume<Scalar>().Value;

            var value = parser.Current;

            if (value is Scalar scalar && scalar.Tag == AssetTag) 
            {
                var assetId = Guid.Parse(scalar.Value);
                var asset = assetManager.LoadAsset(assetId);
                yield return new KeyValuePair<string, object>(key, asset);

                parser.MoveNext();
                continue;
            }

            //if (value is MappingStart mappingStart && mappingStart.Tag != TagName.Empty)
            //{ 
            //    var tag = mappingStart.Tag;
            //    if(factories.ContainsKey(tag.Value))
            //    {
            //        var serObj = rootDeserializer(typeof(ISerializableObject));
            //        yield return new KeyValuePair<string, object?>(key, serObj);
            //        continue;
            //    }
            //}

            //if(value is SequenceStart)
            //{
            //    var seq = ParseSquence(parser, obj.GetPropType(key), rootDeserializer);
            //    yield return new KeyValuePair<string, object?>(key, seq);
            //    continue;
            //}

            var propType = obj.GetPropType(key);
            yield return new KeyValuePair<string, object?>(key, rootDeserializer(propType));
        }

        parser.MoveNext();
    }

    //IEnumerable ParseSquence(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    //{
    //    parser.Consume<SequenceStart>();

    //    Type itemsType;

    //    if (type.IsGenericType && type.Name.Contains("IEnumerable"))
    //    {
    //        itemsType = type.GenericTypeArguments.First();
    //    }
    //    else
    //    {
    //        itemsType = type.GetInterfaces().First(x => x.IsGenericType && x.Name.Contains("IEnumerable")).GenericTypeArguments.First();
    //    }        

    //    List<object?> seq = new();

    //    while(!parser.Accept<SequenceEnd>(out _))
    //    {
    //        seq.Add(rootDeserializer(itemsType));
    //    }

    //    parser.MoveNext();

    //    return seq;
    //}

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var serializableObject = Unsafe.As<ISerializableObject>(value);
        if(serializableObject is null)
        {
            throw new InvalidOperationException("Attempted to serialize an object that does not implement ISearializableObject");
        }

        WriteObject(serializableObject, emitter, serializer);
    }

    void WriteObject(ISerializableObject obj, IEmitter emitter, ObjectSerializer serializer)
    {
        var ownerIndex = obj.GetOwnerIndex();

        emitter.Emit(new MappingStart(AnchorName.Empty, TagName.Empty, true, MappingStyle.Block));

        emitter.Emit(new Scalar("__type"));
        emitter.Emit(new Scalar(obj.GetType().FullName ?? obj.GetType().Name));

        if (ownerIndex > -1) 
        {
            emitter.Emit(new Scalar("__ownerIndex"));
            emitter.Emit(new Scalar(ownerIndex.ToString()));
        }

        foreach (var (key, val) in obj.GetSerializableValues())
        {
            emitter.Emit(new Scalar(key));

            //if(val is IEnumerable arr and not string)
            //{
            //    WriteArray(arr, emitter, serializer);
            //    continue;
            //}

            //if(val is ISerializableObject childObj)
            //{
            //    WriteObject(childObj, emitter, serializer);
            //    continue;
            //}

            if(val is IAsset asset)
            {
                assetManager.SaveOrCreateInPath(asset, asset.Name);
                emitter.Emit(new Scalar(AnchorName.Empty, AssetTag, Path.Combine(newAssetsRelativePath, asset.Id.ToString()), ScalarStyle.Any, false, false));
                continue;
            }

            serializer(val);
        }

        emitter.Emit(new MappingEnd());
    }

    //void WriteArray(IEnumerable values, IEmitter emitter, ObjectSerializer serializer)
    //{
    //    emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));

    //    foreach (var val in values) 
    //    {
    //        if (val is IEnumerable arr and not string)
    //        {
    //            WriteArray(arr, emitter, serializer);
    //            continue;
    //        }

    //        if (val is ISerializableObject childObj)
    //        {
    //            WriteObject(childObj, emitter, serializer);
    //            continue;
    //        }

    //        if (val is IAsset asset)
    //        {
    //            emitter.Emit(new Scalar(AssetTag, asset.Id.ToString()));
    //            continue;
    //        }

    //        serializer(val);
    //    }

    //    emitter.Emit(new SequenceEnd());
    //}

}
