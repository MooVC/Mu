namespace Muify
{
    using System.Collections.Generic;
    using MooVC.Syntax;

    internal abstract class AttributeStrategy
        : IPostInitializationStrategy
    {
        private readonly string _hint;

        private protected AttributeStrategy(string hint)
        {
            _hint = hint;
        }

        public IEnumerable<File> Apply()
        {
            Snippet content = GetContent();

            yield return new File(content, _hint);
        }

        protected abstract Snippet GetContent();
    }
}