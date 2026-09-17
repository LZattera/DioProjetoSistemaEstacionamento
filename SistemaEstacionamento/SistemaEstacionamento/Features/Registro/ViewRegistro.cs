using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaEstacionamento.Features.Registro
{
    internal class ViewRegistro
    {
        public int Id { get; set; }
        public int IdCarro { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
        public decimal ValorPago { get; set; }
        public bool Excluido { get; set; }
    }
}
