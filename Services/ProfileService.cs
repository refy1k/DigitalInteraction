using Supabase;
using DigitalInteraction.Models;

namespace DigitalInteraction.Services;

public class ProfileService(Client client)
{
    public async Task<CitizenDocument?> GetDocumentsAsync(int citizenId) =>
        await client.From<CitizenDocument>()
            .Where(d => d.CitizenId == citizenId)
            .Single();

    public async Task SaveDocumentsAsync(CitizenDocument doc) =>
        await client.From<CitizenDocument>().Upsert(doc);

    public async Task<ContactInfo?> GetContactInfoAsync(int citizenId) =>
        await client.From<ContactInfo>()
            .Where(c => c.CitizenId == citizenId)
            .Single();

    public async Task SaveContactInfoAsync(ContactInfo info) =>
        await client.From<ContactInfo>().Upsert(info);
}