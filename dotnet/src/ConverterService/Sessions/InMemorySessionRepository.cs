using System.Collections.Concurrent;

namespace ConverterService.Sessions
{
    /// <summary>
    /// An in-memory implementation of stateful session repository.
    /// </summary>
    public class InMemorySessionRepository : ISessionRepository
    {
        #region Private Members

        private readonly ConcurrentDictionary<Guid, ConversionSession> _sessions = new();
        private Func<SessionEventArgs, Task> _sessionCreated = default!;
        private Func<SessionEventArgs, Task> _sessionChanged = default!;
        private Func<SessionEventArgs, Task> _sessionRemoved = default!;

        #endregion

        #region Public Members

        /// <inheritdoc/>
        public void AddSession(ConversionSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (_sessions.ContainsKey(session.Id))
            {
                throw new InvalidOperationException($"Session with Id {session.Id} already exists in repository");
            }

            _sessions[session.Id] = session;
            OnSessionCreated(new SessionEventArgs() { Session = session });
        }

        /// <inheritdoc/>
        public ConversionSession GetSession(Guid id)
        {
            if (_sessions.ContainsKey(id))
            {
                return _sessions[id];
            }
            else
            {
                return null!;
            }
        }

        /// <inheritdoc/>
        public void SetSession(ConversionSession session)
        {
            ArgumentNullException.ThrowIfNull(session, nameof(session));            
            _sessions[session.Id] = session;
            OnSessionChanged(new SessionEventArgs() { Session = session});
        }

        /// <inheritdoc/>
        public void RemoveSession(Guid id)
        {
            if(!_sessions.ContainsKey(id))
            {
                return;
            }

            if(_sessions.TryRemove(id, out ConversionSession? session))
            {
                OnSessionRemoved(new SessionEventArgs() { Session = session });
            }
            else
            {
                throw new InvalidOperationException($"Session Id {id} not found");
            }
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public event Func<SessionEventArgs, Task> SessionCreated
        {
            add 
            {
                ArgumentNullException.ThrowIfNull(value, nameof(SessionCreated));
                _sessionCreated = value; 
            }
            remove 
            {
                ArgumentNullException.ThrowIfNull(value, nameof(SessionCreated));
                _sessionCreated = default!;
            } 
        }

        /// <inheritdoc/>
        public event Func<SessionEventArgs, Task> SessionChanged
        {
            add
            {
                ArgumentNullException.ThrowIfNull(value, nameof(SessionChanged));
                _sessionChanged = value;
            }
            remove
            {
                ArgumentNullException.ThrowIfNull(value, nameof(SessionChanged));
                _sessionChanged = default!;
            }
        }

        /// <inheritdoc/>
        public event Func<SessionEventArgs, Task> SessionRemoved
        {
            add
            {
                ArgumentNullException.ThrowIfNull(value, nameof(SessionRemoved));
                _sessionRemoved = value;
            }
            remove
            {
                ArgumentNullException.ThrowIfNull(value, nameof(SessionRemoved));
                _sessionRemoved = default!;
            }
        }

        #endregion

        #region Implementation

        private void OnSessionCreated(SessionEventArgs args)
        {
            if (_sessionCreated != null)
            {
                _ = _sessionCreated(args);
            }
        }

        private void OnSessionChanged(SessionEventArgs args)
        {
            if (_sessionChanged != null)
            {
                _ = _sessionChanged(args);
            }
        }

        private void OnSessionRemoved(SessionEventArgs args)
        {
            if (_sessionRemoved != null)
            {
                _ = _sessionRemoved(args);
            }
        }

        #endregion
    }
}
