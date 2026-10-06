let pedidos = [];
let clientesPedido = [];
let entregadoresPedido = [];
let itensCardapioPedido = [];
let itensSelecionados = [];

document.addEventListener("DOMContentLoaded", () => {

    const btnNovoPedido =
        document.getElementById("btnNovoPedido");

    const btnCancelarPedido =
        document.getElementById("btnCancelarPedido");

    const formPedido =
        document.getElementById("formPedido");

    const btnAdicionarItem =
        document.getElementById("btnAdicionarItem");

    const filtroStatus =
        document.getElementById("filtroStatus");

    if (btnNovoPedido) {

        btnNovoPedido.addEventListener("click", async () => {

            document
                .getElementById("formularioPedido")
                .classList.remove("oculto");

            await carregarDadosFormulario();
        });
    }

    if (btnCancelarPedido) {

        btnCancelarPedido.addEventListener("click", () => {

            formPedido.reset();

            itensSelecionados = [];

            renderizarItensSelecionados();

            atualizarResumo();

            document
                .getElementById("formularioPedido")
                .classList.add("oculto");
        });
    }

    if (btnAdicionarItem) {

        btnAdicionarItem.addEventListener(
            "click",
            adicionarItem
        );
    }

    if (formPedido) {

        formPedido.addEventListener(
            "submit",
            criarPedido
        );
    }

    if (filtroStatus) {

        filtroStatus.addEventListener(
            "change",
            carregarPedidos
        );
    }

    carregarPedidos();
});

async function carregarDadosFormulario() {

    try {

        const [
            clientesDados,
            entregadoresDados,
            cardapioDados
        ] = await Promise.all([
            requisicao("/clientes"),
            requisicao("/entregadores"),
            requisicao("/cardapio")
        ]);

        clientesPedido =
            obterLista(clientesDados);

        entregadoresPedido =
            obterLista(entregadoresDados);

        itensCardapioPedido =
            obterLista(cardapioDados);

        preencherClientes();

        preencherEntregadores();

        preencherCardapio();

    } catch (erro) {

        mostrarMensagem(
            `Não foi possível carregar os dados do pedido: ${erro.message}`,
            "erro"
        );
    }
}

function preencherClientes() {

    const select =
        document.getElementById("clientePedido");

    if (!select) {
        return;
    }

    select.innerHTML = `
        <option value="">
            Selecione o cliente
        </option>
    `;

    clientesPedido.forEach(cliente => {

        const telefone =
            obterTelefoneCliente(cliente);

        const nome =
            obterNomeCliente(cliente);

        const option =
            document.createElement("option");

        option.value = telefone;

        option.textContent =
            `${nome} - ${telefone}`;

        select.appendChild(option);
    });
}

function preencherEntregadores() {

    const select =
        document.getElementById("entregadorPedido");

    if (!select) {
        return;
    }

    select.innerHTML = `
        <option value="">
            Selecione o entregador
        </option>
    `;

    entregadoresPedido
        .filter(obterDisponibilidade)
        .forEach(entregador => {

            const id =
                obterId(entregador);

            const nome =
                obterNomeEntregador(entregador);

            const veiculo =
                obterVeiculo(entregador);

            const option =
                document.createElement("option");

            option.value = id;

            option.textContent =
                `${nome} - ${veiculo}`;

            select.appendChild(option);
        });
}

function preencherCardapio() {

    const select =
        document.getElementById("itemPedido");

    if (!select) {
        return;
    }

    select.innerHTML = `
        <option value="">
            Selecione um item
        </option>
    `;

    itensCardapioPedido.forEach(item => {

        const codigo =
            obterCodigoItem(item);

        const nome =
            obterNomeItem(item);

        const preco =
            obterPrecoItem(item);

        const option =
            document.createElement("option");

        option.value = codigo;

        option.textContent =
            `${nome} - ${formatarMoeda(preco)}`;

        select.appendChild(option);
    });
}

function adicionarItem() {

    const select =
        document.getElementById("itemPedido");

    const quantidadeInput =
        document.getElementById("quantidadeItem");

    const codigo =
        Number(select.value);

    const quantidade =
        Number(quantidadeInput.value);

    if (!codigo) {

        mostrarMensagem(
            "Selecione um item do cardápio.",
            "erro"
        );

        return;
    }

    if (!quantidade ||
        quantidade < 1 ||
        quantidade > 100) {

        mostrarMensagem(
            "A quantidade deve ser entre 1 e 100.",
            "erro"
        );

        return;
    }

    const item =
        itensCardapioPedido.find(
            item =>
                obterCodigoItem(item) === codigo
        );

    if (!item) {

        mostrarMensagem(
            "Item do cardápio não encontrado.",
            "erro"
        );

        return;
    }

    const existente =
        itensSelecionados.find(
            item =>
                item.codigoItem === codigo
        );

    if (existente) {

        const novaQuantidade =
            existente.quantidade + quantidade;

        if (novaQuantidade > 100) {

            mostrarMensagem(
                "A quantidade de um item não pode ultrapassar 100 unidades.",
                "erro"
            );

            return;
        }

        existente.quantidade =
            novaQuantidade;

    } else {

        itensSelecionados.push({

            codigoItem: codigo,

            nome:
                obterNomeItem(item),

            preco:
                obterPrecoItem(item),

            quantidade:
                quantidade
        });
    }

    renderizarItensSelecionados();

    atualizarResumo();

    select.value = "";

    quantidadeInput.value = 1;
}

