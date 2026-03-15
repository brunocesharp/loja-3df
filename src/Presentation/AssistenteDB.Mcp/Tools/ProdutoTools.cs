using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class ProdutoTools(IProdutoRepository repo, ITipoProdutoRepository tipoRepo)
{
    [McpServerTool(Name = "listar_produtos"), Description("Lista produtos com suporte a filtros e paginação.")]
    public async Task<string> ListarProdutos(
        [Description("Número da página (padrão: 1)")] int page = 1,
        [Description("Tamanho da página (padrão: 20)")] int pageSize = 20,
        [Description("Filtrar por ativado/desativado")] bool? ativado = null,
        [Description("Filtrar por id de tipo de produto")] long? tipoProdutoId = null)
    {
        try
        {
            pageSize = Math.Min(pageSize, 100);
            var (items, total) = await repo.GetAllAsync(page, pageSize, ativado, tipoProdutoId);
            return JsonSerializer.Serialize(new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                data = items.Select(ToDto)
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "obter_produto"), Description("Obtém um produto pelo id.")]
    public async Task<string> ObterProduto(
        [Description("Id do produto")] long id)
    {
        try
        {
            var produto = await repo.GetByIdAsync(id);
            if (produto is null)
                return JsonSerializer.Serialize(new { error = $"Produto com id {id} não encontrado." });
            return JsonSerializer.Serialize(ToDto(produto));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "criar_produto"), Description("Cria um novo produto.")]
    public async Task<string> CriarProduto(
        [Description("Id do tipo de produto")] long tipoProdutoId,
        [Description("Nome do produto (máx. 150 caracteres)")] string nome,
        [Description("Descrição opcional")] string? descricao = null,
        [Description("Se o produto está ativo (padrão: true)")] bool ativado = true)
    {
        try
        {
            var tipo = await tipoRepo.GetByIdAsync(tipoProdutoId);
            if (tipo is null)
                return JsonSerializer.Serialize(new { error = "tipoProdutoId inválido." });

            var produto = new Produto { TipoProdutoId = tipoProdutoId, Nome = nome, Descricao = descricao, Ativado = ativado };
            var created = await repo.CreateAsync(produto);
            return JsonSerializer.Serialize(ToDto(created));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "atualizar_produto"), Description("Atualiza um produto existente.")]
    public async Task<string> AtualizarProduto(
        [Description("Id do produto")] long id,
        [Description("Id do tipo de produto")] long tipoProdutoId,
        [Description("Nome do produto (máx. 150 caracteres)")] string nome,
        [Description("Se o produto está ativo")] bool ativado,
        [Description("Descrição opcional")] string? descricao = null)
    {
        try
        {
            var tipo = await tipoRepo.GetByIdAsync(tipoProdutoId);
            if (tipo is null)
                return JsonSerializer.Serialize(new { error = "tipoProdutoId inválido." });

            var updated = await repo.UpdateAsync(id, new Produto { TipoProdutoId = tipoProdutoId, Nome = nome, Descricao = descricao, Ativado = ativado });
            if (updated is null)
                return JsonSerializer.Serialize(new { error = $"Produto com id {id} não encontrado." });
            return JsonSerializer.Serialize(ToDto(updated));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "deletar_produto"), Description("Deleta um produto pelo id.")]
    public async Task<string> DeletarProduto(
        [Description("Id do produto")] long id)
    {
        try
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return JsonSerializer.Serialize(new { error = $"Produto com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { success = true, message = $"Produto {id} deletado com sucesso." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private static object ToDto(Produto p) => new
    {
        p.Id,
        tipoProduto = new { p.TipoProduto.Id, p.TipoProduto.Sigla, p.TipoProduto.Nome, p.TipoProduto.Descricao },
        p.Nome,
        p.Descricao,
        p.DataCriacao,
        p.DataAtualizacao,
        p.Ativado
    };
}
