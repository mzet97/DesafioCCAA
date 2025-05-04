dockerfile run docker build -f src/DesafioCCAA/DesafioCCAA.Api/Dockerfile -t desafioccaa-api:latest .

# Desafio CCAA

## Visão Geral do Projeto
Este projeto é um sistema de catálogo de livros na web, desenvolvido com uma arquitetura em camadas. O objetivo principal é permitir que os usuários criem contas, cadastrem e gerenciem livros, além de emitir relatórios em PDF.

### Tecnologias Utilizadas
- **Front-end**: MVC
- **Back-end**: .NET Core 8+ com Web API REST em C#
- **Banco de Dados**: SQL Server
- **ORM**: Entity Framework Core (Code First)
- **Testes**: X-Unit
- **Autenticação**: JWT
- **Outras Tecnologias**: Automapper, FluentValidation, Logging, Redis, UnitOfWork, CQRS

---

## Requisitos Necessários
### Pré-requisitos
Certifique-se de ter as seguintes ferramentas instaladas no seu ambiente:
- **Git**: Para clonar o repositório.
- **Docker e Docker Compose**: Para rodar os containers.
- **.NET Core SDK 8+**: Para compilar e executar a API.
- **SQL Server**: Caso opte por um banco de dados local fora do Docker.

---

## Passos para Rodar o Projeto

### 1. Clone o Repositório
Abra o terminal e execute o seguinte comando para clonar o repositório:
```bash
git clone https://github.com/mzet97/DesafioCCAA.git
cd DesafioCCAA
```

-----

### 2\. Configuração do Banco de Dados

  - Caso utilize o Docker, o banco de dados estará configurado automaticamente com o `docker-compose`.
  - Caso opte por um banco local, configure a string de conexão no arquivo de configuração da API (appsettings.json) com as credenciais do seu ambiente SQL Server.

-----

### 3\. Rodar o Docker Compose

Certifique-se de que o Docker está em execução e execute o seguinte comando na raiz do projeto:

```bash
docker-compose up --build
```

Isso iniciará os containers necessários, incluindo o banco de dados e outros serviços dependentes.

-----

### 4\. Rodar a API

1.  Navegue até a pasta da API:
    ```bash
    cd src/DesafioCCAA/DesafioCCAA.Api
    ```
2.  Restaure os pacotes do projeto:
    ```bash
    dotnet restore
    ```
3.  Execute a API:
    ```bash
    dotnet run
    ```
4.  A API estará disponível no endereço:
    ```
    http://localhost:5120
    ```

-----

### 5\. Rodar o Front-end

1.  Navegue até a pasta do front-end MVC:
    ```bash
    cd src/DesafioCCAA/DesafioCCAA.Web
    ```
2.   Restaure os pacotes do projeto:
    ```bash
    dotnet restore
    ```
3.  Execute a WEB:
    ```bash
    dotnet run
    ```
4.  Acesse o front-end pelo endereço:
    ```
    http://localhost:5200
    ```

-----

## Fluxos Principais do Sistema

### 1\. Inclusão de Livro

  - Faça login no sistema.
  - Navegue até a seção de "Catálogo de Livros".
  - Clique em "Adicionar Livro" e preencha os campos obrigatórios:
      - Título
      - Nº ISBN
      - Gênero
      - Autor
      - Editora
      - Sinopse
      - Foto do Livro (opcional)
  - Salve o cadastro.

### 2\. Emissão de Relatório

  - Após o login, acesse a seção de "Lista de livros".
  - Clique em "Gerar PDF" e o sistema exibirá um PDF com os livros cadastrados pelo usuário logado.

-----

## Testes

### Testes Unitários e de Integração

1.  Navegue até a pasta de testes:
    ```bash
    cd tests/DesafioCCAA.Tests
    ```
2.  Execute os testes com o comando:
    ```bash
    dotnet test
    ```

-----

## Ferramentas Disponíveis

  - **Swagger**: Acesse a documentação da API em:
    ```
    http://localhost:5000/swagger
    ```
  - **Coleção Postman**: Uma coleção de requisições está disponível na pasta `docs/postman_collection.json`.

-----

## Contribuição

Contribuições são bem-vindas\! Siga as boas práticas de desenvolvimento e abra uma pull request no repositório.

-----

## Licença

Este projeto está licenciado sob a licença [MIT](https://www.google.com/search?q=LICENSE).

-----
