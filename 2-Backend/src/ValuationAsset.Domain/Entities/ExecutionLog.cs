using System;

namespace ValuationAsset.Domain.Entities;

public class ExecutionLog
{
    public int LogId { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string ProcessStatus { get; set; } = string.Empty;
    public string? LogMessage { get; set; }
    public int RecordsAffected { get; set; }
}