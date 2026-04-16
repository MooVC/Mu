namespace Mu.Modelling
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;

    public sealed partial class Options
    {
        public sealed class GithubOptions
        {
            [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "Inner default is qualified.")]
            public static readonly GithubOptions Default = new GithubOptions(
                DefaultApiBaseAddress,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                },
                DefaultOwner,
                DefaultRepository,
                DefaultReference,
                string.Empty);

            private const string DefaultApiBaseAddress = "https://api.github.com/";
            private const string DefaultOwner = "Mu";
            private const string DefaultReference = "master";
            private const string DefaultRepository = "Mu.Template";

            public GithubOptions(string apiBaseAddress, JsonSerializerOptions json, string owner, string repository, string reference, string token)
            {
                ApiBaseAddress = apiBaseAddress;
                Json = json;
                Owner = owner;
                Repository = repository;
                Reference = reference;
                Token = token;
            }

            public string ApiBaseAddress { get; }

            public bool IsConfigured => !string.IsNullOrWhiteSpace(Owner) && !string.IsNullOrWhiteSpace(Repository);

            public JsonSerializerOptions Json { get; }

            public string Owner { get; }

            public string Reference { get; }

            public string Repository { get; }

            public string Token { get; }

            public string ContentsPath(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return $"repos/{Owner}/{Repository}/contents?ref={Reference}";
                }

                return $"repos/{Owner}/{Repository}/contents/{path}?ref={Reference}";
            }
        }
    }
}