namespace LeaveFlow.Application.Abstractions.Data;

public interface IDataTransactionFactory
{
    Task<IDataTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