function removerItem(index) {

    itensSelecionados.splice(index, 1);

    renderizarItensSelecionados();

    atualizarResumo();
}

function renderizarItensSelecionados() {

    const container =
        document.getElementById(
            "listaItensPedido"
        );

    if (!container) {
        return;
    }

    if (!itensSelecionados.length) {

        container.innerHTML = `
            <p>
                Nenhum item adicionado.
            </p>
        `;

        return;
    }

    container.innerHTML = `

        <table>

            <thead>

                <tr>

                    <th>
                        Item
                    </th>

                    <th>
                        Preço
                    </th>

                    <th>
                        Quantidade
                    </th>

                    <th>
                        Subtotal
                    </th>

                    <th>
                        Ação
                    </th>

                </tr>

            </thead>

            <tbody>

                ${itensSelecionados.map((item, index) => {

                    const subtotal =
                        item.preco *
                        item.quantidade;

                    return `
                        <tr>

                            <td>
                                ${item.nome}
                            </td>

                            <td>
                                ${formatarMoeda(item.preco)}
                            </td>

                            <td>
                                ${item.quantidade}
                            </td>

                            <td>
                                ${formatarMoeda(subtotal)}
                            </td>

                            <td>

                                <button
                                    type="button"
                                    onclick="removerItem(${index})"
                                >
                                    Remover
                                </button>

                            </td>

                        </tr>
                    `;

                }).join("")}

            </tbody>

        </table>
    `;
}

function atualizarResumo() {

    const subtotal =
        itensSelecionados.reduce(
            (total, item) =>
                total +
                item.preco *
                item.quantidade,
            0
        );

    const subtotalElemento =
        document.getElementById(
            "subtotalPedido"
        );

    const taxaElemento =
        document.getElementById(
            "taxaEntrega"
        );

    const totalElemento =
        document.getElementById(
            "totalPedido"
        );

    if (subtotalElemento) {

        subtotalElemento.textContent =
            formatarMoeda(subtotal);
    }

    if (taxaElemento) {

        taxaElemento.textContent =
            "R$ 0,00";
    }

    if (totalElemento) {

        totalElemento.textContent =
            formatarMoeda(subtotal);
    }
}

async function criarPedido(evento) {

    evento.preventDefault();

    const telefoneCliente =
        document
            .getElementById("clientePedido")
            .value;

    const entregadorId =
        Number(
            document
                .getElementById("entregadorPedido")
                .value
        );

    const distanciaKm =
        Number(
            document
                .getElementById("distanciaPedido")
                .value
        );

    if (!telefoneCliente) {

        mostrarMensagem(
            "Selecione um cliente.",
            "erro"
        );

        return;
    }

    if (!entregadorId) {

        mostrarMensagem(
            "Selecione um entregador.",
            "erro"
        );

        return;
    }

    if (!distanciaKm ||
        distanciaKm < 0.1 ||
        distanciaKm > 500) {

        mostrarMensagem(
            "Informe uma distância entre 0,1 e 500 km.",
            "erro"
        );

        return;
    }

    if (!itensSelecionados.length) {

        mostrarMensagem(
            "Adicione pelo menos um item ao pedido.",
            "erro"
        );

        return;
    }

    const dados = {

        telefoneCliente:
            telefoneCliente,

        entregadorId:
            entregadorId,

        distanciaKm:
            distanciaKm,

        itens:
            itensSelecionados.map(item => ({

                codigoItem:
                    item.codigoItem,

                quantidade:
                    item.quantidade
            }))
    };

    try {

        const resposta =
            await requisicao("/pedidos", {

                method: "POST",

                body:
                    JSON.stringify(dados)
            });

        const pedidoCriado =
            resposta?.data ??
            resposta;

        const subtotal =
            pedidoCriado?.subtotal ??
            pedidoCriado?.Subtotal ??
            0;

        const frete =
            pedidoCriado?.frete ??
            pedidoCriado?.Frete ??
            0;

        const total =
            pedidoCriado?.total ??
            pedidoCriado?.Total ??
            0;

        const subtotalElemento =
            document.getElementById(
                "subtotalPedido"
            );

        const taxaElemento =
            document.getElementById(
                "taxaEntrega"
            );

        const totalElemento =
            document.getElementById(
                "totalPedido"
            );

        if (subtotalElemento) {

            subtotalElemento.textContent =
                formatarMoeda(subtotal);
        }

        if (taxaElemento) {

            taxaElemento.textContent =
                formatarMoeda(frete);
        }

        if (totalElemento) {

            totalElemento.textContent =
                formatarMoeda(total);
        }

        mostrarMensagem(
            `Pedido criado com sucesso. Frete: ${formatarMoeda(frete)} | Total: ${formatarMoeda(total)}`,
            "sucesso"
        );

        await carregarPedidos();

        setTimeout(() => {

            document
                .getElementById("formPedido")
                .reset();

            itensSelecionados = [];

            renderizarItensSelecionados();

            atualizarResumo();

            document
                .getElementById("formularioPedido")
                .classList.add("oculto");

        }, 1500);

    } catch (erro) {

        mostrarMensagem(
            `Não foi possível criar o pedido: ${erro.message}`,
            "erro"
        );
    }
}

