let clientes = [];

document.addEventListener("DOMContentLoaded", () => {

    const btnNovoCliente =
        document.getElementById("btnNovoCliente");

    const btnCancelarCliente =
        document.getElementById("btnCancelarCliente");

    const formCliente =
        document.getElementById("formCliente");

    if (btnNovoCliente) {
        btnNovoCliente.addEventListener("click", () => {

            document
                .getElementById("formularioCliente")
                .classList.remove("oculto");

        });
    }

    if (btnCancelarCliente) {
        btnCancelarCliente.addEventListener("click", () => {

            formCliente.reset();

            document
                .getElementById("formularioCliente")
                .classList.add("oculto");

        });
    }

    if (formCliente) {
        formCliente.addEventListener(
            "submit",
            cadastrarCliente
        );
    }

    carregarClientes();
});

async function carregarClientes() {

    try {

        const dados = await requisicao("/clientes");

        clientes = obterLista(dados);

        renderizarClientes(clientes);

    } catch (erro) {

        renderizarClientes([]);

        mostrarMensagem(
            `Não foi possível carregar os clientes: ${erro.message}`,
            "erro"
        );
    }
}

function obterNomeCliente(cliente) {
    return cliente?.nome ??
        cliente?.Nome ??
        cliente?.nomeCompleto ??
        "Sem nome";
}

function obterTelefoneCliente(cliente) {
    return cliente?.telefone ??
        cliente?.Telefone ??
        cliente?.celular ??
        "-";
}

function obterEnderecoCliente(cliente) {
    return cliente?.endereco ??
        cliente?.Endereco ??
        cliente?.enderecoCompleto ??
        "-";
}

function renderizarClientes(lista) {

    const tabela =
        document.getElementById("tabelaClientes");

    if (!tabela) {
        return;
    }

    if (!lista.length) {

        tabela.innerHTML = `
            <tr>
                <td colspan="3">
                    Nenhum cliente cadastrado.
                </td>
            </tr>
        `;

        return;
    }

    tabela.innerHTML = lista.map(cliente => `
        <tr>
            <td>${obterNomeCliente(cliente)}</td>
            <td>${obterTelefoneCliente(cliente)}</td>
            <td>${obterEnderecoCliente(cliente)}</td>
        </tr>
    `).join("");
}

async function cadastrarCliente(evento) {

    evento.preventDefault();

    const nome =
        document.getElementById("nomeCliente").value.trim();

    const telefone =
        document.getElementById("telefoneCliente").value.trim();

    const endereco =
        document.getElementById("enderecoCliente").value.trim();

    if (!nome || !telefone || !endereco) {

        mostrarMensagem(
            "Preencha todos os campos do cliente.",
            "erro"
        );

        return;
    }

    const dados = {
        nome: nome,
        telefone: telefone,
        endereco: endereco
    };

    try {

        await requisicao("/clientes", {
            method: "POST",
            body: JSON.stringify(dados)
        });

        mostrarMensagem(
            "Cliente cadastrado com sucesso.",
            "sucesso"
        );

        document
            .getElementById("formCliente")
            .reset();

        document
            .getElementById("formularioCliente")
            .classList.add("oculto");

        carregarClientes();

    } catch (erro) {

        mostrarMensagem(
            `Não foi possível cadastrar o cliente: ${erro.message}`,
            "erro"
        );
    }
}