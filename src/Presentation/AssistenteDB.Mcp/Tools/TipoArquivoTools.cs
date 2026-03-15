using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class TipoArquivoTools(ITipoArquivoRepository repo)
{
    [McpServerTool(Name = "listar_tipos_arquivo"), Description("Lista todos os tipos de arquivo disponíveis.")]
    public async Task<string> ListarTiposArquivo()
    {
        try
        {
            var tipos = await repo.GetAllAsync();
            return JsonSerializer.Serialize(tipos.Select(t => new { t.Id, t.Nome, t.Sigla }));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "obter_tipo_arquivo"), Description("Obtém um tipo de arquivo pelo id.")]
    public async Task<string> ObterTipoArquivo(
        [Description("Id do tipo de arquivo")] long id)
    {
        try
        {
            var tipo = await repo.GetByIdAsync(id);
            if (tipo is null)
                return JsonSerializer.Serialize(new { error = $"TipoArquivo com id {id} não encontrado." });
            return JsonSerializer.Serialize(new { tipo.Id, tipo.Nome, tipo.Sigla });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
