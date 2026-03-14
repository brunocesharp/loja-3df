using Microsoft.EntityFrameworkCore;
using AssistenteDB.Domain.Entities;

namespace AssistenteDB.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TipoArquivo> TiposArquivo => Set<TipoArquivo>();
    public DbSet<TipoProduto> TiposProduto => Set<TipoProduto>();
    public DbSet<Arquivo> Arquivos => Set<Arquivo>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ProdutoVersao> ProdutosVersao => Set<ProdutoVersao>();
    public DbSet<Item> Itens => Set<Item>();
    public DbSet<ItemCusto> ItensCusto => Set<ItemCusto>();
    public DbSet<ItemArquivo> ItensArquivo => Set<ItemArquivo>();
    public DbSet<ProdutoArquivo> ProdutosArquivo => Set<ProdutoArquivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── tipo_arquivo ────────────────────────────────────────────
        modelBuilder.Entity<TipoArquivo>(e =>
        {
            e.ToTable("tipo_arquivo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(100);
            e.Property(x => x.Sigla).HasColumnName("sigla").IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.Sigla).IsUnique().HasDatabaseName("uq_tipo_arquivo_sigla");

            e.HasData(
                new TipoArquivo { Id = 1, Nome = "Documento PDF", Sigla = "PDF" },
                new TipoArquivo { Id = 2, Nome = "Imagem PNG",    Sigla = "PNG" },
                new TipoArquivo { Id = 3, Nome = "Modelo 3D STL", Sigla = "STL" }
            );
        });

        // ── tipo_produto ────────────────────────────────────────────
        modelBuilder.Entity<TipoProduto>(e =>
        {
            e.ToTable("tipo_produto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Sigla).HasColumnName("sigla").IsRequired().HasMaxLength(20);
            e.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(100);
            e.Property(x => x.Descricao).HasColumnName("descricao");
            e.HasIndex(x => x.Sigla).IsUnique().HasDatabaseName("uq_tipo_produto_sigla");

            e.HasData(
                new TipoProduto { Id = 1, Sigla = "ORGN", Nome = "ORGANIZADORES", Descricao = "ORGANIZADORES" },
                new TipoProduto { Id = 2, Sigla = "ACSS", Nome = "ACESSORIOS",    Descricao = "ACESSORIOS"    },
                new TipoProduto { Id = 3, Sigla = "MINI", Nome = "MINIATURA",     Descricao = "MINIATURAS"    }
            );
        });

        // ── arquivo ─────────────────────────────────────────────────
        modelBuilder.Entity<Arquivo>(e =>
        {
            e.ToTable("arquivo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TipoArquivoId).HasColumnName("tipo_arquivo_id");
            e.Property(x => x.Bytes).HasColumnName("byte");
            e.HasOne(x => x.TipoArquivo)
                .WithMany()
                .HasForeignKey(x => x.TipoArquivoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_arquivo_tipo_arquivo");
        });

        // ── produto ──────────────────────────────────────────────────
        modelBuilder.Entity<Produto>(e =>
        {
            e.ToTable("produto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TipoProdutoId).HasColumnName("tipo_produto_id");
            e.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(150);
            e.Property(x => x.Descricao).HasColumnName("descricao");
            e.Property(x => x.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("NOW()");
            e.Property(x => x.DataAtualizacao).HasColumnName("data_atualizacao");
            e.Property(x => x.Ativado).HasColumnName("ativado").HasDefaultValue(true);
            e.HasIndex(x => x.TipoProdutoId).HasDatabaseName("ix_produto_tipo_produto_id");
            e.HasOne(x => x.TipoProduto)
                .WithMany()
                .HasForeignKey(x => x.TipoProdutoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_produto_tipo_produto");
        });

        // ── produto_versao ───────────────────────────────────────────
        modelBuilder.Entity<ProdutoVersao>(e =>
        {
            e.ToTable("produto_versao");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProdutoId).HasColumnName("produto_id");
            e.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(150);
            e.Property(x => x.Numero).HasColumnName("numero");
            e.HasIndex(x => x.ProdutoId).HasDatabaseName("ix_produto_versao_produto_id");
            e.HasIndex(nameof(ProdutoVersao.ProdutoId), nameof(ProdutoVersao.Numero))
                .IsUnique().HasDatabaseName("uq_produto_versao_produto_numero");
            e.HasOne(x => x.Produto)
                .WithMany()
                .HasForeignKey(x => x.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_produto_versao_produto");
        });

        // ── item ─────────────────────────────────────────────────────
        modelBuilder.Entity<Item>(e =>
        {
            e.ToTable("item");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProdutoVersaoId).HasColumnName("produto_versao_id");
            e.Property(x => x.Nome).HasColumnName("nome").IsRequired().HasMaxLength(150);
            e.Property(x => x.Descricao).HasColumnName("descricao");
            e.HasIndex(x => x.ProdutoVersaoId).HasDatabaseName("ix_item_produto_versao_id");
            e.HasOne(x => x.ProdutoVersao)
                .WithMany()
                .HasForeignKey(x => x.ProdutoVersaoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_item_produto_versao");
        });

        // ── item_custo ───────────────────────────────────────────────
        modelBuilder.Entity<ItemCusto>(e =>
        {
            e.ToTable("item_custo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("item_id");
            e.Property(x => x.Peso).HasColumnName("peso").HasColumnType("numeric(12,3)");
            e.Property(x => x.Tempo).HasColumnName("tempo").HasColumnType("numeric(12,2)");
            e.Property(x => x.Quantidade).HasColumnName("quantidade").HasColumnType("numeric(12,3)");
            e.Property(x => x.Perdas).HasColumnName("perdas").HasColumnType("numeric(12,3)");
            e.HasIndex(x => x.ItemId).HasDatabaseName("ix_item_custo_item_id");
            e.HasOne(x => x.Item)
                .WithMany(i => i.Custos)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_item_custo_item");
        });

        // ── item_arquivo ─────────────────────────────────────────────
        modelBuilder.Entity<ItemArquivo>(e =>
        {
            e.ToTable("item_arquivo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("id_item");
            e.Property(x => x.ArquivoId).HasColumnName("id_arquivo");
            e.HasIndex(x => x.ItemId).IsUnique().HasDatabaseName("uq_item_arquivo_item");
            e.HasIndex(x => x.ArquivoId).IsUnique().HasDatabaseName("uq_item_arquivo_arquivo");
            e.HasOne(x => x.Item)
                .WithOne(i => i.ItemArquivo)
                .HasForeignKey<ItemArquivo>(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_item_arquivo_item");
            e.HasOne(x => x.Arquivo)
                .WithMany()
                .HasForeignKey(x => x.ArquivoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_item_arquivo_arquivo");
        });

        // ── produto_arquivo ──────────────────────────────────────────
        modelBuilder.Entity<ProdutoArquivo>(e =>
        {
            e.ToTable("produto_arquivo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProdutoVersaoId).HasColumnName("id_produto_versao");
            e.Property(x => x.ArquivoId).HasColumnName("id_arquivo");
            e.HasIndex(x => x.ProdutoVersaoId).IsUnique().HasDatabaseName("uq_produto_arquivo_produto_versao");
            e.HasIndex(x => x.ArquivoId).IsUnique().HasDatabaseName("uq_produto_arquivo_arquivo");
            e.HasOne(x => x.ProdutoVersao)
                .WithOne(v => v.ProdutoArquivo)
                .HasForeignKey<ProdutoArquivo>(x => x.ProdutoVersaoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_produto_arquivo_produto_versao");
            e.HasOne(x => x.Arquivo)
                .WithMany()
                .HasForeignKey(x => x.ArquivoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_produto_arquivo_arquivo");
        });
    }
}
