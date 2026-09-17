using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Registro
{
    public class Registro
    {
        public int Id { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public decimal ValorPago { get; set; }
        public bool Excluido { get; set; }
    }
}
