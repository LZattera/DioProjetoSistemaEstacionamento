using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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