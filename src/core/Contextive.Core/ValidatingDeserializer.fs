module Contextive.Core.ValidatingDeserializer

open YamlDotNet.Serialization
open YamlDotNet.Serialization.NodeDeserializers
open System.ComponentModel.DataAnnotations

type ValidatingNodeDeserializer(nodeDeserializer: INodeDeserializer) =
    let nodeDeserializer = nodeDeserializer

    interface INodeDeserializer with
        member this.Deserialize
            (
                reader: YamlDotNet.Core.IParser,
                expectedType: System.Type,
                nestedObjectDeserializer: System.Func<YamlDotNet.Core.IParser, System.Type, obj>,
                value: byref<obj>,
                rootDeserializer: ObjectDeserializer
            ) : bool =
            if
                nodeDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, &value, rootDeserializer)
            then
                let context = ValidationContext(value, null, null)
                Validator.ValidateObject(value, context, true)
                true
            else
                false

let factory f = ValidatingNodeDeserializer(f)

let where (s: ITrackingRegistrationLocationSelectionSyntax<INodeDeserializer>) = s.InsteadOf<ObjectNodeDeserializer>()
