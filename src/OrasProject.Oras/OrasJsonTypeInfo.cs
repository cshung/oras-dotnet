// Copyright The ORAS Authors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text.Json.Serialization.Metadata;
using OrasProject.Oras.Oci;

namespace OrasProject.Oras;

/// <summary>
/// Provides source-generated <see cref="JsonTypeInfo{T}"/>
/// instances for public OCI model types, enabling
/// AOT-compatible JSON serialization without requiring
/// consumers to define their own
/// <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>.
/// </summary>
public static class OrasJsonTypeInfo
{
    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{T}"/> for
    /// <see cref="Manifest"/>.
    /// </summary>
    public static JsonTypeInfo<Manifest> Manifest =>
        OrasJsonSerializerContext.Default.Manifest;

    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{T}"/> for
    /// <see cref="Oci.Index"/>.
    /// </summary>
    public static JsonTypeInfo<Index> Index =>
        OrasJsonSerializerContext.Default.Index;

    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{T}"/> for
    /// <see cref="Descriptor"/>.
    /// </summary>
    public static JsonTypeInfo<Descriptor> Descriptor =>
        OrasJsonSerializerContext.Default.Descriptor;

    /// <summary>
    /// Gets the <see cref="JsonTypeInfo{T}"/> for
    /// <see cref="Oci.Platform"/>.
    /// </summary>
    public static JsonTypeInfo<Platform> Platform =>
        OrasJsonSerializerContext.Default.Platform;
}
