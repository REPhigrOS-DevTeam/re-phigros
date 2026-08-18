using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MainCore.Data
{
    public class VersionModel
    {
        [JsonProperty("full_tag")] public string FullTag { get; set; }

        [JsonProperty("short_tag")] public string ShortTag { get; set; }

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("structured_version")]
        public SemanticVersionModel StructuredVersion { get; set; } = new SemanticVersionModel();

        [JsonProperty("bundle_version")] public string BundleVersion { get; set; }

        [JsonProperty("android_bundle_version_code")]
        public string AndroidBundleVersionCode { get; set; }

        [JsonProperty("apple_build_number")] public string AppleBuildNumber { get; set; }

        [JsonProperty("build_symbols")] public List<string> BuildSymbols { get; set; }

#if !NOTANOTE_USING_CI && UNITY_EDITOR
        [JsonProperty("git_tag_commit_time",
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset GitTagCommitTime { get; set; }

        [JsonProperty("current_commit_id",
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentCommitId { get; set; }

        [JsonProperty("workspace_is_dirty",
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore)]
        public bool WorkspaceIsDirty { get; set; }
#else
#endif
    }

    public class SemanticVersionModel
    {
        [JsonProperty("major")] public int Major { get; set; }
        [JsonProperty("minor")] public int Minor { get; set; }
        [JsonProperty("patch")] public int Patch { get; set; }
        [JsonProperty("prerelease")] public string Prerelease { get; set; }
        [JsonProperty("metadata")] public string Metadata { get; set; }
    }

    public static class SemanticVersionModelExtension
    {
        public static bool IsPrerelease(this SemanticVersionModel self)
        {
            return !string.IsNullOrWhiteSpace(self.Prerelease);
        }
    }
}
