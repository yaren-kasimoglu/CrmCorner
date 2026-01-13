public class FinanceDashboardVm
{
    public int Year { get; set; }

    public int SelectedMonth { get; set; }
    public List<int> AvailableYears { get; set; } = new();
    public List<int> AvailableMonths { get; set; } = new();

    public decimal TotalExpectedNet { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }

    public decimal MonthExpectedNet { get; set; }
    public decimal MonthPaid { get; set; }
    public decimal MonthRemaining { get; set; }

    public List<string> MonthLabels { get; set; } = new();
    public List<decimal> MonthlyExpectedNet { get; set; } = new();
    public List<decimal> MonthlyPaid { get; set; } = new();

    // ✅ Seçili Ay donut
    public List<string> StatusLabels { get; set; } = new();
    public List<int> StatusCounts { get; set; } = new();

    // ✅ Seçili Yıl donut (yeni)
    public List<string> YearStatusLabels { get; set; } = new();
    public List<int> YearStatusCounts { get; set; } = new();
}
