namespace Mu.Communications.Tracing.Composition;

using System;
using System.Collections.Generic;
using System.Text;
using SimpleInjector;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        public Container RegisterScribe()
        {
            container.RegisterConditional<IScribe, Scribe>(Lifestyle.Singleton, context => !context.Handled);

            return container;
        }
    }
}