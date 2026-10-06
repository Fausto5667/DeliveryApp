let clientes = [];

document.addEventListener("DOMContentLoaded", () => {
    const btnNovoCliente = document.getElementById("btnNovoCliente");
    const btnCancelarCliente = document.getElementById("btnCancelarCliente");
    const formCliente = document.getElementById("formCliente");

    if (btnNovoCliente) {
        btnNovoCliente.addEventListener("click", () => {
            document.getElementById("formularioCliente")?.classList.remove("oculto");
            document.getElementById("nomeCliente")?.focus();
        });
    }

    if (btnCancelarCliente) {
        btnCancelarCliente.addEventListener("click", () => {
            formCliente?.reset();
            document.getElementById("formularioCliente")?.classList.add("oculto");
        });
    }

    if (formCliente) {
        formCliente.addEventListener("submit", cadastrarCliente);
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
        mostrarMensagem(`Não foi possível carregar os clientes: ${erro.message}`, "erro");
    }
}

function obterNomeCliente(cliente) {
    return cliente?.nome ?? cliente?.Nome ?? "Sem nome";
}

function obterTelefoneCliente(cliente) {
    const raw = cliente?.telefone ?? cliente?.Telefone ?? "";
    const digitos = raw.replace(/\D/g, "");
    if (digitos.length === 11) {
        return `(${digitos.slice(0, 2)}) ${digitos.slice(2, 7)}-${digitos.slice(7)}`;
    }
    if (digitos.length === 10) {
        return `(${digitos.slice(0, 2)}) ${digitos.slice(2, 6)}-${digitos.slice(6)}`;
    }
    return raw || "-";
}

function obterEnderecoCliente(cliente) {
    return cliente?.endereco ?? cliente?.Endereco ?? "-";
}

function renderizarClientes(lista) {
    const tabela = document.getElementById("tabelaClientes");
    if (!tabela) return;

    if (!lista.length) {
        tabela.innerHTML = `
            <tr>
                <td colspan="3" style="text-align: center; color: var(--texto-secundario); padding: 24px;">
                    Nenhum cliente cadastrado ainda.
                </td>
            </tr>
        `;
        return;
    }

    tabela.innerHTML = lista.map(cliente => `
        <tr>
            <td><strong>${obterNomeCliente(cliente)}</strong></td>
            <td><code>${obterTelefoneCliente(cliente)}</code></td>
            <td>${obterEnderecoCliente(cliente)}</td>
        </tr>
    `).join("");
}

async function cadastrarCliente(evento) {
    evento.preventDefault();

    const nome = document.getElementById("nomeCliente")?.value?.trim() ?? "";
    const telefone = document.getElementById("telefoneCliente")?.value?.trim() ?? "";
    const endereco = document.getElementById("enderecoCliente")?.value?.trim() ?? "";

    if (!nome || !telefone || !endereco) {
        mostrarMensagem("Preencha todos os campos do cliente.", "erro");
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

        mostrarMensagem("Cliente cadastrado com sucesso!", "sucesso");

        document.getElementById("formCliente")?.reset();
        document.getElementById("formularioCliente")?.classList.add("oculto");

        await carregarClientes();
    } catch (erro) {
        mostrarMensagem(`Não foi possível cadastrar o cliente: ${erro.message}`, "erro");
    }
}