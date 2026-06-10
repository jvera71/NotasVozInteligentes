# 🎙️ Notas de Voz Inteligentes

Aplicación web (mono usuario) para capturar notas de voz a lo largo del día y convertirlas, bajo demanda, en un documento **Markdown** estructurado por proyecto y por pantalla/clase/servicio/módulo, usando la **API de Gemini**.

El modelo recibe los audios directamente (sin paso previo de transcripción), limpia titubeos y muletillas, resuelve contradicciones entre notas (la más reciente prevalece) y categoriza el contenido. Un **vocabulario personalizado** (CRUD) se envía junto a las notas para evitar errores de transcripción en términos propios (ej. *opstream*, *todopedia*).

Para el detalle completo de la arquitectura, modelo de datos y flujos, consulta [docs/ARQUITECTURA.md](docs/ARQUITECTURA.md).

---

## ✨ Características

- 🎤 Grabación de notas de voz desde el navegador (`MediaRecorder`).
- 🤖 Conversión global a Markdown con la API de Gemini (un único análisis para todas las notas pendientes).
- 📂 Agrupación automática por **Proyecto** → **Pantalla / Clase / Servicio / Módulo**.
- 📖 Vocabulario personalizado (CRUD) para evitar *mismatches* de transcripción.
- 📝 Editor Markdown integrado con **Monaco Editor**, vista previa, guardado, borrado y copia al portapapeles.
- 🗄️ Persistencia con **Entity Framework Core** + **SQLite**.

---

## 🧰 Tecnologías

- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) — Blazor Web App (render interactivo WebAssembly)
- Entity Framework Core (code first) + SQLite
- Monaco Editor
- Google Gemini API (`generateContent`, multimodal)

---

## ✅ Requisitos previos

- [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Una clave de API de [Google AI Studio](https://aistudio.google.com/) para Gemini

---

## 🚀 Puesta en marcha (local)

### 1. Clonar el repositorio

```bash
git clone https://github.com/jvera71/NotasVozInteligentes.git
cd NotasVozInteligentes
```

### 2. Configurar la clave de Gemini

La clave **nunca** debe ir en `appsettings.json` ni llegar al navegador. En desarrollo, usa los *user secrets* de .NET:

```bash
dotnet user-secrets set "Gemini:ApiKey" "TU_CLAVE_DE_API" --project src/NotasVozInteligentes
```

Opcionalmente, puedes cambiar el modelo (por defecto `gemini-2.5-flash`) en `appsettings.json` o vía `Gemini:Model`.

### 3. Ejecutar la aplicación

```bash
dotnet run --project src/NotasVozInteligentes
```

Las migraciones de EF Core se aplican automáticamente al arrancar (se crea `App_Data/notas.db`). La aplicación queda disponible en la URL indicada en la consola (por ejemplo `http://localhost:5085`).

---

## 🐳 Puesta en marcha con Docker

### 1. Configurar las variables de entorno

```bash
cp .env.example .env
```

Edita `.env` y añade tu clave:

```env
GEMINI_API_KEY=tu-clave-de-api-aqui
GEMINI_MODEL=gemini-2.5-flash
```

### 2. Levantar el contenedor

```bash
docker compose up -d --build
```

La aplicación quedará disponible en [http://localhost:8080](http://localhost:8080). Los datos (base de datos SQLite y audios pendientes) se persisten en el volumen `notas-data`.

Para detenerla:

```bash
docker compose down
```

---

## 📂 Estructura del proyecto

```
src/
├── NotasVozInteligentes/          # Host ASP.NET Core: API REST, EF Core, integración con Gemini
└── NotasVozInteligentes.Client/   # Cliente Blazor WebAssembly: grabación, editor, vocabulario
docs/
└── ARQUITECTURA.md                # Arquitectura y diseño técnico detallado
```

---

## 📖 Uso básico

1. En la página **Notas**, pulsa **🎙️ Grabar nota** y dicta lo que necesites (puedes grabar varias notas a lo largo del día).
2. Cuando quieras procesarlas, pulsa **✨ Convertir a texto**. Se generará un nuevo documento Markdown y las notas de voz se eliminarán.
3. En **Documentos**, abre el documento generado, edítalo con el editor integrado, cópialo al portapapeles o elimínalo.
4. En **Vocabulario**, da de alta los términos propios de tus proyectos (nombres, productos, tecnologías) para que Gemini los transcriba siempre correctamente.

---

## 📄 Licencia

Este proyecto se distribuye bajo los términos de la licencia [MIT](LICENSE.txt).

---

<div align="center">

### 🤔 ¿Programas mejor que yo?

**Me gusta aprender, cuéntamelo...**

📧 **programomejorquetu@hotmail.com**

</div>
