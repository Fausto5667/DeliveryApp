let pedidos = [];
let clientesPedido = [];
let entregadoresPedido = [];
let itensCardapioPedido = [];
let itensSelecionados = [];

document.addEventListener("DOMContentLoaded", () => {
    const btnNovoPedido = document.getElementById("btnNovoPedido");
    const btnCancelarPedido = document.getElementById("btnCancelarPedido");
    const formPedido = document.getElementById("formPedido");
    const btnAdicionarItem = document.getElementById("btnAdicionarItem");
    const filtroStatus = document.getElementById("filtroStatus");
    const entregadorSelect = document.getElementById("entregadorPedido");
    const distanciaInput = document.getElementById("distanciaPedido");
    const clienteSelect = document.getElementById("clientePedido");

    if (btnNovoPedido) {
        btnNovoPedido.addEventListener("click", async () => {
            document.getElementById("formularioPedido")?.classList.remove("oculto");
            await carregarDadosFormulario();
        });
    }

    if (btnCancelarPedido) {
        btnCancelarPedido.addEventListener("click", () => {
            formPedido?.reset();
            itensSelecionados = [];
            renderizarItensSelecionados();
            atualizarResumoLocal();
            document.getElementById("formularioPedido")?.classList.add("oculto");
        });
    }

    if (btnAdicionarItem) {
        btnAdicionarItem.addEventListener("click", adicionarItem);
    }

    if (formPedido) {
        formPedido.addEventListener("submit", criarPedido);
    }

    if (filtroStatus) {
        filtroStatus.addEventListener("change", carregarPedidos);
    }

    // Recalcular simulação sempre que mudar entregador ou distância
    if (entregadorSelect) {
        entregadorSelect.addEventListener("change", consultarSimulacaoSePossivel);
    }
    if (distanciaInput) {
        distanciaInput.addEventListener("input", consultarSimulacaoSePossivel);
    }

    carregarPedidos();
});

async function carregarDadosFormulario() {
    try {
        const [clientesDados, entregadoresDados, cardapioDados] = await Promise.all([
            requisicao("/clientes"),
            requisicao("/entregadores"),
            requisicao("/cardapio")
        ]);

        clientesPedido = obterLista(clientesDados);
        entregadoresPedido = obterLista(entregadoresDados);
        itensCardapioPedido = obterLista(cardapioDados);

        preencherClientes();
        preencherEntregadores();
        preencherCardapio();
    } catch (erro) {
        mostrarMensagem(`Não foi possível carregar os dados do formulário: ${erro.message}`, "erro");
    }
}

function preencherClientes() {
    const select = document.getElementById("clientePedido");
    if (!select) return;

    select.innerHTML = `<option value="">Selecione o cliente</option>`;
    clientesPedido.forEach(cliente => {
        const telefone = cliente?.telefone ?? cliente?.Telefone ?? "";
        const nome = cliente?.nome ?? cliente?.Nome ?? "Sem nome";
        const option = document.createElement("option");
        option.value = telefone;
        option.textContent = `${nome} (${telefone})`;
        select.appendChild(option);
    });
}

function preencherEntregadores() {
    const select = document.getElementById("entregadorPedido");
    if (!select) return;

    select.innerHTML = `<option value="">Selecione o entregador</option>`;
    const disponiveis = entregadoresPedido.filter(e => (e?.disponivel ?? e?.Disponivel) === true);

    if (disponiveis.length === 0) {
        const opt = document.createElement("option");
        opt.value = "";
        opt.disabled = true;
        opt.textContent = "Nenhum entregador disponível no momento";
        select.appendChild(opt);
        return;
    }

    disponiveis.forEach(entregador => {
        const id = obterId(entregador);
        const nome = entregador?.nome ?? entregador?.Nome ?? "Sem nome";
        const veiculo = entregador?.veiculo ?? entregador?.Veiculo ?? "";
        const option = document.createElement("option");
        option.value = id;
        option.textContent = `${nome} - ${veiculo} (Disponível)`;
        select.appendChild(option);
    });
}

