namespace Muify
{
    using Valuify;

    [Valuify]
    internal sealed partial class File
    {
        public File(string content, string hint)
        {
            Content = content;
            Hint = hint;
        }

        public string Content { get; private set; }

        public string Hint { get; private set; }
    }
}