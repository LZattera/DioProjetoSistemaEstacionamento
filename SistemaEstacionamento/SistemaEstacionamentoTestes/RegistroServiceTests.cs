using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SistemaEstacionamento
{
    public class RegistroServiceTests : IDisposable
    {
        private readonly DataContext _context;
        private readonly RegistroService _service;
        private readonly Mock<ILogger<RegistroService>> _loggerMock;


        public RegistroServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _loggerMock = new Mock<ILogger<RegistroService>>();

            _service = new RegistroService(_context, _loggerMock.Object);

        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarApenasRegistrosNaoExcluidos()
        {
            // Arrange
            var registros = new List<SistemaEstacionamento.Features.Registro.Registro>
            {
                new() { Id = 1, IdCarro = 10, Excluido = false },
                new() { Id = 2, IdCarro = 20, Excluido = true },
                new() { Id = 3, IdCarro = 30, Excluido = false }
            };

            await _context.Registros.AddRangeAsync(registros);
            await _context.SaveChangesAsync();

            // Act
            var resultado = (await _service.ObterTodosAsync()).ToList();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.DoesNotContain(resultado, r => r.Excluido);
            Assert.Contains(resultado, r => r.Id == 1);
            Assert.Contains(resultado, r => r.Id == 3);
        }

        #endregion

        #region ObterCarroAsync

        [Fact]
        public async Task ObterCarroAsync_QuandoExisteRegistroAtivoParaOCarro_DeveRetornarRegistro()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro { Id = 1, IdCarro = 10, Excluido = false };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _service.ObterCarroAsync(10);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.Id);
            Assert.Equal(10, resultado.IdCarro);
        }

        [Fact]
        public async Task ObterCarroAsync_QuandoRegistroDoCarroEstaExcluido_DeveRetornarNull()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro { Id = 2, IdCarro = 20, Excluido = true };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _service.ObterCarroAsync(20);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterCarroAsync_QuandoCarroNaoPossuiRegistro_DeveRetornarNull()
        {
            // Act
            var resultado = await _service.ObterCarroAsync(999);

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region ObterAbertosAsync

        [Fact]
        public async Task ObterAbertosAsync_DeveRetornarApenasRegistrosSemDataSaidaENaoExcluidos()
        {
            // Arrange
            var registros = new List<SistemaEstacionamento.Features.Registro.Registro>
            {
                new() { Id = 1, IdCarro = 1, Excluido = false, DataSaida = null },              // Aberto válido
                new() { Id = 2, IdCarro = 2, Excluido = false, DataSaida = DateTime.Now },     // Já finalizado
                new() { Id = 3, IdCarro = 3, Excluido = true,  DataSaida = null },              // Excluído sem saída
                new() { Id = 4, IdCarro = 4, Excluido = false, DataSaida = null }               // Aberto válido
            };

            await _context.Registros.AddRangeAsync(registros);
            await _context.SaveChangesAsync();

            // Act
            var resultado = (await _service.ObterAbertosAsync()).ToList();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.All(resultado, r =>
            {
                Assert.False(r.Excluido);
                Assert.Null(r.DataSaida);
            });
            Assert.Contains(resultado, r => r.Id == 1);
            Assert.Contains(resultado, r => r.Id == 4);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_QuandoExisteENaoEstaExcluido_DeveRetornarRegistro()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro { Id = 5, IdCarro = 10, Excluido = false };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _service.ObterPorIdAsync(5);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(5, resultado.Id);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoEstaExcluido_DeveRetornarNull()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro { Id = 6, IdCarro = 10, Excluido = true };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _service.ObterPorIdAsync(6);

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region RegistrarEntradaAsync

        [Fact]
        public async Task RegistrarEntradaAsync_QuandoDataEntradaNaoInformada_DeveAtribuirDataAtualEInicializarCampos()
        {
            // Arrange
            var dataAntes = DateTime.Now.AddSeconds(-1);
            var novoRegistro = new SistemaEstacionamento.Features.Registro.Registro
            {
                IdCarro = 15,
                Excluido = true,               // Deve ser forçado a false
                ValorPago = 50.00m,            // Deve ser zerado
                DataSaida = DateTime.Now       // Deve ser forçado a null
            };

            // Act
            var resultado = await _service.RegistrarEntradaAsync(novoRegistro);
            var dataDepois = DateTime.Now.AddSeconds(1);

            // Assert
            Assert.False(resultado.Excluido);
            Assert.Null(resultado.DataSaida);
            Assert.Equal(0, resultado.ValorPago);
            Assert.InRange(resultado.DataEntrada, dataAntes, dataDepois);

            var registroSalvo = await _context.Registros.FindAsync(resultado.Id);
            Assert.NotNull(registroSalvo);
            Assert.False(registroSalvo.Excluido);
        }

        [Fact]
        public async Task RegistrarEntradaAsync_QuandoDataEntradaInformada_DeveManterDataInformada()
        {
            // Arrange
            var dataInformada = new DateTime(2026, 9, 23, 10, 0, 0);
            var novoRegistro = new SistemaEstacionamento.Features.Registro.Registro
            {
                IdCarro = 16,
                DataEntrada = dataInformada
            };

            // Act
            var resultado = await _service.RegistrarEntradaAsync(novoRegistro);

            // Assert
            Assert.Equal(dataInformada, resultado.DataEntrada);
            Assert.Null(resultado.DataSaida);
            Assert.Equal(0, resultado.ValorPago);
        }

        #endregion

        #region RegistrarSaidaAsync

        [Fact]
        public async Task RegistrarSaidaAsync_QuandoValidoSemDataSaidaParametro_DeveDefinirDataAtualEValorPago()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 1,
                IdCarro = 10,
                DataEntrada = DateTime.Now.AddHours(-2),
                DataSaida = null,
                Excluido = false
            };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            var dataAntes = DateTime.Now.AddSeconds(-1);

            // Act
            await _service.RegistrarSaidaAsync(1, 20.00m);
            var dataDepois = DateTime.Now.AddSeconds(1);

            // Assert
            var registroAtualizado = await _context.Registros.FindAsync(1);
            Assert.NotNull(registroAtualizado.DataSaida);
            Assert.InRange(registroAtualizado.DataSaida.Value, dataAntes, dataDepois);
            Assert.Equal(20.00m, registroAtualizado.ValorPago);
        }

        [Fact]
        public async Task RegistrarSaidaAsync_QuandoDataSaidaForFornecida_DeveUsarDataInformada()
        {
            // Arrange
            var dataSaidaCustom = new DateTime(2026, 9, 23, 18, 0, 0);
            var registro = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 2,
                IdCarro = 11,
                DataEntrada = dataSaidaCustom.AddHours(-3),
                DataSaida = null,
                Excluido = false
            };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            await _service.RegistrarSaidaAsync(2, 30.00m, dataSaidaCustom);

            // Assert
            var registroAtualizado = await _context.Registros.FindAsync(2);
            Assert.Equal(dataSaidaCustom, registroAtualizado.DataSaida);
            Assert.Equal(30.00m, registroAtualizado.ValorPago);
        }

        [Fact]
        public async Task RegistrarSaidaAsync_QuandoRegistroNaoExiste_DeveLancarException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegistrarSaidaAsync(999, 10.00m));
            Assert.Equal("Registro não encontrado ou excluído.", ex.Message);
        }

        [Fact]
        public async Task RegistrarSaidaAsync_QuandoRegistroEstaExcluido_DeveLancarException()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 3,
                IdCarro = 12,
                DataEntrada = DateTime.Now.AddHours(-1),
                Excluido = true
            };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegistrarSaidaAsync(3, 10.00m));
            Assert.Equal("Registro não encontrado ou excluído.", ex.Message);
        }

        [Fact]
        public async Task RegistrarSaidaAsync_QuandoRegistroJaFinalizado_DeveLancarException()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 4,
                IdCarro = 13,
                DataEntrada = DateTime.Now.AddHours(-2),
                DataSaida = DateTime.Now.AddHours(-1),
                ValorPago = 10.00m,
                Excluido = false
            };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegistrarSaidaAsync(4, 20.00m));
            Assert.Equal("Este registro já foi finalizado anteriormente.", ex.Message);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_QuandoRegistroExisteENaoEstaExcluido_DeveAtualizarComSucesso()
        {
            // Arrange
            var registroOriginal = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 10,
                IdCarro = 1,
                ValorPago = 10.00m,
                Excluido = false
            };
            await _context.Registros.AddAsync(registroOriginal);
            await _context.SaveChangesAsync();
            _context.Entry(registroOriginal).State = EntityState.Detached;

            var registroModificado = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 10,
                IdCarro = 1,
                ValorPago = 25.00m,
                Excluido = false
            };

            // Act
            await _service.AtualizarAsync(registroModificado);

            // Assert
            var registroDoBanco = await _context.Registros.FindAsync(10);
            Assert.Equal(25.00m, registroDoBanco.ValorPago);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoRegistroEstaExcluido_DeveLancarException()
        {
            // Arrange
            var registroExcluido = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 11,
                IdCarro = 1,
                Excluido = true
            };
            await _context.Registros.AddAsync(registroExcluido);
            await _context.SaveChangesAsync();

            var registroParaAtualizar = new SistemaEstacionamento.Features.Registro.Registro { Id = 11, IdCarro = 1 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AtualizarAsync(registroParaAtualizar));
            Assert.Equal("Registro não encontrado ou excluído.", ex.Message);
        }

        #endregion

        #region ExcluirAsync

        [Fact]
        public async Task ExcluirAsync_QuandoRegistroExisteENaoEstaExcluido_DeveAplicarSoftDelete()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 50,
                IdCarro = 10,
                Excluido = false
            };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            await _service.ExcluirAsync(50);

            // Assert
            var registroNoBanco = await _context.Registros.FindAsync(50);
            Assert.NotNull(registroNoBanco);
            Assert.True(registroNoBanco.Excluido);
        }

        [Fact]
        public async Task ExcluirAsync_QuandoRegistroNaoExiste_NaoDeveDispararErro()
        {
            // Act & Assert (não deve lançar exceção conforme regra do método)
            var exception = await Record.ExceptionAsync(() => _service.ExcluirAsync(999));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ExcluirAsync_QuandoRegistroJaExcluido_NaoDeveAlterarEstado()
        {
            // Arrange
            var registro = new SistemaEstacionamento.Features.Registro.Registro
            {
                Id = 51,
                IdCarro = 10,
                Excluido = true
            };
            await _context.Registros.AddAsync(registro);
            await _context.SaveChangesAsync();

            // Act
            await _service.ExcluirAsync(51);

            // Assert
            var registroNoBanco = await _context.Registros.FindAsync(51);
            Assert.True(registroNoBanco.Excluido);
        }

        #endregion
    }
}