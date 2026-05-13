using Supabase;
using DigitalInteraction.Helpers;
using DigitalInteraction.Models;

namespace DigitalInteraction.Services;

public class ApplicationService(Client client)
{
    public async Task<List<ServiceApplication>> GetMyApplicationsAsync(int citizenId)
    {
        var result = await client
            .From<ServiceApplication>()
            .Where(a => a.CitizenId == citizenId)
            .Order("creation_date", Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<ServiceApplication?> GetByIdAsync(int id)
    {
        return await client
            .From<ServiceApplication>()
            .Where(a => a.Id == id)
            .Single();
    }

    public async Task<ServiceApplication> CreateAsync(
        int citizenId, string title, string text, int? appealId = null)
    {
        var app = new ServiceApplication
        {
            ApplicationNumber = NumberGenerator.GenerateApplicationNumber(),
            Title = title,
            DocumentText = text,
            CitizenId = citizenId,
            CreationDate = DateTime.UtcNow,
            ApplicationStatusId = 1,
            AppealId = appealId
        };
        var response = await client.From<ServiceApplication>().Insert(app);
        return response.Models.First();
    }

    public async Task DeleteAsync(int applicationId)
    {
        await client.From<ServiceApplication>()
            .Where(a => a.Id == applicationId)
            .Delete();
    }
}