using System.Net;
using System.Net.Http.Json;
using NotasVozInteligentes.Client.Shared;

namespace NotasVozInteligentes.Client.Services;

public class ApiException(HttpStatusCode statusCode, string mensaje) : Exception(mensaje)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public class ApiClient(HttpClient http)
{
    // --- Notas ---

    public async Task<List<NotaVozDto>> ObtenerNotasAsync() =>
        await http.GetFromJsonAsync<List<NotaVozDto>>("api/notas") ?? [];

    public async Task<NotaVozDto> SubirNotaAsync(byte[] audio, string mimeType, double? duracionSegundos)
    {
        using var contenido = new MultipartFormDataContent();
        var audioContent = new ByteArrayContent(audio);
        audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        contenido.Add(audioContent, "audio", "nota" + ExtensionDe(mimeType));
        if (duracionSegundos is not null)
            contenido.Add(new StringContent(duracionSegundos.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "duracionSegundos");

        var respuesta = await http.PostAsync("api/notas", contenido);
        await AsegurarExito(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<NotaVozDto>())!;
    }

    public string UrlAudio(Guid notaId) => $"api/notas/{notaId}/audio";

    public async Task EliminarNotaAsync(Guid id) =>
        await AsegurarExito(await http.DeleteAsync($"api/notas/{id}"));

    public async Task<Guid> ConvertirAsync()
    {
        var respuesta = await http.PostAsync("api/notas/convertir", null);
        await AsegurarExito(respuesta);
        var resultado = await respuesta.Content.ReadFromJsonAsync<ConversionResultadoDto>();
        return resultado!.DocumentoId;
    }

    // --- Documentos ---

    public async Task<List<DocumentoResumenDto>> ObtenerDocumentosAsync() =>
        await http.GetFromJsonAsync<List<DocumentoResumenDto>>("api/documentos") ?? [];

    public async Task<DocumentoDto?> ObtenerDocumentoAsync(Guid id)
    {
        var respuesta = await http.GetAsync($"api/documentos/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;
        await AsegurarExito(respuesta);
        return await respuesta.Content.ReadFromJsonAsync<DocumentoDto>();
    }

    public async Task GuardarDocumentoAsync(Guid id, string titulo, string contenido) =>
        await AsegurarExito(await http.PutAsJsonAsync($"api/documentos/{id}", new ActualizarDocumentoRequest(titulo, contenido)));

    public async Task EliminarDocumentoAsync(Guid id) =>
        await AsegurarExito(await http.DeleteAsync($"api/documentos/{id}"));

    // --- Vocabulario ---

    public async Task<List<TerminoDto>> ObtenerVocabularioAsync() =>
        await http.GetFromJsonAsync<List<TerminoDto>>("api/vocabulario") ?? [];

    public async Task<TerminoDto> CrearTerminoAsync(string termino, string? descripcion)
    {
        var respuesta = await http.PostAsJsonAsync("api/vocabulario", new GuardarTerminoRequest(termino, descripcion));
        await AsegurarExito(respuesta);
        return (await respuesta.Content.ReadFromJsonAsync<TerminoDto>())!;
    }

    public async Task ActualizarTerminoAsync(Guid id, string termino, string? descripcion) =>
        await AsegurarExito(await http.PutAsJsonAsync($"api/vocabulario/{id}", new GuardarTerminoRequest(termino, descripcion)));

    public async Task EliminarTerminoAsync(Guid id) =>
        await AsegurarExito(await http.DeleteAsync($"api/vocabulario/{id}"));

    // --- Utilidades ---

    private static async Task AsegurarExito(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return;

        var detalle = await respuesta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(detalle))
            detalle = $"Error {(int)respuesta.StatusCode}";
        throw new ApiException(respuesta.StatusCode, detalle.Trim('"'));
    }

    private static string ExtensionDe(string mimeType) => mimeType switch
    {
        "audio/webm" => ".webm",
        "audio/mp4" => ".m4a",
        _ => ".bin"
    };
}
