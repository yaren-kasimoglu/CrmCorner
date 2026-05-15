using CrmCorner.Models;
namespace CrmCorner.ViewModels;

public class TeamDashboardVm
{
    public int TotalTasks { get; set; }
    public int Ongoing { get; set; }
    public int Won { get; set; }
    public int Lost { get; set; }

    public List<TeamDashboardUserVm> Users { get; set; }

    public Dictionary<string, decimal> TotalBudgetByCurrency { get; set; } = new();
    public Dictionary<string, decimal> WonBudgetByCurrency { get; set; } = new();
    public Dictionary<string, decimal> LostBudgetByCurrency { get; set; } = new();
}

public class TeamDashboardUserVm
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }

    public int Total { get; set; }
    public int Won { get; set; }
    public int Lost { get; set; }
    public int Ongoing { get; set; }

    public Dictionary<string, decimal> TotalBudgetByCurrency { get; set; } = new();
    public Dictionary<string, decimal> WonBudgetByCurrency { get; set; } = new();
    public Dictionary<string, decimal> LostBudgetByCurrency { get; set; } = new();


    public Dictionary<string, decimal> TotalBudgetContributionByCurrency { get; set; } = new();
    public Dictionary<string, decimal> WonBudgetContributionByCurrency { get; set; } = new();

    public Dictionary<string, decimal> TotalBudgetContributionRateByCurrency { get; set; } = new();
    public Dictionary<string, decimal> WonBudgetContributionRateByCurrency { get; set; } = new();
}


public class TeamUserDetailVm
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }

    public int Total { get; set; }
    public int Ongoing { get; set; }
    public int Won { get; set; }
    public int Lost { get; set; }

    public List<PipelineTask> Tasks { get; set; } = new List<PipelineTask>();
}

public class WeeklyMeetingReportViewModel
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int MeetingCount { get; set; }

    public string? Companies { get; set; }
}