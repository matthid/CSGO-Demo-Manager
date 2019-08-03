namespace Thoth.Json.Net

module Util =
    open System
    open System.Collections.Concurrent

    type Cache<'Key, 'Value>() =
        let cache = ConcurrentDictionary<'Key, 'Value>()
        member __.GetOrAdd(key: 'Key, factory: 'Key->'Value) =
            cache.GetOrAdd(key, factory)

    let CachedEncoders = lazy Cache<Type, BoxedEncoder>()
    let CachedDecoders = lazy Cache<Type, BoxedDecoder>()