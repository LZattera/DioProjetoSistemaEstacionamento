using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Registro;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Carro
{
    public interface IServiceCarro
    {
        Task<IEnumerable<Carro>> ObterTodosAsync();
        Task<Carro> ObterPorIdAsync(int id);
        Task<Carro> AdicionarAsync(Carro carro);
        Task AtualizarAsync(Carro carro);
        Task ExcluirAsync(int id); // Vai realizar o Soft Delete
    }

    public class CarroService : IServiceCarro
    {
        private readonly DataContext _context;
        private readonly IServiceRegistro serviceRegistro;

        // Injeção de dependência do Entity Framework
        public CarroService(DataContext context, IServiceRegistro _serviceRegistro)
        {
            _context = context;
            serviceRegistro = _serviceRegistro;
        }

        public async Task<IEnumerable<Carro>> ObterTodosAsync()
        {
            // Retorna apenas os carros que NÃO foram excluídos
            return await _context.Carros
                .Where(c => !c.Excluido)
                .AsNoTracking() // Otimização de leitura
                .ToListAsync();
        }

        public async Task<Carro> ObterPorIdAsync(int id)
        {
            // Busca o carro garantindo que ele não está excluído
            return await _context.Carros
                .FirstOrDefaultAsync(c => c.Id == id && !c.Excluido);
        }

        public async Task<Carro> AdicionarAsync(Carro carro)
        {
            carro.Excluido = false; 

            await _context.Carros.AddAsync(carro);
            await _context.SaveChangesAsync();

            return carro;
        }

        public async Task AtualizarAsync(Carro carro)
        {
            // Opcional: Validar se o carro existe antes de atualizar
            var existe = await _context.Carros.AnyAsync(c => c.Id == carro.Id && !c.Excluido);
            if (!existe)
                throw new Exception("Carro não encontrado ou excluído.");

            _context.Carros.Update(carro);
            await _context.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            var carro = await _context.Carros.FindAsync(id);

            var registro = await serviceRegistro.ObterCarroAsync(id);
            if(registro == null)
            {
                throw new Exception($"Não tem registro cadastrado para o carro {carro.Modelo}");
            }
            else
            {
                registro.DataSaida = DateTime.Now;
                TimeSpan duracao = registro.DataSaida.Value - registro.DataEntrada;

                decimal valorHora = 10.00m;
                int horasCobradas = (int)Math.Max(1, Math.Ceiling(duracao.TotalHours));

                registro.DataSaida = registro.DataSaida;
                registro.ValorPago = horasCobradas * valorHora;

                await serviceRegistro.AtualizarAsync(registro);
                await serviceRegistro.ExcluirAsync(registro.Id);
            }

            carro.Excluido = true;
            _context.Carros.Update(carro);

            await _context.SaveChangesAsync();

        }
    }

}
