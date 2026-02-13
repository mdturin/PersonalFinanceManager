using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Application.DTOs;

public class MetricModel
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string? Trend { get; set; } // "positive", "negative", "neutral"
    public string? Helper { get; set; }
}
