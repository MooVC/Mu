namespace Mu.Auditing;

using System;

public partial class ScopeManager
{
    private sealed class Lease
        : IDisposable
    {
        private readonly State _state;
        private bool _disposed;

        public Lease(State state)
        {
            _state = state;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (ReferenceEquals(_current.Value, _state))
            {
                _current.Value = _state.Previous;
            }

            _disposed = true;
        }
    }
}