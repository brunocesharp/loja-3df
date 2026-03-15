using AssistenteDB.Data.Context;
using AssistenteDB.Data.Repositories;
using AssistenteDB.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
    options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));

builder.Services.AddScoped<ITipoArquivoRepository, TipoArquivoRepository>();
builder.Services.AddScoped<ITipoProdutoRepository, TipoProdutoRepository>();
builder.Services.AddScoped<IArquivoRepository, ArquivoRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoVersaoRepository, ProdutoVersaoRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemCustoRepository, ItemCustoRepository>();
builder.Services.AddScoped<IItemArquivoRepository, ItemArquivoRepository>();
builder.Services.AddScoped<IProdutoArquivoRepository, ProdutoArquivoRepository>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();