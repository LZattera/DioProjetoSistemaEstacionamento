using Microsoft.EntityFrameworkCore;
using Serilog;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;

// 1. Configuração do Logger inicial (captura erros que ocorrem antes do Host subir)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando a API de Estacionamento...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. Conecta o Serilog ao ASP.NET Core
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

    // ============================================================
    // CONFIGURAÇÃO DO BANCO DE DADOS (ENTITY FRAMEWORK)
    // ============================================================
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseInMemoryDatabase("EstacionamentoDbMemory"));

    // ============================================================
    // INJEÇÃO DE DEPENDÊNCIA (SERVICES)
    // ============================================================
    builder.Services.AddScoped<IServiceCarro, CarroService>();
    builder.Services.AddScoped<IServiceRegistro, RegistroService>();

    // ============================================================
    // CONFIGURAÇÕES DA API E SWAGGER
    // ============================================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // 3. Substitui os logs padrão de requisição do ASP.NET por logs compactos e estruturados
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        Log.Information("Ambiente de Desenvolvimento ativo. Carregando Swagger...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação encerrou inesperadamente.");
}
finally
{
    // Garante a gravação de todos os buffers em disco antes de finalizar o processo
    Log.CloseAndFlush();
}