using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class PublicAiApiClient(HttpClient http)
{
    /// <summary>
    /// Sends the conversation history to the server and returns the AI reply.
    /// The last item in <paramref name="history"/> must be the user message to answer.
    /// Returns null when the server call fails or the AI returns no response.
    /// </summary>
    public async Task<string?> AskAsync(List<AiMessage> history)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync("/api/ai/ask", new AiAskRequest(history));
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        return await resp.Content.ReadFromJsonAsync<string>();
    }
}
