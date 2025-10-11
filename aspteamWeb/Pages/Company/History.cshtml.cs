using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

public class JobPostingHistoryModel : PageModel
{
    // Bindable query properties for search and filter
    [BindProperty(SupportsGet = true)]
    public string SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    public int TotalPages { get; set; }
    public int CurrentPage => Page;

    public List<Job> Jobs { get; set; } = new List<Job>();

    // Simulated data source (replace with your DB)
    private List<Job> AllJobs = new List<Job>
    {
        new Job { Id = 1, Title = "Software Engineer", Location = "NY", JobType = "Full-Time", Status = "Active", PostedDaysAgo = 3, ApplicantsCount = 10 },
        new Job { Id = 2, Title = "Project Manager", Location = "LA", JobType = "Contract", Status = "Closed", PostedDaysAgo = 10, ApplicantsCount = 5 },
        new Job { Id = 3, Title = "UI/UX Designer", Location = "Remote", JobType = "Part-Time", Status = "Draft", PostedDaysAgo = 1, ApplicantsCount = 0 },
        // Add more sample jobs here
    };

    private const int PageSize = 5;

    public void OnGet()
    {
        // Start with all jobs
        var query = AllJobs.AsQueryable();

        // Apply search
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            query = query.Where(j => j.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)
                                  || j.Location.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            query = query.Where(j => j.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Calculate total pages
        var totalItems = query.Count();
        TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

        // Apply pagination
        Jobs = query
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    // Handler for deleting a job
    public IActionResult OnPostDelete(int id)
    {
        var job = AllJobs.FirstOrDefault(j => j.Id == id);
        if (job != null)
        {
            AllJobs.Remove(job);
            TempData["SuccessMessage"] = $"Job '{job.Title}' has been deleted.";
        }

        return RedirectToPage();
    }
}

// Job model
public class Job
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Location { get; set; }
    public string JobType { get; set; }
    public string Status { get; set; } // Active, Closed, Draft
    public int PostedDaysAgo { get; set; }
    public int ApplicantsCount { get; set; }
}
