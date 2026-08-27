namespace LeaveFlow.Application.Abstractions.Data;

public interface IDataTransaction : IAsyncDisposable
{
    CancellationToken CancellationToken { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
}