async function carregarPedidos() {

    try {

        const filtro =
            document
                .getElementById("filtroStatus")
                ?.value;

        let endpoint =
            "/pedidos";

        if (filtro) {

            endpoint +=
                `?status=${encodeURIComponent(filtro)}`;
        }

        const dados =
            await requisicao(endpoint);

        pedidos =
            obterLista(dados);

        renderizarPedidos(pedidos);

    } catch (erro) {

        renderizarPedidos([]);

        mostrarMensagem(
            `Não foi possível carregar os pedidos: ${erro.message}`,
            "erro"
        );
    }
}

function renderizarPedidos(lista) {

    const tabela =
        document.getElementById(
            "tabelaPedidos"
        );

    if (!tabela) {
        return;
    }

    if (!lista.length) {

        tabela.innerHTML = `
            <tr>

                <td colspan="6">
                    Nenhum pedido encontrado.
                </td>

            </tr>
        `;

        return;
    }

    tabela.innerHTML =
        lista.map(pedido => {

            const id =
                pedido?.id ??
                pedido?.Id ??
                "-";

            const cliente =
                pedido?.cliente ??
                pedido?.Cliente;

            const entregador =
                pedido?.entregador ??
                pedido?.Entregador;

            const nomeCliente =
                obterNomeCliente(cliente);

            const nomeEntregador =
                obterNomeEntregador(entregador);

            const total =
                pedido?.total ??
                pedido?.Total ??
                0;

            const status =
                pedido?.status ??
                pedido?.Status ??
                "-";

            return `
                <tr>

                    <td>
                        ${id}
                    </td>

                    <td>
                        ${nomeCliente}
                    </td>

                    <td>
                        ${nomeEntregador}
                    </td>

                    <td>
                        ${formatarMoeda(total)}
                    </td>

                    <td>
                        ${renderizarStatus(status)}
                    </td>

                    <td>

                        ${podeAvancar(status)
                            ? `
                                <button
                                    type="button"
                                    onclick="avancarStatus(${id})"
                                >
                                    Avançar
                                </button>
                            `
                            : "Finalizado"}

                    </td>

                </tr>
            `;

        }).join("");
}

async function avancarStatus(id) {

    try {

        await requisicao(
            `/pedidos/${id}/avancar`,
            {
                method: "PATCH"
            }
        );

        mostrarMensagem(
            "Status do pedido atualizado com sucesso.",
            "sucesso"
        );

        await carregarPedidos();

    } catch (erro) {

        mostrarMensagem(
            `Não foi possível avançar o status: ${erro.message}`,
            "erro"
        );
    }
}

function podeAvancar(status) {

    return status !== "Entregue" &&
        status !== "entregue";
}

function renderizarStatus(status) {

    const valor =
        String(status);

    let classe =
        "status-recebido";

    let texto =
        "Recebido";

    if (valor === "EmPreparo") {

        classe =
            "status-preparo";

        texto =
            "Em Preparo";

    } else if (valor === "Pronto") {

        classe =
            "status-pronto";

        texto =
            "Pronto";

    } else if (valor === "EmRota") {

        classe =
            "status-rota";

        texto =
            "Em Rota";

    } else if (valor === "Entregue") {

        classe =
            "status-entregue";

        texto =
            "Entregue";
    }

    return `
        <span class="status ${classe}">
            ${texto}
        </span>
    `;
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
        "";
}

function obterNomeEntregador(entregador) {

    return entregador?.nome ??
        entregador?.Nome ??
        "Sem nome";
}

function obterVeiculo(entregador) {

    return entregador?.veiculo ??
        entregador?.Veiculo ??
        "-";
}

function obterDisponibilidade(entregador) {

    const valor =
        entregador?.disponivel ??
        entregador?.Disponivel;

    return valor === true ||
        valor === "true";
}

function obterCodigoItem(item) {

    return item?.codigo ??
        item?.Codigo ??
        item?.id ??
        item?.Id ??
        0;
}

function obterNomeItem(item) {

    return item?.nome ??
        item?.Nome ??
        "Sem nome";
}

function obterPrecoItem(item) {

    return Number(
        item?.precoBase ??
        item?.PrecoBase ??
        item?.preco ??
        item?.Preco ??
        0
    );
}

function obterId(objeto) {

    return objeto?.id ??
        objeto?.Id ??
        null;
}

function formatarMoeda(valor) {

    return Number(valor).toLocaleString(
        "pt-BR",
        {
            style: "currency",
            currency: "BRL"
        }
    );
}