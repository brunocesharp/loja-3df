using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class ArquivoTools(IArquivoRepository repo)
{
    [McpServerTool(Name = "upload_arquivo"), Description("Faz upload de um arquivo em base64, criando o registro e retornando o id gerado.")]
    public async Task<string> UploadArquivo(
        [Description("Id do tipo do arquivo")] long tipoArquivoId,
        [Description("Conteúdo do arquivo codificado em base64")] string base64)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch
        {
            return JsonSerializer.Serialize(new { error = "Base64 inválido." });
        }

        try
        {
            var arquivo = new Arquivo { TipoArquivoId = tipoArquivoId, Bytes = bytes };
            await repo.CreateAsync(arquivo);
            return JsonSerializer.Serialize(new
            {
                id = arquivo.Id,
                tipoArquivoId = arquivo.TipoArquivoId,
                message = "Arquivo criado com sucesso."
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "download_arquivo"), Description("Baixa os bytes de um arquivo pelo id, retornando base64 e content type.")]
    public async Task<string> DownloadArquivo(
        [Description("Id do arquivo")] long id)
    {
        try
        {
            var arquivo = await repo.GetByIdAsync(id);
            if (arquivo is null)
                return JsonSerializer.Serialize(new { error = $"Arquivo com id {id} não encontrado." });
            if (arquivo.Bytes is null || arquivo.Bytes.Length == 0)
                return JsonSerializer.Serialize(new { error = "Arquivo sem conteúdo." });

            return JsonSerializer.Serialize(new
            {
                id = arquivo.Id,
                contentType = "application/octet-stream",
                base64 = Convert.ToBase64String(arquivo.Bytes)
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "delete_arquivo"), Description("Deleta um arquivo pelo id.")]
    public async Task<string> DeleteArquivo(
        [Description("Id do arquivo")] long id)
    {
        try
        {
            var arquivo = await repo.GetByIdAsync(id);
            if (arquivo is null)
                return JsonSerializer.Serialize(new { error = $"Arquivo com id {id} não encontrado." });

            if (await repo.IsVinculadoAsync(id))
                return JsonSerializer.Serialize(new { error = "Arquivo está vinculado a um item ou versão e não pode ser removido." });

            await repo.DeleteAsync(id);
            return JsonSerializer.Serialize(new { success = true, message = $"Arquivo {id} deletado com sucesso." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}