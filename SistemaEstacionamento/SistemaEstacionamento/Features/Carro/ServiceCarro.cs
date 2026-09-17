using Microsoft.EntityFrameworkCore;
using SistemaEstacionamento.Data;
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
