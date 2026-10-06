# DeliveryApp - Sistema de Gestão de Delivery

Sistema completo para gerenciamento de pedidos, cardápio gastronômico, controle de entregadores e clientes, cálculo dinâmico de fretes e relatórios de faturamento.

---

## 🚀 Pré-requisitos

Antes de começar, certifique-se de ter instalado em seu computador:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (versão 8.0 ou superior)
- Um navegador web moderno (Google Chrome, Microsoft Edge, Firefox, etc.)
- **Git** instalado no terminal

---

## 🛠️ Como rodar o projeto em um novo computador

### 1. Clonar o repositório
Abra o terminal (Git Bash, CMD ou PowerShell) e execute:

```bash
git clone https://github.com/Fausto5667/DeliveryApp.git
cd DeliveryApp
```

### 2. Executar a API (.NET 8)
Navegue até a pasta do backend e inicie a aplicação:

```bash
cd DeliveryApi-main
dotnet run
```

O terminal exibirá a inicialização da API, rodando por padrão em:
`http://localhost:5173` (ou na porta configurada no terminal).

### 3. Acessar o Frontend
Com o backend em execução:
- Basta abrir o arquivo `frontend/index.html` diretamente no seu navegador, ou acessar `http://localhost:5173/index.html`.
- Para visualizar e testar os endpoints da API via Swagger: acesse `http://localhost:5173/swagger`.

---

## 📁 Estrutura do Projeto

- **`DeliveryApi-main/`**: Backend em .NET 8 Web API
  - `Controllers/`: Endpoints da API REST
  - `Dominio/`: Entidades de negócio (Pratos, Bebidas, Sobremesas, Clientes, Entregadores, Pedidos) e regras de frete
  - `Dtos/`: Objetos de transferência de dados (Data Transfer Objects)
  - `Servicos/`: Regras de negócio e orquestração
  - `Repositorios/`: Armazenamento em memória thread-safe
- **`frontend/`**: Interface web responsiva
  - `index.html`: Painel inicial / Dashboard
  - `cardapio.html`: Gestão de itens e categorias do cardápio
  - `clientes.html`: Cadastro e listagem de clientes
  - `entregadores.html`: Gerenciamento da frota de entregadores
  - `pedidos.html`: Abertura, cálculo de frete e avanço de status
  - `relatorios.html`: Métricas de vendas, faturamento e pratos mais vendidos
  - `css/style.css`: Folha de estilos responsiva com suporte a tema claro e escuro
  - `js/`: Scripts de integração com a API