function preencherCardapio() {
    const select = document.getElementById("itemPedido");
    if (!select) return;

    select.innerHTML = `<option value="">Selecione um item do cardápio</option>`;
    itensCardapioPedido.forEach(item => {
        const codigo = obterId(item);
        const nome = item?.nome ?? item?.Nome ?? "Sem nome";
        const preco = item?.precoBase ?? item?.PrecoBase ?? 0;
        const categoria = item?.categoria ?? item?.Categoria ?? "";

        const option = document.createElement("option");
        option.value = codigo;
        option.textContent = `[${categoria}] ${nome} - ${formatarMoeda(preco)}`;
        select.appendChild(option);
    });
}

function adicionarItem() {
    const select = document.getElementById("itemPedido");
    const quantidadeInput = document.getElementById("quantidadeItem");

    const codigo = Number(select?.value);
    const quantidade = Number(quantidadeInput?.value);

    if (!codigo) {
        mostrarMensagem("Selecione um item do cardápio.", "erro");
        return;
    }

    if (!quantidade || quantidade < 1 || quantidade > 100) {
        mostrarMensagem("A quantidade deve ser entre 1 e 100.", "erro");
        return;
    }

    const item = itensCardapioPedido.find(i => Number(obterId(i)) === codigo);
    if (!item) {
        mostrarMensagem("Item do cardápio não encontrado.", "erro");
        return;
    }

    const existente = itensSelecionados.find(i => i.codigoItem === codigo);
    if (existente) {
        const novaQtd = existente.quantidade + quantidade;
        if (novaQtd > 100) {
            mostrarMensagem("A quantidade total deste item não pode ultrapassar 100.", "erro");
            return;
        }
        existente.quantidade = novaQtd;
    } else {
        itensSelecionados.push({
            codigoItem: codigo,
            nome: item?.nome ?? item?.Nome ?? "Item",
            preco: item?.precoBase ?? item?.PrecoBase ?? 0,
            tempoPreparo: item?.tempoPreparoMinutos ?? item?.TempoPreparoMinutos ?? 0,
            quantidade: quantidade
        });
    }

    renderizarItensSelecionados();
    select.value = "";
    if (quantidadeInput) quantidadeInput.value = 1;

    consultarSimulacaoSePossivel();
}

function removerItem(index) {
    itensSelecionados.splice(index, 1);
    renderizarItensSelecionados();
    consultarSimulacaoSePossivel();
}

function renderizarItensSelecionados() {
    const container = document.getElementById("listaItensPedido");
    if (!container) return;

    if (!itensSelecionados.length) {
        container.innerHTML = `<p style="padding: 12px; color: var(--texto-secundario);">Nenhum item adicionado ao pedido.</p>`;
        atualizarResumoLocal();
        return;
    }

    container.innerHTML = `
        <table>
            <thead>
                <tr>
                    <th>Item</th>
                    <th>Preço Unit.</th>
                    <th>Qtd</th>
                    <th>Subtotal</th>
                    <th>Ação</th>
                </tr>
            </thead>
            <tbody>
                ${itensSelecionados.map((item, index) => {
                    const sub = item.preco * item.quantidade;
                    return `
                        <tr>
                            <td>${item.nome}</td>
                            <td>${formatarMoeda(item.preco)}</td>
                            <td>${item.quantidade}</td>
                            <td><strong>${formatarMoeda(sub)}</strong></td>
                            <td>
                                <button type="button" class="btn-perigo-sm" onclick="removerItem(${index})">Remover</button>
                            </td>
                        </tr>
                    `;
                }).join("")}
            </tbody>
        </table>
    `;
    atualizarResumoLocal();
}

function atualizarResumoLocal() {
    const subtotal = itensSelecionados.reduce((total, item) => total + (item.preco * item.quantidade), 0);
    const subtotalEl = document.getElementById("subtotalPedido");
    const totalEl = document.getElementById("totalPedido");
    const taxaEl = document.getElementById("taxaEntrega");

    if (subtotalEl) subtotalEl.textContent = formatarMoeda(subtotal);
    if (totalEl) totalEl.textContent = formatarMoeda(subtotal);
    if (taxaEl) taxaEl.textContent = "Selecione o entregador e distância...";
}

