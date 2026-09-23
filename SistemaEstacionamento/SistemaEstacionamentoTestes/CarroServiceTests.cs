using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaEstacionamento.Data;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SistemaEstacionamento
{
    public class CarroServiceTests : IDisposable
    {
        private readonly DataContext _context;
        private readonly Mock<IServiceRegistro> _serviceRegistroMock;
        private readonly CarroService _service;
        private readonly Mock<ILogger<CarroService>> _loggerMock;

        public CarroServiceTests()
        {
            // Configura um banco InMemory com nome único para isolamento entre testes
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _serviceRegistroMock = new Mock<IServiceRegistro>();
            _loggerMock = new Mock<ILogger<CarroService>>();

            _service = new CarroService(_context, _serviceRegistroMock.Object, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region ObterTodosAsync

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarApenasCarrosNaoExcluidos()
        {
            // Arrange
            var carros = new List<Carro>
            {
                new Carro { Id = 1, Modelo = "Civic", Excluido = false },
                new Carro { Id = 2, Modelo = "Corolla", Excluido = true },
                new Carro { Id = 3, Modelo = "Golf", Excluido = false }
            };

            await _context.Carros.AddRangeAsync(carros);
            await _context.SaveChangesAsync();

            // Act
            var resultado = (await _service.ObterTodosAsync()).ToList();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.DoesNotContain(resultado, c => c.Excluido);
            Assert.Contains(resultado, c => c.Id == 1);
            Assert.Contains(resultado, c => c.Id == 3);
        }

        #endregion

        #region ObterPorIdAsync

        [Fact]
        public async Task ObterPorIdAsync_QuandoCarroExisteENaoEstaExcluido_DeveRetornarCarro()
        {
            // Arrange
            var carro = new Carro { Id = 10, Modelo = "Onix", Excluido = false };
            await _context.Carros.AddAsync(carro);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _service.ObterPorIdAsync(10);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Onix", resultado.Modelo);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoCarroEstaExcluido_DeveRetornarNull()
        {
            // Arrange
            var carro = new Carro { Id = 20, Modelo = "HB20", Excluido = true };
            await _context.Carros.AddAsync(carro);
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _service.ObterPorIdAsync(20);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoCarroNaoExiste_DeveRetornarNull()
        {
            // Act
            var resultado = await _service.ObterPorIdAsync(999);

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region AdicionarAsync

        [Fact]
        public async Task AdicionarAsync_DeveGarantirExcluidoFalsoESalvarNoBanco()
        {
            // Arrange
            var novoCarro = new Carro { Id = 1, Modelo = "Polo", Excluido = true }; // Passado como true propositalmente

            // Act
            var resultado = await _service.AdicionarAsync(novoCarro);

            // Assert
            Assert.False(resultado.Excluido);

            var carroSalvo = await _context.Carros.FindAsync(1);
            Assert.NotNull(carroSalvo);
            Assert.False(carroSalvo.Excluido);
        }

        #endregion

        #region AtualizarAsync

        [Fact]
        public async Task AtualizarAsync_QuandoCarroExisteENaoEstaExcluido_DeveAtualizarComSucesso()
        {
            // Arrange
            var carroOriginal = new Carro { Id = 5, Modelo = "Compass", Excluido = false };
            await _context.Carros.AddAsync(carroOriginal);
            await _context.SaveChangesAsync();
            _context.Entry(carroOriginal).State = EntityState.Detached;

            var carroModificado = new Carro { Id = 5, Modelo = "Compass Atualizado", Excluido = false };

            // Act
            await _service.AtualizarAsync(carroModificado);

            // Assert
            var carroDoBanco = await _context.Carros.FindAsync(5);
            Assert.Equal("Compass Atualizado", carroDoBanco.Modelo);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoCarroEstaExcluido_DeveLancarException()
        {
            // Arrange
            var carroExcluido = new Carro { Id = 6, Modelo = "Renegade", Excluido = true };
            await _context.Carros.AddAsync(carroExcluido);
            await _context.SaveChangesAsync();

            var carroParaAtualizar = new Carro { Id = 6, Modelo = "Renegade Modificado" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AtualizarAsync(carroParaAtualizar));
            Assert.Equal("Carro não encontrado ou excluído.", ex.Message);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoCarroNaoExiste_DeveLancarException()
        {
            // Arrange
            var carroInexistente = new Carro { Id = 99, Modelo = "Fantasma" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AtualizarAsync(carroInexistente));
            Assert.Equal("Carro não encontrado ou excluído.", ex.Message);
        }

        #endregion

        #region ExcluirAsync

        [Fact]
        public async Task ExcluirAsync_QuandoRegistroExiste_DeveCalcularValorAtualizarRegistroEEfetuarSoftDelete()
        {
            // Arrange
            int carroId = 1;
            var carro = new Carro { Id = carroId, Modelo = "Cruze", Excluido = false };
            await _context.Carros.AddAsync(carro);
            await _context.SaveChangesAsync();

            var registro = new Registro
            {
                Id = 100,
                IdCarro = carroId,
                DataEntrada = DateTime.Now.AddHours(-2.5) // 2.5 horas = cobra 3 horas (R$ 30,00)
            };

            _serviceRegistroMock
                .Setup(r => r.ObterCarroAsync(carroId))
                .ReturnsAsync(registro);

            _serviceRegistroMock
                .Setup(r => r.AtualizarAsync(It.IsAny<Registro>()))
                .Returns(Task.CompletedTask);

            _serviceRegistroMock
                .Setup(r => r.ExcluirAsync(registro.Id))
                .Returns(Task.CompletedTask);

            // Act
            await _service.ExcluirAsync(carroId);

            // Assert
            // 1. Verifica se o cálculo de cobrança foi executado corretamente (teto de 2.5h = 3h * 10.00m = 30.00m)
            Assert.Equal(30.00m, registro.ValorPago);
            Assert.NotNull(registro.DataSaida);

            // 2. Garante que os métodos do serviço de registro foram acionados
            _serviceRegistroMock.Verify(r => r.AtualizarAsync(It.Is<Registro>(reg => reg.ValorPago == 30.00m)), Times.Once);
            _serviceRegistroMock.Verify(r => r.ExcluirAsync(registro.Id), Times.Once);

            // 3. Garante o soft delete do carro
            var carroNoBanco = await _context.Carros.FindAsync(carroId);
            Assert.True(carroNoBanco.Excluido);
        }

        [Fact]
        public async Task ExcluirAsync_QuandoMenosDeUmaHora_DeveCobrarPisoDeUmaHora()
        {
            // Arrange
            int carroId = 2;
            var carro = new Carro { Id = carroId, Modelo = "Mobi", Excluido = false };
            await _context.Carros.AddAsync(carro);
            await _context.SaveChangesAsync();

            var registro = new Registro
            {
                Id = 101,
                IdCarro = carroId,
                DataEntrada = DateTime.Now.AddMinutes(-20) // 20 min = cobra no mínimo 1 hora (R$ 10,00)
            };

            _serviceRegistroMock
                .Setup(r => r.ObterCarroAsync(carroId))
                .ReturnsAsync(registro);

            // Act
            await _service.ExcluirAsync(carroId);

            // Assert
            Assert.Equal(10.00m, registro.ValorPago);
            _serviceRegistroMock.Verify(r => r.AtualizarAsync(registro), Times.Once);
        }

        [Fact]
        public async Task ExcluirAsync_QuandoNaoTemRegistroCadastrado_DeveLancarExceptionENaoExcluirCarro()
        {
            // Arrange
            int carroId = 3;
            var carro = new Carro { Id = carroId, Modelo = "Tracker", Excluido = false };
            await _context.Carros.AddAsync(carro);
            await _context.SaveChangesAsync();

            _serviceRegistroMock
                .Setup(r => r.ObterCarroAsync(carroId))
                .ReturnsAsync((Registro)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.ExcluirAsync(carroId));
            Assert.Equal("Não tem registro cadastrado para o carro Tracker", ex.Message);

            // Garante que o carro continuou ativo
            var carroNoBanco = await _context.Carros.FindAsync(carroId);
            Assert.False(carroNoBanco.Excluido);

            // Garante que operações de encerramento de registro não foram chamadas
            _serviceRegistroMock.Verify(r => r.AtualizarAsync(It.IsAny<Registro>()), Times.Never);
            _serviceRegistroMock.Verify(r => r.ExcluirAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion
    }
}