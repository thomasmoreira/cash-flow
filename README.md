# CashFlow
> O CashFlow é uma solução de controle de fluxo de caixa destinada a ajudar comerciantes a gerenciar seus lançamentos financeiros (débito e crédito) e a obter relatórios consolidados do saldo diário. A aplicação adota uma arquitetura baseada em microsserviços, separando as operações de escrita (Transactions) e de leitura/consolidação (Consolidation), e utiliza um API Gateway para centralizar as requisições, autenticação e a documentação da API.

![](diagrama.png)

## Descrição

A solução é composta por três componentes principais:

Serviço de Lançamentos (Transactions):
Recebe os dados dos lançamentos (data, tipo, valor, descrição), valida os comandos utilizando FluentValidation, registra as transações no banco de dados e publica eventos via MassTransit para notificar o serviço de consolidação.

Serviço de Consolidação (Consolidation):
Atua como consumidor dos eventos gerados pelo serviço de Lançamentos, processando os lançamentos e atualizando o saldo diário consolidado. Este serviço expõe endpoints para consulta do saldo consolidado.

API Gateway:
Funciona como porta de entrada unificada para a solução. Ele autentica as requisições com tokens JWT, encaminha as chamadas para os serviços internos e unifica a documentação via Swagger/OpenAPI. Além disso, o gateway gerencia as configurações de endpoints por ambiente, propagando os tokens de autenticação para os demais serviços.
## Tecnologias Utilizadas

.NET 9 / ASP.NET Core Minimal APIs:
Plataforma para construção dos microsserviços e API Gateway.

Entity Framework Core:
Acesso e persistência de dados com PostgreSQL.

PostgreSQL:
Banco de dados relacional.

MassTransit & RabbitMQ:
Implementação de mensageria para comunicação assíncrona entre os serviços.

Serilog & Seq:
Log centralizado e monitoramento dos logs.

FluentValidation:
Validação dos comandos e requisições.

Polly:
Implementação de políticas de retry e circuit breaker para aumentar a resiliência das chamadas HTTP.

Docker & Docker Compose:
Containerização e orquestração dos microsserviços.

MediatR:
Implementação do padrão CQRS para separar operações de escrita (commands) e leitura (queries).

xUnit, Moq, FluentAssertions:
Frameworks para testes unitários e de integração.

## Decisões Arquiteturais

Microsserviços e API Gateway:
A aplicação foi dividida em serviços independentes (Transactions e Consolidation) que se comunicam de forma assíncrona via MassTransit. O API Gateway centraliza a entrada de requisições, gerencia a autenticação (JWT) e encaminha as chamadas para os serviços correspondentes.

CQRS e DDD:
Adotamos o padrão CQRS para separar os casos de uso de escrita e leitura. As regras de negócio e os contratos (interfaces) foram definidos na camada Core (Domínio), enquanto as implementações que interagem com recursos externos (banco, mensageria, etc.) foram implementadas na camada de Infraestrutura.

Mensageria Assíncrona:
Utilizamos MassTransit com RabbitMQ para desacoplar a comunicação entre os serviços, garantindo que o serviço de consolidação processe os eventos de transação de forma resiliente.

Resiliência e Monitoramento:
Políticas de retry e circuit breaker foram implementadas (com Polly) para lidar com falhas transitórias nas chamadas HTTP. O Serilog, integrado ao Seq, possibilita o monitoramento centralizado dos logs.

Configurações Dinâmicas por Ambiente:
As URLs dos serviços, chaves JWT e outras configurações são centralizadas e podem ser sobrescritas via variáveis de ambiente. Isso permite que a aplicação funcione corretamente tanto em ambiente local (usando localhost) quanto em ambientes de container (usando os nomes dos serviços no Docker Compose).

## Release History

* 0.2.1
    * CHANGE: Update docs (module code remains unchanged)
* 0.2.0
    * CHANGE: Remove `setDefaultXYZ()`
    * ADD: Add `init()`
* 0.1.1
    * FIX: Crash when calling `baz()` (Thanks @GenerousContributorName!)
* 0.1.0
    * The first proper release
    * CHANGE: Rename `foo()` to `bar()`
* 0.0.1
    * Work in progress

## Meta

Your Name – [@YourTwitter](https://twitter.com/dbader_org) – YourEmail@example.com

Distributed under the XYZ license. See ``LICENSE`` for more information.

[https://github.com/yourname/github-link](https://github.com/dbader/)

## Contributing

1. Fork it (<https://github.com/yourname/yourproject/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

<!-- Markdown link & img dfn's -->
[npm-image]: https://img.shields.io/npm/v/datadog-metrics.svg?style=flat-square
[npm-url]: https://npmjs.org/package/datadog-metrics
[npm-downloads]: https://img.shields.io/npm/dm/datadog-metrics.svg?style=flat-square
[travis-image]: https://img.shields.io/travis/dbader/node-datadog-metrics/master.svg?style=flat-square
[travis-url]: https://travis-ci.org/dbader/node-datadog-metrics
[wiki]: https://github.com/yourname/yourproject/wiki
