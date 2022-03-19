// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Primitives;

namespace Ferrite.Transport;

/// <summary>
/// Represents HttpRequest and HttpResponse headers
/// </summary>
public partial interface IHeaderDictionary : IDictionary<string, StringValues>
{
    /// <summary>
    /// IHeaderDictionary has a different indexer contract than IDictionary, where it will return StringValues.Empty for missing entries.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The stored value, or StringValues.Empty if the key is not present.</returns>
    new StringValues this[string key] { get; set; }
}