let entregadores = [];

document.addEventListener("DOMContentLoaded", () => {

    const btnNovoEntregador =
        document.getElementById("btnNovoEntregador");

    const btnCancelarEntregador =
        document.getElementById("btnCancelarEntregador");

    const formEntregador =
        document.getElementById("formEntregador");

    if (btnNovoEntregador) {

        btnNovoEntregador.addEventListener("click", () => {

            document
                .getElementById("formularioEntregador")
                .classList.remove("oculto");

        });
    }

    if (btnCancelarEntregador) {

        btnCancelarEntregador.addEventListener("click", () => {

            formEntregador.reset();

            document
                .getElementById("formularioEntregador")
                .classList.add("oculto");

        });
    }

    if (formEntregador) {

        formEntregador.addEventListener(
            "submit",
            cadastrarEntregador
        );
    }

    carregarEntregadores();
});

async function carregarEntregadores() {

    try {

        const dados =
            await requisicao("/entregadores");

        entregadores =
            obterLista(dados);

        renderizarEntregadores(
            entregadores
        );

    } catch (erro) {

        renderizarEntregadores([]);

        mostrarMensagem(
            `Não foi possível carregar os entregadores: ${erro.message}`,
            "erro"
        );
    }
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

function renderizarEntregadores(lista) {

    const tabela =
        document.getElementById(
            "tabelaEntregadores"
        );

    if (!tabela) {
        return;
    }

    if (!lista.length) {

        tabela.innerHTML = `
            <tr>
                <td colspan="4">
                    Nenhum entregador cadastrado.
                </td>
            </tr>
        `;

        return;
    }

    tabela.innerHTML = lista.map(entregador => {

        const disponivel =
            obterDisponibilidade(entregador);

        return `
            <tr>

                <td>
                    ${obterId(entregador) ?? "-"}
                </td>

                <td>
                    ${obterNomeEntregador(entregador)}
                </td>

                <td>
                    ${obterVeiculo(entregador)}
                </td>

                <td>
                    ${disponivel
                        ? "Disponível"
                        : "Ocupado"}
                </td>

            </tr>
        `;

    }).join("");
}

async function cadastrarEntregador(evento) {

    evento.preventDefault();

    const nome =
        document
            .getElementById("nomeEntregador")
            .value
            .trim();

    const veiculo =
        document
            .getElementById("veiculoEntregador")
            .value;

    if (!nome || !veiculo) {

        mostrarMensagem(
            "Preencha todos os campos do entregador.",
            "erro"
        );

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

        mostrarMensagem(
            "Entregador cadastrado com sucesso.",
            "sucesso"
        );

        document
            .getElementById("formEntregador")
            .reset();

        document
            .getElementById("formularioEntregador")
            .classList.add("oculto");

        await carregarEntregadores();

    } catch (erro) {

        mostrarMensagem(
            `Não foi possível cadastrar o entregador: ${erro.message}`,
            "erro"
        );
    }
}