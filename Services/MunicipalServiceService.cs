using Supabase;
using DigitalInteraction.Helpers;
using DigitalInteraction.Models;

namespace DigitalInteraction.Services;

public class MunicipalServiceService(Client client)
{
    public async Task<List<ServiceCategory>> GetCategoriesAsync()
    {
        var result = await client
            .From<ServiceCategory>()
            .Order("id", Postgrest.Constants.Ordering.Ascending)
            .Get();
        return result.Models;
    }

    public async Task<List<MunicipalService>> GetServicesByCategoryAsync(int categoryId)
    {
        var result = await client
            .From<MunicipalService>()
            .Where(s => s.CategoryId == categoryId)
            .Order("name", Postgrest.Constants.Ordering.Ascending)
            .Get();

        // Фильтруем is_active на стороне клиента
        return result.Models.Where(s => s.IsActive).ToList();
    }

    public async Task<List<ServiceRequest>> GetMyRequestsAsync(int citizenId)
    {
        var result = await client
            .From<ServiceRequest>()
            .Where(r => r.CitizenId == citizenId)
            .Order("creation_date", Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<ServiceRequest> CreateRequestAsync(
        int citizenId, int serviceId, string? comment)
    {
        var request = new ServiceRequest
        {
            RequestNumber = NumberGenerator.GenerateServiceRequestNumber(),
            CitizenId = citizenId,
            ServiceId = serviceId,
            Comment = comment,
            CreationDate = DateTime.UtcNow,
            AppealStatusId = 1
        };
        var response = await client.From<ServiceRequest>().Insert(request);
        return response.Models.First();
    }

    public async Task<ServiceRequest?> GetRequestByIdAsync(int id)
    {
        return await client
            .From<ServiceRequest>()
            .Where(r => r.Id == id)
            .Single();
    }

    public async Task DeleteRequestAsync(int id)
    {
        await client.From<ServiceRequest>()
            .Where(r => r.Id == id)
            .Delete();
    }
}