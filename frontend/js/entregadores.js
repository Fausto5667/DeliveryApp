let entregadores = [];

document.addEventListener("DOMContentLoaded", () => {
    const btnNovoEntregador = document.getElementById("btnNovoEntregador");
    const btnCancelarEntregador = document.getElementById("btnCancelarEntregador");
    const formEntregador = document.getElementById("formEntregador");

    if (btnNovoEntregador) {
        btnNovoEntregador.addEventListener("click", () => {
            document.getElementById("formularioEntregador")?.classList.remove("oculto");
            document.getElementById("nomeEntregador")?.focus();
        });
    }

    if (btnCancelarEntregador) {
        btnCancelarEntregador.addEventListener("click", () => {
            formEntregador?.reset();
            document.getElementById("formularioEntregador")?.classList.add("oculto");
        });
    }

    if (formEntregador) {
        formEntregador.addEventListener("submit", cadastrarEntregador);
    }

    carregarEntregadores();
});

async function carregarEntregadores() {
    try {
        const dados = await requisicao("/entregadores");
        entregadores = obterLista(dados);
        renderizarEntregadores(entregadores);
    } catch (erro) {
        renderizarEntregadores([]);
        mostrarMensagem(`Não foi possível carregar os entregadores: ${erro.message}`, "erro");
    }
}

function obterNomeEntregador(entregador) {
    return entregador?.nome ?? entregador?.Nome ?? "Sem nome";
}

function obterVeiculo(entregador) {
    return entregador?.veiculo ?? entregador?.Veiculo ?? "-";
}

function obterDisponibilidade(entregador) {
    const valor = entregador?.disponivel ?? entregador?.Disponivel;
    return valor === true || valor === "true";
}

function renderizarEntregadores(lista) {
    const tabela = document.getElementById("tabelaEntregadores");
    if (!tabela) return;

    if (!lista.length) {
        tabela.innerHTML = `
            <tr>
                <td colspan="4" style="text-align: center; color: var(--texto-secundario); padding: 24px;">
                    Nenhum entregador cadastrado na frota.
                </td>
            </tr>
        `;
        return;
    }

    tabela.innerHTML = lista.map(entregador => {
        const id = obterId(entregador);
        const disponivel = obterDisponibilidade(entregador);
        const veiculo = obterVeiculo(entregador);

        const iconeDisponivel = `<span class="ponto-pulso"></span>`;
        const iconeOcupado = `<svg class="badge-icone" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>`;

        return `
            <tr>
                <td><strong>#${id ?? "-"}</strong></td>
                <td>${obterNomeEntregador(entregador)}</td>
                <td><span class="badge badge-veiculo">${veiculo}</span></td>
                <td>
                    <span class="badge ${disponivel ? "badge-disponivel" : "badge-ocupado"}">
                        ${disponivel ? `${iconeDisponivel} Disponível` : `${iconeOcupado} Ocupado em Rota`}
                    </span>
                </td>
            </tr>
        `;
    }).join("");
}

async function cadastrarEntregador(evento) {
    evento.preventDefault();

    const nome = document.getElementById("nomeEntregador")?.value?.trim() ?? "";
    const veiculo = document.getElementById("veiculoEntregador")?.value ?? "";

    if (!nome || !veiculo) {
        mostrarMensagem("Preencha o nome e selecione o tipo de veículo do entregador.", "erro");
        return;
    }

    const dados = {
        nome: nome,
        veiculo: veiculo
    };

    try {
        await requisicao("/entregadores", {
            method: "POST",
            body: JSON.stringify(dados)
        });

        mostrarMensagem("Entregador cadastrado com sucesso!", "sucesso");

        document.getElementById("formEntregador")?.reset();
        document.getElementById("formularioEntregador")?.classList.add("oculto");

        await carregarEntregadores();
    } catch (erro) {
        mostrarMensagem(`Não foi possível cadastrar o entregador: ${erro.message}`, "erro");
    }
}