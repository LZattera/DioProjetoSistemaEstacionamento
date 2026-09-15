using Microsoft.EntityFrameworkCore;
using SistemaEstacionamento.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Carro
{
    public interface ICarroService
    {
        Task<IEnumerable<Carro>> ObterTodosAsync();
        Task<Carro> ObterPorIdAsync(Guid id);
        Task<Carro> AdicionarAsync(Carro carro);
        Task AtualizarAsync(Carro carro);
        Task ExcluirAsync(Guid id); // Vai realizar o Soft Delete
    }

    public class CarroService : ICarroService
    {
        private readonly DataContext _context;

        // Injeção de dependência do Entity Framework
        public CarroService(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Carro>> ObterTodosAsync()
        {
            // Retorna apenas os carros que NÃO foram excluídos
            return await _context.Carros
                .Where(c => !c.Excluido)
                .AsNoTracking() // Otimização de leitura
                .ToListAsync();
        }

        public async Task<Carro> ObterPorIdAsync(Guid id)
        {
            // Busca o carro garantindo que ele não está excluído
            return await _context.Carros
                .FirstOrDefaultAsync(c => c.Id == id && !c.Excluido);
        }

        public async Task<Carro> AdicionarAsync(Carro carro)
        {
            carro.Id = Guid.NewGuid();
            carro.Excluido = false; // Garante que não nasça excluído

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

        public async Task ExcluirAsync(Guid id)
        {
            var carro = await _context.Carros.FindAsync(id);

            if (carro != null && !carro.Excluido)
            {
                // Soft Delete: Apenas altera a flag de exclusão
                carro.Excluido = true;

                _context.Carros.Update(carro);
                await _context.SaveChangesAsync();
            }
        }
    }

}
