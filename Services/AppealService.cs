using Supabase;
using DigitalInteraction.Helpers;
using DigitalInteraction.Models;

namespace DigitalInteraction.Services;

public class AppealService(Client client)
{
    public async Task<List<Appeal>> GetMyAppealsAsync(int citizenId)
    {
        var result = await client
            .From<Appeal>()
            .Where(a => a.CitizenId == citizenId)
            .Order("creation_date", Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<Appeal?> GetByIdAsync(int id)
    {
        return await client
            .From<Appeal>()
            .Where(a => a.Id == id)
            .Single();
    }

    public async Task<Appeal> CreateAsync(int citizenId, string title, string text)
    {
        var appeal = new Appeal
        {
            AppealNumber = NumberGenerator.GenerateAppealNumber(),
            Title = title,
            DocumentText = text,
            CitizenId = citizenId,
            CreationDate = DateTime.UtcNow,
            AppealStatusId = 1
        };
        var response = await client.From<Appeal>().Insert(appeal);
        return response.Models.First();
    }

    public async Task DeleteAsync(int appealId)
    {
        await client.From<Appeal>()
            .Where(a => a.Id == appealId)
            .Delete();
    }
}