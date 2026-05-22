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

using System.Text.Json;
using OrasProject.Oras;
using OrasProject.Oras.Content;
using OrasProject.Oras.Oci;

// Test 1: Manifest round-trip through MemoryStore
var store = new MemoryStore();

var configBytes = "{}"u8.ToArray();
var configDesc = Descriptor.Create(
    configBytes, "application/vnd.oci.image.config.v1+json");
await store.PushAsync(
    configDesc, new MemoryStream(configBytes)).ConfigureAwait(false);

var layerBytes = "hello world"u8.ToArray();
var layerDesc = Descriptor.Create(
    layerBytes, "application/octet-stream");
await store.PushAsync(
    layerDesc, new MemoryStream(layerBytes)).ConfigureAwait(false);

var manifest = new Manifest
{
    SchemaVersion = 2,
    MediaType = OrasProject.Oras.Oci.MediaType.ImageManifest,
    Config = configDesc,
    Layers = [layerDesc]
};
var manifestBytes = JsonSerializer.SerializeToUtf8Bytes(
    manifest, OrasJsonTypeInfo.Manifest);
var manifestDesc = Descriptor.Create(
    manifestBytes, OrasProject.Oras.Oci.MediaType.ImageManifest);
await store.PushAsync(
    manifestDesc, new MemoryStream(manifestBytes)).ConfigureAwait(false);

var fetched = await store.FetchAsync(manifestDesc).ConfigureAwait(false);
var fetchedBytes = new byte[manifestDesc.Size];
await fetched.ReadExactlyAsync(fetchedBytes).ConfigureAwait(false);
var roundTripped = JsonSerializer.Deserialize(
    fetchedBytes, OrasJsonTypeInfo.Manifest);
if (roundTripped?.SchemaVersion != 2
    || roundTripped.Layers?.Count != 1)
{
    throw new Exception("Manifest round-trip failed");
}
Console.WriteLine("PASS: Manifest round-trip");

// Test 2: Successors traversal (internal AOT deserialization)
var successors = await store.GetSuccessorsAsync(
    manifestDesc).ConfigureAwait(false);
var successorCount = successors.Count();
if (successorCount != 2)
{
    throw new Exception(
        $"Expected 2 successors, got {successorCount}");
}
Console.WriteLine("PASS: Successors traversal");

// Test 3: Index round-trip through MemoryStore
var index = new OrasProject.Oras.Oci.Index
{
    SchemaVersion = 2,
    MediaType = OrasProject.Oras.Oci.MediaType.ImageIndex,
    Manifests = [manifestDesc]
};
var indexBytes = JsonSerializer.SerializeToUtf8Bytes(
    index, OrasJsonTypeInfo.Index);
var indexDesc = Descriptor.Create(
    indexBytes, OrasProject.Oras.Oci.MediaType.ImageIndex);
await store.PushAsync(
    indexDesc, new MemoryStream(indexBytes)).ConfigureAwait(false);

var fetchedIndex = await store.FetchAsync(
    indexDesc).ConfigureAwait(false);
var fetchedIndexBytes = new byte[indexDesc.Size];
await fetchedIndex.ReadExactlyAsync(
    fetchedIndexBytes).ConfigureAwait(false);
var roundTrippedIndex = JsonSerializer.Deserialize(
    fetchedIndexBytes, OrasJsonTypeInfo.Index);
if (roundTrippedIndex?.Manifests?.Count != 1)
{
    throw new Exception("Index round-trip failed");
}
Console.WriteLine("PASS: Index round-trip");

// Test 4: Index successors (internal AOT deserialization)
var indexSuccessors = await store.GetSuccessorsAsync(
    indexDesc).ConfigureAwait(false);
var indexSuccessorCount = indexSuccessors.Count();
if (indexSuccessorCount != 1)
{
    throw new Exception(
        $"Expected 1 index successor, got {indexSuccessorCount}");
}
Console.WriteLine("PASS: Index successors");

// Test 5: Standalone Descriptor serialization
var descBytes = JsonSerializer.SerializeToUtf8Bytes(
    layerDesc, OrasJsonTypeInfo.Descriptor);
var descRoundTripped = JsonSerializer.Deserialize(
    descBytes, OrasJsonTypeInfo.Descriptor);
if (descRoundTripped?.Digest != layerDesc.Digest
    || descRoundTripped.Size != layerDesc.Size)
{
    throw new Exception("Descriptor round-trip failed");
}
Console.WriteLine("PASS: Descriptor round-trip");

// Test 6: Pull manifest from a public registry (opt-in via env var)
if (Environment.GetEnvironmentVariable("ORAS_AOT_LIVE_TEST") == "1")
{
    var repo = new OrasProject.Oras.Registry.Remote.Repository(
        "mcr.microsoft.com/hello-world");
    var resolved = await repo.ResolveAsync(
        "latest").ConfigureAwait(false);
    if (string.IsNullOrEmpty(resolved.Digest)
        || resolved.Size <= 0)
    {
        throw new Exception("Registry resolve failed");
    }
    Console.WriteLine(
        $"Resolved: mediaType={resolved.MediaType}, "
        + $"size={resolved.Size}");

    var (fetchedDesc2, manifestStream) = await repo.FetchAsync(
        "latest").ConfigureAwait(false);
    using (manifestStream)
    {
        var manifestContent = new byte[fetchedDesc2.Size];
        await manifestStream.ReadExactlyAsync(
            manifestContent).ConfigureAwait(false);
        var pulledManifest = JsonSerializer.Deserialize(
            manifestContent, OrasJsonTypeInfo.Manifest);
        if (pulledManifest?.SchemaVersion != 2
            || pulledManifest.Config == null)
        {
            throw new Exception(
                "Registry pull: invalid manifest");
        }
        Console.WriteLine(
            $"PASS: Registry pull "
            + $"(config={pulledManifest.Config.MediaType}, "
            + $"layers={pulledManifest.Layers?.Count})");
    }
}
else
{
    Console.WriteLine("SKIP: Registry pull (set "
        + "ORAS_AOT_LIVE_TEST=1 to enable)");
}

Console.WriteLine("All AOT tests passed!");
return 0;
