using Microsoft.EntityFrameworkCore;
using NotasVozInteligentes.Models;

namespace NotasVozInteligentes.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<NotaVoz> NotasVoz => Set<NotaVoz>();
    public DbSet<DocumentoMarkdown> Documentos => Set<DocumentoMarkdown>();
    public DbSet<TerminoVocabulario> Vocabulario => Set<TerminoVocabulario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TerminoVocabulario>(e =>
        {
            e.Property(t => t.Termino).UseCollation("NOCASE");
            e.HasIndex(t => t.Termino).IsUnique();
        });
    }
}
