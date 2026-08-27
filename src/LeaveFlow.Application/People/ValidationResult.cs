namespace LeaveFlow.Application.People;

public sealed class ValidationResult
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    public bool IsValid => _errors.Count == 0;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors =>
        _errors.ToDictionary(error => error.Key, error => (IReadOnlyList<string>)error.Value);

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
    }
}
