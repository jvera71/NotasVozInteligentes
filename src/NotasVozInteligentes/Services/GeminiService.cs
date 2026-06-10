using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NotasVozInteligentes.Models;

namespace NotasVozInteligentes.Services;

public class GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger) : IGeminiService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<ResultadoConversion> ProcesarNotasAsync(
        IReadOnlyList<AudioNota> audios,
        IReadOnlyList<TerminoVocabulario> glosario,
        CancellationToken ct = default)
    {
        var apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException(
                "Falta la clave de API de Gemini. Configúrala con 'dotnet user-secrets set Gemini:ApiKey <clave>' o la variable de entorno Gemini__ApiKey.");
        var modelo = config["Gemini:Model"] ?? "gemini-2.5-flash";

        var peticion = ConstruirPeticion(audios, glosario);

        using var mensaje = new HttpRequestMessage(
            HttpMethod.Post,
            $"v1beta/models/{modelo}:generateContent");
        mensaje.Headers.Add("x-goog-api-key", apiKey);
        mensaje.Content = new StringContent(peticion.ToJsonString(), Encoding.UTF8, "application/json");

        var inicio = DateTimeOffset.UtcNow;
        using var respuesta = await http.SendAsync(mensaje, ct);
        var cuerpo = await respuesta.Content.ReadAsStringAsync(ct);

        if (!respuesta.IsSuccessStatusCode)
        {
            logger.LogError("Gemini respondió {Status}: {Cuerpo}", (int)respuesta.StatusCode, cuerpo);
            throw new InvalidOperationException($"Error de la API de Gemini ({(int)respuesta.StatusCode}).");
        }

        logger.LogInformation(
            "Gemini procesó {NumAudios} audios en {Latencia:F1}s",
            audios.Count, (DateTimeOffset.UtcNow - inicio).TotalSeconds);

        return ExtraerResultado(cuerpo);
    }

    private static JsonObject ConstruirPeticion(
        IReadOnlyList<AudioNota> audios,
        IReadOnlyList<TerminoVocabulario> glosario)
    {
        var parts = new JsonArray();
        foreach (var audio in audios.OrderBy(a => a.FechaCreacion))
        {
            parts.Add((JsonNode)new JsonObject
            {
                ["text"] = $"--- Nota de voz grabada el {audio.FechaCreacion:yyyy-MM-dd HH:mm} ---"
            });
            parts.Add((JsonNode)new JsonObject
            {
                ["inline_data"] = new JsonObject
                {
                    ["mime_type"] = audio.MimeType,
                    ["data"] = Convert.ToBase64String(audio.Contenido)
                }
            });
        }

        return new JsonObject
        {
            ["system_instruction"] = new JsonObject
            {
                ["parts"] = new JsonArray((JsonNode)new JsonObject { ["text"] = ConstruirPromptSistema(glosario) })
            },
            ["contents"] = new JsonArray((JsonNode)new JsonObject
            {
                ["role"] = "user",
                ["parts"] = parts
            }),
            ["generationConfig"] = new JsonObject
            {
                ["response_mime_type"] = "application/json",
                ["response_schema"] = EsquemaRespuesta()
            }
        };
    }

    private static string ConstruirPromptSistema(IReadOnlyList<TerminoVocabulario> glosario)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
            Eres un asistente que procesa notas de voz de un desarrollador de software.
            Recibirás varias notas de voz en orden cronológico, cada una precedida de su marca temporal.

            REGLAS:
            1. Transcribe el contenido eliminando titubeos, muletillas y repeticiones. Redacta cada tarea de forma clara y concisa.
            2. Las notas están ordenadas cronológicamente. Si una nota posterior contradice, corrige o anula una anterior, prevalece la más reciente y la información anulada NO debe aparecer en el resultado.
            3. Agrupa el contenido por proyecto y, dentro de cada proyecto, por pantalla, clase, servicio o módulo mencionado. En el campo "tipo" indica cuál de ellos es (Pantalla, Clase, Servicio, Módulo u Otro).
            4. Lo que no puedas asociar a ningún proyecto va al array "sinClasificar". Nunca descartes contenido que no esté anulado por una nota posterior.
            5. Responde en el mismo idioma de las notas.
            """);

        if (glosario.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("GLOSARIO — Los siguientes términos son nombres propios del usuario y deben transcribirse EXACTAMENTE con esta grafía cuando se pronuncien o se pronuncie algo fonéticamente similar; no los sustituyas por palabras parecidas del idioma:");
            foreach (var termino in glosario)
            {
                sb.Append("- ").Append(termino.Termino);
                if (!string.IsNullOrWhiteSpace(termino.Descripcion))
                    sb.Append(": ").Append(termino.Descripcion);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static JsonObject EsquemaRespuesta() => new()
    {
        ["type"] = "OBJECT",
        ["properties"] = new JsonObject
        {
            ["proyectos"] = new JsonObject
            {
                ["type"] = "ARRAY",
                ["items"] = new JsonObject
                {
                    ["type"] = "OBJECT",
                    ["properties"] = new JsonObject
                    {
                        ["nombre"] = new JsonObject { ["type"] = "STRING" },
                        ["elementos"] = new JsonObject
                        {
                            ["type"] = "ARRAY",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "OBJECT",
                                ["properties"] = new JsonObject
                                {
                                    ["nombre"] = new JsonObject { ["type"] = "STRING" },
                                    ["tipo"] = new JsonObject { ["type"] = "STRING" },
                                    ["tareas"] = new JsonObject
                                    {
                                        ["type"] = "ARRAY",
                                        ["items"] = new JsonObject { ["type"] = "STRING" }
                                    }
                                },
                                ["required"] = new JsonArray("nombre", "tipo", "tareas")
                            }
                        }
                    },
                    ["required"] = new JsonArray("nombre", "elementos")
                }
            },
            ["sinClasificar"] = new JsonObject
            {
                ["type"] = "ARRAY",
                ["items"] = new JsonObject { ["type"] = "STRING" }
            }
        },
        ["required"] = new JsonArray("proyectos", "sinClasificar")
    };

    private ResultadoConversion ExtraerResultado(string cuerpoRespuesta)
    {
        try
        {
            var nodo = JsonNode.Parse(cuerpoRespuesta);
            var texto = nodo?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>()
                ?? throw new InvalidOperationException("La respuesta de Gemini no contiene texto.");

            return JsonSerializer.Deserialize<ResultadoConversion>(texto, JsonOpts)
                ?? throw new InvalidOperationException("El JSON devuelto por Gemini está vacío.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "No se pudo parsear la respuesta de Gemini");
            throw new InvalidOperationException("La respuesta de Gemini no es un JSON válido.", ex);
        }
    }
}
