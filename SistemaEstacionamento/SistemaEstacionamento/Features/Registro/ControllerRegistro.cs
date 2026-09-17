using Microsoft.AspNetCore.Mvc;
using SistemaEstacionamento.Features.Carro;
using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Registro
{
    [ApiController]
    [Route("api/registro")]
    public class ControllerRegistro: ControllerBase
    {
        private readonly IServiceRegistro serviceRegistro;

        public ControllerRegistro(IServiceRegistro _serviceRegistro)
        {
            serviceRegistro = _serviceRegistro;
        }

        /* === FYI ===
             tipo de retorno que você usa nos métodos de um Controller para representar uma resposta HTTP.
            Em uma API, você não retorna apenas um dado cru (como uma lista de carros ou um objeto). Você precisa informar ao cliente (o frontend, o mobile ou outro sistema) 
            o status da requisição (se deu certo, se o recurso não foi encontrado, se houve erro de validação, etc.). O ActionResult serve justamente para encapsular 
            esses códigos de status HTTP junto com os dados.
         */
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var lista = await serviceRegistro.ObterTodosAsync();
            return Ok(lista);
        }

        [HttpGet("{id}")]
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

        [HttpPut("{id}/{valorPago}/{dataSaida}")]
        public async Task<ActionResult> RegistrarSaida(int id, decimal valorPago, DateTime? dataSaida)
        {
            var resposta = serviceRegistro.RegistrarSaidaAsync(id, valorPago, dataSaida);
            return Ok(resposta);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await serviceRegistro.ExcluirAsync(id);
            return Ok();
        }
    }
}