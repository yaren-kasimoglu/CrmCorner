using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace CrmCorner.Models;

public class Calendar
{
    public int Id { get; set; }


    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Date { get; set; } = null!;


    public string? UserId { get; set; }
    public virtual AppUser? AppUser { get; set; }

    [NotMapped]
    public EmailProperty EmailProperty { get; set; }
    [NotMapped]
    public List<string> SelectedEmails { get; set; } = new List<string>();



}
public class EmailProperty
{
    [NotMapped]
    public string Email { get; set; }


    [NotMapped]
    public DateTime StartDate { get; set; }
    [NotMapped]
    public DateTime EndDate { get; set; }
    // Diğer özellikler...

}

public class OutlookCalendarService
{
    private readonly string accessToken; // Outlook REST API'ye erişim için alınan erişim belirtecini içerir
    private readonly string calendarId; // Randevunun ekleneceği takvimin kimliği

    public OutlookCalendarService(string accessToken, string calendarId)
    {
        this.accessToken = accessToken;
        this.calendarId = calendarId;
    }

    public async Task AddAppointmentToOutlookAsync(string subject, DateTime startTime, DateTime endTime, string location)
    {
        string apiUrl = $"https://outlook.office.com/api/v2.0/me/calendars/{calendarId}/events";

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var requestBody = new
        {
            Subject = subject,
            Start = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            End = endTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Location = location
        };

        var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
        var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);
        response.EnsureSuccessStatusCode();
    }
}


