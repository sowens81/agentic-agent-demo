namespace Cloud.Infra.Orchestrator.Core.Models;

public sealed class AgentBehavior
{
    private readonly IReadOnlyDictionary<string, object> _values;

    public AgentBehavior(IReadOnlyDictionary<string, object> values)
    {
        _values = values;
    }

    public IReadOnlyDictionary<string, object> All => _values;

    // Typed accessors (safe defaults)
    public double Temperature => GetDouble("temperature", 0.2);
    public double TopP => GetDouble("top_p", 0.9);
    public int MaxTokens => GetInt("max_tokens", 1024);
    public double RepeatPenalty => GetDouble("repeat_penalty", 1.1);

    

    private double GetDouble(string key, double defaultValue)
        => _values.TryGetValue(key, out var v) && v is double d ? d : defaultValue;

    private int GetInt(string key, int defaultValue)
        => _values.TryGetValue(key, out var v) && v is int i ? i : defaultValue;
}
