namespace ConverterService.Sessions
{
    /// <summary>
    /// Defines a repository for storing state of conversion for each conversion session.
    /// </summary>
    public interface ISessionRepository
    {
        /// <summary>
        /// Gets an instance of <see cref="ConversionSession"/> given its Id.
        /// </summary>
        /// <param name="id">A unique identifier of the session.</param>
        /// <returns>An instance of <see cref="ConversionSession"/>.</returns>
        ConversionSession GetSession(Guid id);

        /// <summary>
        /// Saves an updated session to repository.
        /// </summary>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        void SetSession(ConversionSession session);

        /// <summary>
        /// Adds a new session to repository.
        /// </summary>
        /// <param name="session">An instance of <see cref="ConversionSession"/>.</param>
        void AddSession(ConversionSession session);

        /// <summary>
        /// Removes a session from repository.
        /// </summary>
        /// <param name="id">An instance of <see cref="ConversionSession"/>.</param>
        void RemoveSession(Guid id);

        /// <summary>
        /// A delegate that is invoked when a new <see cref="ConversionSession"/> is added to the repository.
        /// </summary>
        event Func<SessionEventArgs, Task> SessionCreated;

        /// <summary>
        /// A delegate that is invoked when an existing <see cref="ConversionSession"/> is saved to the repository.
        /// </summary>
        event Func<SessionEventArgs, Task> SessionChanged;

        /// <summary>
        /// A delegate that is invoked when a <see cref="ConversionSession"/> is removed from the repository.
        /// </summary>
        event Func<SessionEventArgs, Task> SessionRemoved;
    }
}
