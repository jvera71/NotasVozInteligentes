namespace NotasVozInteligentes.Services;

public class FileSystemAudioStorage : IAudioStorage
{
    private readonly string _raiz;
    private readonly ILogger<FileSystemAudioStorage> _logger;

    public FileSystemAudioStorage(IWebHostEnvironment env, ILogger<FileSystemAudioStorage> logger)
    {
        _raiz = Path.Combine(env.ContentRootPath, "App_Data", "audio");
        Directory.CreateDirectory(_raiz);
        _logger = logger;
    }

    public async Task<string> GuardarAsync(Stream contenido, string mimeType, CancellationToken ct = default)
    {
        var extension = mimeType switch
        {
            "audio/webm" => ".webm",
            "audio/mp4" => ".m4a",
            "audio/mpeg" => ".mp3",
            "audio/ogg" => ".ogg",
            "audio/wav" => ".wav",
            _ => ".bin"
        };
        var nombre = $"{Guid.NewGuid():N}{extension}";
        var rutaCompleta = Path.Combine(_raiz, nombre);
        await using var destino = File.Create(rutaCompleta);
        await contenido.CopyToAsync(destino, ct);
        return nombre;
    }

    public Stream Abrir(string rutaRelativa) =>
        File.OpenRead(RutaCompleta(rutaRelativa));

    public Task<byte[]> LeerAsync(string rutaRelativa, CancellationToken ct = default) =>
        File.ReadAllBytesAsync(RutaCompleta(rutaRelativa), ct);

    public void Eliminar(string rutaRelativa)
    {
        try
        {
            File.Delete(RutaCompleta(rutaRelativa));
        }
        catch (IOException ex)
        {
            // Un audio huérfano en disco es inocuo; no debe revertir la conversión.
            _logger.LogWarning(ex, "No se pudo eliminar el audio {Ruta}", rutaRelativa);
        }
    }

    private string RutaCompleta(string rutaRelativa)
    {
        var ruta = Path.GetFullPath(Path.Combine(_raiz, rutaRelativa));
        if (!ruta.StartsWith(_raiz, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Ruta de audio fuera del directorio permitido.");
        return ruta;
    }
}
