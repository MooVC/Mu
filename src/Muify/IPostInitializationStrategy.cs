namespace Muify
{
    using System.Collections.Generic;

    internal interface IPostInitializationStrategy
    {
        IEnumerable<File> Apply();
    }
}