using Microsoft.EntityFrameworkCore;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. CONFIGURAÇÃO DO BANCO DE DADOS (ENTITY FRAMEWORK)
// ============================================================
// Exemplo usando SQL Server. Se for usar outro banco (como SQLite), troque para UseSqlite()
//builder.Services.AddDbContext<DataContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Adicione esta configuração
builder.Services.AddDbContext<DataContext>(options =>
    options.UseInMemoryDatabase("EstacionamentoDbMemory"));


// ============================================================
// 2. INJEÇÃO DE DEPENDÊNCIA (SERVICES)
// ============================================================
// AddScoped garante que uma nova instância do Service seja criada por requisição HTTP
builder.Services.AddScoped<IServiceCarro, CarroService>();
builder.Services.AddScoped<IServiceRegistro, RegistroService>();


// ============================================================
// 3. CONFIGURAÇÕES DA API E SWAGGER
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline de requisições HTTP
Console.WriteLine("Envirom,ent");
Console.WriteLine(app.Environment.IsDevelopment());
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();