async function consultarSimulacaoSePossivel() {
    const telefoneCliente = document.getElementById("clientePedido")?.value;
    const entregadorId = Number(document.getElementById("entregadorPedido")?.value);
    const distanciaKm = Number(document.getElementById("distanciaPedido")?.value);

    if (!entregadorId || !distanciaKm || distanciaKm <= 0 || itensSelecionados.length === 0) {
        atualizarResumoLocal();
        return;
    }

    const payload = {
        telefoneCliente: telefoneCliente || "11999999999",
        entregadorId: entregadorId,
        distanciaKm: distanciaKm,
        itens: itensSelecionados.map(i => ({ codigoItem: i.codigoItem, quantidade: i.quantidade }))
    };

    try {
        const dados = await requisicao("/pedidos/simulacao", {
            method: "POST",
            body: JSON.stringify(payload)
        });

        const subtotal = dados?.subtotal ?? dados?.Subtotal ?? 0;
        const frete = dados?.frete ?? dados?.Frete ?? 0;
        const total = dados?.total ?? dados?.Total ?? 0;
        const freteGratis = dados?.freteGratis ?? dados?.FreteGratis ?? (frete === 0);

        const subtotalEl = document.getElementById("subtotalPedido");
        const taxaEl = document.getElementById("taxaEntrega");
        const totalEl = document.getElementById("totalPedido");

        if (subtotalEl) subtotalEl.textContent = formatarMoeda(subtotal);
        if (taxaEl) {
            taxaEl.innerHTML = freteGratis
                ? `<span class="badge badge-gratis">FRETE GRÁTIS!</span> (R$ 0,00)`
                : formatarMoeda(frete);
        }
        if (totalEl) totalEl.textContent = formatarMoeda(total);

    } catch (e) {
        // Ignora silenciosamente durante digitação
    }
}

async function criarPedido(evento) {
    evento.preventDefault();

    const telefoneCliente = document.getElementById("clientePedido")?.value?.trim() ?? "";
    const entregadorId = Number(document.getElementById("entregadorPedido")?.value);
    const distanciaKm = Number(document.getElementById("distanciaPedido")?.value);

    if (!telefoneCliente) {
        mostrarMensagem("Selecione um cliente para o pedido.", "erro");
        return;
    }

    if (!entregadorId) {
        mostrarMensagem("Selecione um entregador disponível.", "erro");
        return;
    }

    if (!distanciaKm || distanciaKm < 0.1 || distanciaKm > 500) {
        mostrarMensagem("Informe uma distância válida entre 0,1 e 500 km.", "erro");
        return;
    }

    if (!itensSelecionados.length) {
        mostrarMensagem("Adicione pelo menos um item ao pedido.", "erro");
        return;
    }

    const dados = {
        telefoneCliente: telefoneCliente,
        entregadorId: entregadorId,
        distanciaKm: distanciaKm,
        itens: itensSelecionados.map(item => ({
            codigoItem: item.codigoItem,
            quantidade: item.quantidade
        }))
    };

    try {
        const pedidoCriado = await requisicao("/pedidos", {
            method: "POST",
            body: JSON.stringify(dados)
        });

        const id = pedidoCriado?.id ?? pedidoCriado?.Id ?? "";
        const total = pedidoCriado?.total ?? pedidoCriado?.Total ?? 0;

        mostrarMensagem(`Pedido #${id} criado com sucesso! Total: ${formatarMoeda(total)}`, "sucesso");

        document.getElementById("formPedido")?.reset();
        itensSelecionados = [];
        renderizarItensSelecionados();
        document.getElementById("formularioPedido")?.classList.add("oculto");

        await carregarPedidos();
    } catch (erro) {
        mostrarMensagem(`Não foi possível criar o pedido: ${erro.message}`, "erro");
    }
}

