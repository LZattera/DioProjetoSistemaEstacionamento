Neste Projeto de LAB você será desafiado a construir um sistema para um estacionamento, que será usado para gerenciar os veículos estacionados e realizar suas operações, como por exemplo
adicionar um veículo, 
remover um veículo (e exibir o valor cobrado durante o período) e 
listar os veículos.

Projeto terá:
openAPI
Testes - ok
System Design - Package by Feature - ok
EF Entity Framework para ORM - ok
Migrations
BD Mysql
Logs com Serilog - ok
Observabilidade do sistema com Prometheus e Grafana

Parei aqui: 
Inicie os containers pelo terminal:

Bash
docker compose up -d
5. Conectar o Grafana ao Prometheus
Acesse o Grafana no navegador: http://localhost:3000 (usuário padrão: admin, senha: admin).

Vá em Connections > Data Sources > Add data source.

Selecione Prometheus.

No campo Prometheus server URL, insira:

Plaintext
http://prometheus:9090
Role até o final e clique em Save & Test (deve exibir uma confirmação verde).

6. Importar um Dashboard pronto para ASP.NET Core
Em vez de montar gráficos do zero, você pode importar um dashboard pronto para o runtime .NET:

No Grafana, vá em Dashboards > New > Import.

No campo Find and import dashboards through grafana.com, digite o ID: 10915 e clique em Load.

Selecione a fonte de dados Prometheus criada no passo anterior e clique em Import.

O painel exibirá automaticamente taxas de requisições, tempo de resposta, uso de CPU, memória e threads do Garbage Collector.

7. Criando métricas personalizadas de negócio (Opcional)
Para monitorar dados reais da sua aplicação (como carros atualmente estacionados e total arrecadado):

C#
using Prometheus;

public static class MetricasEstacionamento
{
    // Medidor que sobe e desce (carros atualmente no pátio)
    public static readonly Gauge CarrosEstacionados = Metrics
        .CreateGauge("estacionamento_carros_ativos_total", "Quantidade atual de veículos no estacionamento");

    // Contador acumulativo que só sobe (faturamento total)
    public static readonly Counter TotalArrecadado = Metrics
        .CreateCounter("estacionamento_arrecadado_reais_total", "Valor total já arrecadado pelo estacionamento");
}
Usando dentro do seu RegistroService:

C#
public async Task<Registro> RegistrarEntradaAsync(Registro registro)
{
    // ... lógica de entrada
    MetricasEstacionamento.CarrosEstacionados.Inc(); // Incrementa em 1
    return registro;
}

public async Task RegistrarSaidaAsync(int id, decimal valorPago, DateTime? dataSaida = null)
{
    // ... lógica de saída
    MetricasEstacionamento.CarrosEstacionados.Dec(); // Decrementa em 1
    MetricasEstacionamento.TotalArrecadado.Inc((double)valorPago); // Soma valor arrecadado
}
