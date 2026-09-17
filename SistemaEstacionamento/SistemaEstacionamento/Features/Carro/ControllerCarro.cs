using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SistemaEstacionamento.Features.Carro;

namespace SistemaEstacionamento.Features.Carro
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerCarro : ControllerBase
    {
        private readonly IServiceCarro serviceCarro;

        public ControllerCarro(IServiceCarro _serviceCarro)
        {
            serviceCarro = _serviceCarro;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var lista = await serviceCarro.ObterTodosAsync();
            return Ok(lista);
        }

        [HttpGet("/{id}")]
        public async Task<ActionResult> GetbyId(int id)
        {
            var objeto = await serviceCarro.ObterPorIdAsync(id);
            return Ok(objeto);
        }

        [HttpPut]
        public async Task<ActionResult> Save([FromBody] Carro carro)
        {
            var resposta = await serviceCarro.AdicionarAsync(carro);
            return Ok(resposta);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var resposta = serviceCarro.ExcluirAsync(id);
            return Ok(resposta);
        }
    }
}
