namespace Application.Tests.Common;

/// <summary>
/// Дозволяє використовувати фіксований час у unit-тестах.
/// </summary>
public sealed class TestTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public TestTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}