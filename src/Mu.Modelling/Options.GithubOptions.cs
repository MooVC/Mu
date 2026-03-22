namespace Mu.Modelling;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

public partial record Options
{
    public sealed record GithubOptions(
        string ApiBaseAddress,
        JsonSerializerOptions Json,
        string Owner,
        string Repository,
        string Reference,
        string Token)
    {
        [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "Inner default is qualified.")]
        public static readonly GithubOptions Default = new(
            DefaultApiBaseAddress,
            new()
            {
                PropertyNameCaseInsensitive = true,
            },
            DefaultOwner,
            DefaultRepository,
            DefaultReference,
            string.Empty);

        private const string DefaultApiBaseAddress = "https://api.github.com/";
        private const string DefaultOwner = "Mu";
        private const string DefaultRepository = "Mu.Template";
        private const string DefaultReference = "master";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Owner) && !string.IsNullOrWhiteSpace(Repository);

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