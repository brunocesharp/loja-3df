using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class ItemTools(
    IItemRepository repo,
    IProdutoVersaoRepository versaoRepo,
    IItemArquivoRepository itemArquivoRepo,
    IArquivoRepository arquivoRepo)
{
    [McpServerTool(Name = "listar_itens_versao"), Description("Lista os itens de uma versão de produto pelo versaoId.")]
    public async Task<string> ListarItensVersao(
        [Description("Id da versão de produto")] long versaoId)
    {
        try
        {
            var versao = await versaoRepo.GetByIdAsync(versaoId);
            if (versao is null)
                return JsonSerializer.Serialize(new { error = $"Versão com id {versaoId} não encontrada." });
            var itens = await repo.GetByVersaoIdAsync(versaoId);
            return JsonSerializer.Serialize(itens.Select(i => new { i.Id, i.ProdutoVersaoId, i.Nome, i.Descricao }));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "obter_item"), Description("Obtém um item pelo id.")]
    public async Task<string> ObterItem(
        [Description("Id do item")] long id)
    {
        try
        {
            var item = await repo.GetByIdAsync(id);
            if (item is null)
                return JsonSerializer.Serialize(new { error = $"Item com id {id} não encontrado." });
            return JsonSerializer.Serialize(ToDto(item));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "criar_item"), Description("Cria um novo item em uma versão de produto.")]
    public async Task<string> CriarItem(
        [Description("Id da versão de produto")] long versaoId,
        [Description("Nome do item (máx. 150 caracteres)")] string nome,
        [Description("Descrição opcional")] string? descricao = null)
    {
        try
        {
            var versao = await versaoRepo.GetByIdAsync(versaoId);
            if (versao is null)
                return JsonSerializer.Serialize(new { error = $"Versão com id {versaoId} não encontrada." });

            var item = new Item { ProdutoVersaoId = versaoId, Nome = nome, Descricao = descricao };
            var created = await repo.CreateAsync(item);
            return JsonSerializer.Serialize(ToDto(created));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "atualizar_item"), Description("Atualiza um item existente.")]
    public async Task<string> AtualizarItem(
        [Description("Id do item")] long id,
        [Description("Nome do item (máx. 150 caracteres)")] string nome,
        [Description("Descrição opcional")] string? descricao = null)
    {
        try
        {
            var updated = await repo.UpdateAsync(id, new Item { Nome = nome, Descricao = descricao });
            if (updated is null)
                return JsonSerializer.Serialize(new { error = $"Item com id {id} não encontrado." });
            return JsonSerializer.Serialize(ToDto(updated));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "deletar_item"), Description("Deleta um item pelo id.")]
    public async Task<string> DeletarItem(
        [Description("Id do item")] long id)
    {
        try
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return JsonSerializer.Serialize(new { error = $"Item com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { success = true, message = $"Item {id} deletado com sucesso." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "vincular_arquivo_item"), Description("Vincula um arquivo a um item.")]
    public async Task<string> VincularArquivoItem(
        [Description("Id do item")] long id,
        [Description("Id do arquivo")] long arquivoId)
    {
        try
        {
            var item = await repo.GetByIdAsync(id);
            if (item is null)
                return JsonSerializer.Serialize(new { error = $"Item com id {id} não encontrado." });

            var arquivo = await arquivoRepo.GetByIdAsync(arquivoId);
            if (arquivo is null)
                return JsonSerializer.Serialize(new { error = "arquivoId inválido." });

            var resultado = await itemArquivoRepo.VincularAsync(id, arquivoId);
            if (!resultado)
                return JsonSerializer.Serialize(new { error = "Arquivo já está vinculado a outro item." });

            return JsonSerializer.Serialize(new
            {
                success = true,
                itemId = id,
                arquivo = new { arquivo.Id, tipoArquivo = new { arquivo.TipoArquivo.Id, arquivo.TipoArquivo.Nome, arquivo.TipoArquivo.Sigla } }
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "desvincular_arquivo_item"), Description("Remove o vínculo de arquivo de um item.")]
    public async Task<string> DesvincularArquivoItem(
        [Description("Id do item")] long id)
    {
        try
        {
            var desvinculado = await itemArquivoRepo.DesvincularAsync(id);
            if (!desvinculado)
                return JsonSerializer.Serialize(new { error = "Item não possui arquivo vinculado." });
            return JsonSerializer.Serialize(new { success = true, message = $"Arquivo desvinculado do item {id}." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private static object ToDto(Item i)
    {
        var v = i.ProdutoVersao;
        var p = v.Produto;
        var versaoDto = new
        {
            v.Id,
            v.ProdutoId,
            produto = new
            {
                p.Id,
                tipoProduto = new { p.TipoProduto.Id, p.TipoProduto.Sigla, p.TipoProduto.Nome, p.TipoProduto.Descricao },
                p.Nome, p.Descricao, p.DataCriacao, p.DataAtualizacao, p.Ativado
            },
            v.Nome,
            v.Numero
        };
        var custos = i.Custos.Select(c => new { c.Id, c.ItemId, c.Peso, c.Tempo, c.Quantidade, c.Perdas });
        object? arquivoDto = null;
        if (i.ItemArquivo?.Arquivo is not null)
        {
            var ta = i.ItemArquivo.Arquivo.TipoArquivo;
            arquivoDto = new { i.ItemArquivo.Arquivo.Id, tipoArquivo = new { ta.Id, ta.Nome, ta.Sigla } };
        }
        return new { i.Id, i.ProdutoVersaoId, versao = versaoDto, i.Nome, i.Descricao, custos, arquivo = arquivoDto };
    }
}
