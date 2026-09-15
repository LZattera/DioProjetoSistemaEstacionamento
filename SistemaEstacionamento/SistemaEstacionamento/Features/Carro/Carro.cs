using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Carro
{
    public class Carro
    {
        public Guid Id { get; set; }
        public string Modelo { get; set; }
        public bool Excluido { get; set; }

    }
}
