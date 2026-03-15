using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class TipoProdutoTools(ITipoProdutoRepository repo)
{
    [McpServerTool(Name = "listar_tipos_produto"), Description("Lista todos os tipos de produto disponíveis.")]
    public async Task<string> ListarTiposProduto()
    {
        try
        {
            var tipos = await repo.GetAllAsync();
            return JsonSerializer.Serialize(tipos.Select(t => new { t.Id, t.Sigla, t.Nome, t.Descricao }));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "obter_tipo_produto"), Description("Obtém um tipo de produto pelo id.")]
    public async Task<string> ObterTipoProduto(
        [Description("Id do tipo de produto")] long id)
    {
        try
        {
            var tipo = await repo.GetByIdAsync(id);
            if (tipo is null)
                return JsonSerializer.Serialize(new { error = $"TipoProduto com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { tipo.Id, tipo.Sigla, tipo.Nome, tipo.Descricao });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
