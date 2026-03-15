using System.ComponentModel;
using System.Text.Json;
using AssistenteDB.Domain.Entities;
using AssistenteDB.Domain.Interfaces;
using ModelContextProtocol.Server;

namespace AssistenteDB.Mcp.Tools;

[McpServerToolType]
public class ProdutoVersaoTools(
    IProdutoVersaoRepository repo,
    IProdutoRepository produtoRepo,
    IProdutoArquivoRepository arquivoVincRepo,
    IArquivoRepository arquivoRepo)
{
    [McpServerTool(Name = "listar_versoes_produto"), Description("Lista as versões de um produto pelo produtoId.")]
    public async Task<string> ListarVersoesProduto(
        [Description("Id do produto")] long produtoId)
    {
        try
        {
            var produto = await produtoRepo.GetByIdAsync(produtoId);
            if (produto is null)
                return JsonSerializer.Serialize(new { error = $"Produto com id {produtoId} não encontrado." });
            var versoes = await repo.GetByProdutoIdAsync(produtoId);
            return JsonSerializer.Serialize(versoes.Select(ToDto));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "obter_versao"), Description("Obtém uma versão de produto pelo id.")]
    public async Task<string> ObterVersao(
        [Description("Id da versão")] long id)
    {
        try
        {
            var versao = await repo.GetByIdAsync(id);
            if (versao is null)
                return JsonSerializer.Serialize(new { error = $"Versão com id {id} não encontrada." });
            return JsonSerializer.Serialize(ToDto(versao));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "criar_versao"), Description("Cria uma nova versão para um produto.")]
    public async Task<string> CriarVersao(
        [Description("Id do produto")] long produtoId,
        [Description("Nome da versão (máx. 150 caracteres)")] string nome,
        [Description("Número da versão (maior que 0)")] int numero)
    {
        try
        {
            var produto = await produtoRepo.GetByIdAsync(produtoId);
            if (produto is null)
                return JsonSerializer.Serialize(new { error = $"Produto com id {produtoId} não encontrado." });

            if (await repo.NumeroExisteAsync(produtoId, numero))
                return JsonSerializer.Serialize(new { error = $"Já existe uma versão com o número {numero} para este produto." });

            var versao = new ProdutoVersao { ProdutoId = produtoId, Nome = nome, Numero = numero };
            var created = await repo.CreateAsync(versao);
            return JsonSerializer.Serialize(ToDto(created));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "atualizar_versao"), Description("Atualiza uma versão de produto existente.")]
    public async Task<string> AtualizarVersao(
        [Description("Id da versão")] long id,
        [Description("Nome da versão (máx. 150 caracteres)")] string nome,
        [Description("Número da versão (maior que 0)")] int numero)
    {
        try
        {
            var versao = await repo.GetByIdAsync(id);
            if (versao is null)
                return JsonSerializer.Serialize(new { error = $"Versão com id {id} não encontrada." });

            if (await repo.NumeroExisteAsync(versao.ProdutoId, numero, excludeId: id))
                return JsonSerializer.Serialize(new { error = $"Já existe uma versão com o número {numero} para este produto." });

            var updated = await repo.UpdateAsync(id, new ProdutoVersao { Nome = nome, Numero = numero });
            if (updated is null)
                return JsonSerializer.Serialize(new { error = $"Versão com id {id} não encontrada." });
            return JsonSerializer.Serialize(ToDto(updated));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "deletar_versao"), Description("Deleta uma versão de produto pelo id.")]
    public async Task<string> DeletarVersao(
        [Description("Id da versão")] long id)
    {
        try
        {
            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
                return JsonSerializer.Serialize(new { error = $"Versão com id {id} não encontrada." });
            return JsonSerializer.Serialize(new { success = true, message = $"Versão {id} deletada com sucesso." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "vincular_arquivo_versao"), Description("Vincula um arquivo a uma versão de produto.")]
    public async Task<string> VincularArquivoVersao(
        [Description("Id da versão")] long id,
        [Description("Id do arquivo")] long arquivoId)
    {
        try
        {
            var versao = await repo.GetByIdAsync(id);
            if (versao is null)
                return JsonSerializer.Serialize(new { error = $"Versão com id {id} não encontrada." });

            var arquivo = await arquivoRepo.GetByIdAsync(arquivoId);
            if (arquivo is null)
                return JsonSerializer.Serialize(new { error = "arquivoId inválido." });

            var resultado = await arquivoVincRepo.VincularAsync(id, arquivoId);
            if (!resultado)
                return JsonSerializer.Serialize(new { error = "Arquivo já está vinculado a outra versão." });

            return JsonSerializer.Serialize(new
            {
                success = true,
                produtoVersaoId = id,
                arquivo = new { arquivo.Id, tipoArquivo = new { arquivo.TipoArquivo.Id, arquivo.TipoArquivo.Nome, arquivo.TipoArquivo.Sigla } }
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpServerTool(Name = "desvincular_arquivo_versao"), Description("Remove o vínculo de arquivo de uma versão de produto.")]
    public async Task<string> DesvincularArquivoVersao(
        [Description("Id da versão")] long id)
    {
        try
        {
            var desvinculado = await arquivoVincRepo.DesvincularAsync(id);
            if (!desvinculado)
                return JsonSerializer.Serialize(new { error = "Versão não possui arquivo vinculado." });
            return JsonSerializer.Serialize(new { success = true, message = $"Arquivo desvinculado da versão {id}." });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private static object ToDto(ProdutoVersao v)
    {
        var p = v.Produto;
        object? arquivoDto = null;
        if (v.ProdutoArquivo?.Arquivo is not null)
        {
            var ta = v.ProdutoArquivo.Arquivo.TipoArquivo;
            arquivoDto = new { v.ProdutoArquivo.Arquivo.Id, tipoArquivo = new { ta.Id, ta.Nome, ta.Sigla } };
        }
        return new
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
            v.Numero,
            arquivo = arquivoDto
        };
    }
}
