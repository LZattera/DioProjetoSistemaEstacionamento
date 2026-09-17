using Microsoft.AspNetCore.Mvc;
using SistemaEstacionamento.Features.Carro;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Registro
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerRegistro: ControllerBase
    {
        private readonly IServiceRegistro serviceRegistro;

        public ControllerRegistro(IServiceRegistro _serviceRegistro)
        {
            serviceRegistro = _serviceRegistro;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var lista = await serviceRegistro.ObterTodosAsync();
            return Ok(lista);
        }

        [HttpGet("/{id}")]
        public async Task<ActionResult> GetbyId(int id)
        {
            var objeto = await serviceRegistro.ObterPorIdAsync(id);
            return Ok(objeto);
        }

        [HttpPut]
        public async Task<ActionResult> RegistrarEntrada([FromBody] Registro registro)
        {
            var resposta = await serviceRegistro.RegistrarEntradaAsync(registro);
            return Ok(resposta);
        }

        [HttpPut]
        public async Task<ActionResult> RegistrarSaida(int id, decimal valorPago, DateTime? dataSaida)
        {
            var resposta = serviceRegistro.RegistrarSaidaAsync(id, valorPago, dataSaida);
            return Ok(resposta);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var resposta = serviceRegistro.ExcluirAsync(id);
            return Ok(resposta);
        }
    }
}