async function carregarPedidos() {
    try {
        const filtro = document.getElementById("filtroStatus")?.value ?? "";
        let endpoint = "/pedidos";
        if (filtro) {
            endpoint += `?status=${encodeURIComponent(filtro)}`;
        }

        const dados = await requisicao(endpoint);
        pedidos = obterLista(dados);
        renderizarPedidos(pedidos);
    } catch (erro) {
        renderizarPedidos([]);
        mostrarMensagem(`Não foi possível carregar os pedidos: ${erro.message}`, "erro");
    }
}

function renderizarPedidos(lista) {
    const tabela = document.getElementById("tabelaPedidos");
    if (!tabela) return;

    if (!lista.length) {
        tabela.innerHTML = `
            <tr>
                <td colspan="6" style="text-align: center; color: var(--texto-secundario); padding: 24px;">
                    Nenhum pedido encontrado.
                </td>
            </tr>
        `;
        return;
    }

    tabela.innerHTML = lista.map(pedido => {
        const id = pedido?.id ?? pedido?.Id ?? "-";
        const cliente = pedido?.cliente ?? pedido?.Cliente;
        const entregador = pedido?.entregador ?? pedido?.Entregador;
        const nomeCliente = cliente?.nome ?? cliente?.Nome ?? "Cliente";
        const nomeEntregador = entregador?.nome ?? entregador?.Nome ?? "Entregador";
        const veiculo = entregador?.veiculo ?? entregador?.Veiculo ?? "";
        const total = pedido?.total ?? pedido?.Total ?? 0;
        const status = pedido?.status ?? pedido?.Status ?? "-";
        const proximoStatus = pedido?.proximoStatus ?? pedido?.ProximoStatus;

        return `
            <tr>
                <td><strong>#${id}</strong></td>
                <td>${nomeCliente}</td>
                <td>${nomeEntregador} <small style="color:var(--texto-secundario)">(${veiculo})</small></td>
                <td><strong>${formatarMoeda(total)}</strong></td>
                <td>${renderizarStatusBadge(status)}</td>
                <td>
                    ${proximoStatus ? `
                        <button type="button" class="btn-avancar" onclick="avancarStatus(${id})">
                            <span>Avançar para ${formatarTextoStatus(proximoStatus)}</span>
                            <svg class="btn-icone" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg>
                        </button>
                    ` : `
                        <span class="badge badge-entregue">
                            <svg class="badge-icone" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                            Concluído
                        </span>
                    `}
                </td>
            </tr>
        `;
    }).join("");
}

async function avancarStatus(id) {
    try {
        await requisicao(`/pedidos/${id}/avancar`, {
            method: "PATCH"
        });

        mostrarMensagem(`Status do pedido #${id} avançado com sucesso!`, "sucesso");
        await carregarPedidos();
    } catch (erro) {
        mostrarMensagem(`Não foi possível avançar o status: ${erro.message}`, "erro");
    }
}

function formatarTextoStatus(status) {
    const s = String(status);
    switch (s) {
        case "Recebido": return "Recebido";
        case "EmPreparo": return "Em Preparo";
        case "Pronto": return "Pronto";
        case "EmRota": return "Em Rota";
        case "Entregue": return "Entregue";
        default: return s;
    }
}

function renderizarStatusBadge(status) {
    const s = String(status);
    let classe = "badge-recebido";
    let icone = `<svg class="badge-icone" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>`;
    let texto = formatarTextoStatus(s);

    if (s === "EmPreparo") {
        classe = "badge-preparo";
        icone = `<svg class="badge-icone" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg>`;
    } else if (s === "Pronto") {
        classe = "badge-pronto";
        icone = `<svg class="badge-icone" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>`;
    } else if (s === "EmRota") {
        classe = "badge-rota";
        icone = `<span class="ponto-pulso"></span>`;
    } else if (s === "Entregue") {
        classe = "badge-entregue";
        icone = `<svg class="badge-icone" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>`;
    }

    return `<span class="badge ${classe}">${icone} ${texto}</span>`;
}
