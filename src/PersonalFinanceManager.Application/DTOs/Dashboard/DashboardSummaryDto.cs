using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceManager.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public List<MetricModel> Metrics { get; set; } = [];
}