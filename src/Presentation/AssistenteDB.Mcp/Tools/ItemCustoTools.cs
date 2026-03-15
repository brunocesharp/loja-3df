using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class ItemCustoTools(IItemCustoRepository repo, IItemRepository itemRepo)
{
    [McpServerTool(Name = "listar_custos_item"), Description("Lista os custos de um item pelo itemId.")]
    public async Task<string> ListarCustosItem(
        [Description("Id do item")] long itemId)
    {
        try
        {
            var item = await itemRepo.GetByIdAsync(itemId);
            if (item is null)
                return JsonSerializer.Serialize(new { error = $"Item com id {itemId} não encontrado." });
            var custos = await repo.GetByItemIdAsync(itemId);
            return JsonSerializer.Serialize(custos.Select(c => new { c.Id, c.ItemId, c.Peso, c.Tempo, c.Quantidade, c.Perdas }));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "obter_custo"), Description("Obtém um custo de item pelo id.")]
    public async Task<string> ObterCusto(
        [Description("Id do custo")] long id)
    {
        try
        {
            var custo = await repo.GetByIdAsync(id);
            if (custo is null)
                return JsonSerializer.Serialize(new { error = $"Custo com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { custo.Id, custo.ItemId, custo.Peso, custo.Tempo, custo.Quantidade, custo.Perdas });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "criar_custo"), Description("Cria um novo custo para um item. Pelo menos um campo deve ser informado.")]
    public async Task<string> CriarCusto(
        [Description("Id do item")] long itemId,
        [Description("Peso (decimal, opcional)")] decimal? peso = null,
        [Description("Tempo (decimal, opcional)")] decimal? tempo = null,
        [Description("Quantidade (decimal, opcional)")] decimal? quantidade = null,
        [Description("Perdas (decimal, opcional)")] decimal? perdas = null)
    {
        try
        {
            var item = await itemRepo.GetByIdAsync(itemId);
            if (item is null)
                return JsonSerializer.Serialize(new { error = $"Item com id {itemId} não encontrado." });

            if (peso is null && tempo is null && quantidade is null && perdas is null)
                return JsonSerializer.Serialize(new { error = "Pelo menos um campo de custo deve ser informado." });

            var custo = new ItemCusto { ItemId = itemId, Peso = peso, Tempo = tempo, Quantidade = quantidade, Perdas = perdas };
            var created = await repo.CreateAsync(custo);
            return JsonSerializer.Serialize(new { created.Id, created.ItemId, created.Peso, created.Tempo, created.Quantidade, created.Perdas });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "atualizar_custo"), Description("Atualiza um custo de item existente. Pelo menos um campo deve ser informado.")]
    public async Task<string> AtualizarCusto(
        [Description("Id do custo")] long id,
        [Description("Peso (decimal, opcional)")] decimal? peso = null,
        [Description("Tempo (decimal, opcional)")] decimal? tempo = null,
        [Description("Quantidade (decimal, opcional)")] decimal? quantidade = null,
        [Description("Perdas (decimal, opcional)")] decimal? perdas = null)
    {
        try
        {
            if (peso is null && tempo is null && quantidade is null && perdas is null)
                return JsonSerializer.Serialize(new { error = "Pelo menos um campo de custo deve ser informado." });

            var updated = await repo.UpdateAsync(id, new ItemCusto { Peso = peso, Tempo = tempo, Quantidade = quantidade, Perdas = perdas });
            if (updated is null)
                return JsonSerializer.Serialize(new { error = $"Custo com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { updated.Id, updated.ItemId, updated.Peso, updated.Tempo, updated.Quantidade, updated.Perdas });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "deletar_custo"), Description("Deleta um custo de item pelo id.")]
    public async Task<string> DeletarCusto(
        [Description("Id do custo")] long id)
    {
        try
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return JsonSerializer.Serialize(new { error = $"Custo com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { success = true, message = $"Custo {id} deletado com sucesso." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
