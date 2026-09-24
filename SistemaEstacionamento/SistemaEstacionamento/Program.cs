using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;
var builder = WebApplication.CreateBuilder(args);

// ============================================================
// ADICIONE ESTE BLOCO (Obrigatório para o UseSerilogRequestLogging funcionar)
// ============================================================
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Injeção de dependências e Controllers
builder.Services.AddDbContext<DataContext>(options =>
    options.UseInMemoryDatabase("EstacionamentoDbMemory"));
builder.Services.AddScoped<IServiceCarro, CarroService>();
builder.Services.AddScoped<IServiceRegistro, RegistroService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Agora o UseSerilogRequestLogging encontrará o DiagnosticContext com sucesso:
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseHttpMetrics();
app.UseAuthorization();
app.MapControllers();
app.MapMetrics();

app.Run();