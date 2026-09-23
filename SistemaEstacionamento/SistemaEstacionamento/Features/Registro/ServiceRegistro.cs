using Microsoft.EntityFrameworkCore;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;


namespace SistemaEstacionamento.Features.Registro
{
    public interface IServiceRegistro
    {
        Task<IEnumerable<Registro>> ObterTodosAsync();
        Task<IEnumerable<Registro>> ObterAbertosAsync(); 
        Task<Registro> ObterPorIdAsync(int id);
        Task<Registro> RegistrarEntradaAsync(Registro registro);
        Task RegistrarSaidaAsync(int id, decimal valorPago, DateTime? dataSaida = null);
        Task AtualizarAsync(Registro registro);
        Task ExcluirAsync(int id);
        Task<Registro> ObterCarroAsync(int idCarro);
    }
    public class RegistroService : IServiceRegistro
    {
        private readonly DataContext _context;
        private readonly ILogger<RegistroService> _logger;

        public RegistroService(DataContext context, ILogger<RegistroService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Registro>> ObterTodosAsync()
        {
            return await _context.Registros
                .Where(r => !r.Excluido)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Registro> ObterCarroAsync(int idCarro)
        {
            return await _context.Registros
                .Where(r => !r.Excluido && r.IdCarro==idCarro)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // Método extra: Traz apenas os veículos que ainda não saíram
        public async Task<IEnumerable<Registro>> ObterAbertosAsync()
        {
            return await _context.Registros
                .Where(r => !r.Excluido && r.DataSaida == null)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Registro> ObterPorIdAsync(int id)
        {
            return await _context.Registros
                .FirstOrDefaultAsync(r => r.Id == id && !r.Excluido);
        }

        public async Task<Registro> RegistrarEntradaAsync(Registro registro)
        {
            registro.Excluido = false;

            // Garante que não tenha data de saída e valor pago na entrada
            registro.DataSaida = null;
            registro.ValorPago = 0;

            // Se a data de entrada não foi enviada, usa a hora atual
            if (registro.DataEntrada == default)
            {
                registro.DataEntrada = DateTime.Now;
            }

            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Registro {Id} cadastrado com sucesso", registro.Id);
            return registro;
        }

        // Método específico de negócio para fechar a conta do estacionamento
        public async Task RegistrarSaidaAsync(int id, decimal valorPago, DateTime? dataSaida = null)
        {
            var registro = await _context.Registros.FirstOrDefaultAsync(r => r.Id == id && !r.Excluido);

            if (registro == null)
                throw new Exception("Registro não encontrado ou excluído.");

            if (registro.DataSaida != null)
                throw new Exception("Este registro já foi finalizado anteriormente.");

            registro.DataSaida = dataSaida ?? DateTime.Now;
            registro.ValorPago = valorPago;

            _context.Registros.Update(registro);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Registro {Id} atualizado com sucesso", registro.Id);
        }

        public async Task AtualizarAsync(Registro registro)
        {
            var existe = await _context.Registros.AnyAsync(r => r.Id == registro.Id && !r.Excluido);
            if (!existe)
                throw new Exception("Registro não encontrado ou excluído.");

            _context.Registros.Update(registro);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Registro {Id} atualizado com sucesso", registro.Id);
        }

        public async Task ExcluirAsync(int id)
        {
            var registro = await _context.Registros.FindAsync(id);

            if (registro != null && !registro.Excluido)
            {
                // Aplica o Soft Delete
                registro.Excluido = true;
                _logger.LogInformation("Registro {Id} excluído com sucesso", registro.Id);

                _context.Registros.Update(registro);
                await _context.SaveChangesAsync();
            }
        }
    }
}