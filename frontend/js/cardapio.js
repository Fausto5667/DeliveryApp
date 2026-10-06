let itensCardapio = [];

document.addEventListener("DOMContentLoaded", () => {
    const tipoItem = document.getElementById("tipoItem");
    const btnNovoItem = document.getElementById("btnNovoItem");
    const btnCancelarItem = document.getElementById("btnCancelarItem");
    const formItem = document.getElementById("formItem");
    const btnBuscar = document.getElementById("btnBuscar");
    const btnListar = document.getElementById("btnListar");

    if (tipoItem) {
        tipoItem.addEventListener("change", atualizarCamposEspecificos);
        atualizarCamposEspecificos();
    }

    if (btnNovoItem) {
        btnNovoItem.addEventListener("click", () => {
            document
                .getElementById("formularioItem")
                .classList.remove("oculto");
        });
    }

    if (btnCancelarItem) {
        btnCancelarItem.addEventListener("click", () => {
            formItem.reset();
            atualizarCamposEspecificos();

            document
                .getElementById("formularioItem")
                .classList.add("oculto");
        });
    }

    if (formItem) {
        formItem.addEventListener("submit", cadastrarItem);
    }

    if (btnBuscar) {
        btnBuscar.addEventListener("click", buscarItens);
    }

    if (btnListar) {
        btnListar.addEventListener("click", carregarItens);
    }

    carregarItens();
});

function atualizarCamposEspecificos() {
    const tipo = document.getElementById("tipoItem")?.value;

    document
        .getElementById("campoPrato")
        ?.classList.toggle("oculto", tipo !== "prato");

    document
        .getElementById("campoBebida")
        ?.classList.toggle("oculto", tipo !== "bebida");

    document
        .getElementById("campoSobremesa")
        ?.classList.toggle("oculto", tipo !== "sobremesa");
}

async function carregarItens() {
    try {
        const dados = await requisicao("/Cardapio");

        itensCardapio = obterLista(dados);

        renderizarItens(itensCardapio);

    } catch (erro) {
        itensCardapio = [];

        renderizarItens([]);
    }
}

async function buscarItens() {
    const busca = document
        .getElementById("buscaCardapio")
        .value
        .trim();

    if (!busca) {
        carregarItens();
        return;
    }

    try {
        const dados = await requisicao(
            `/Cardapio/buscar?nome=${encodeURIComponent(busca)}`
        );

        itensCardapio = obterLista(dados);

        renderizarItens(itensCardapio);

    } catch (erro) {
        const resultado = itensCardapio.filter(item =>
            obterNomeItem(item)
                .toLowerCase()
                .includes(busca.toLowerCase())
        );

        renderizarItens(resultado);
    }
}

function obterNomeItem(item) {
    return item?.nome ??
        item?.Nome ??
        "Sem nome";
}

function obterTipoItem(item) {
    return item?.categoria ??
        item?.Categoria ??
        "-";
}

function obterPrecoItem(item) {
    return item?.precoBase ??
        item?.PrecoBase ??
        0;
}

function renderizarItens(itens) {
    const tabela = document.getElementById("tabelaCardapio");

    if (!tabela) {
        return;
    }

    if (!itens.length) {
        tabela.innerHTML = `
            <tr>
                <td colspan="4">
                    Nenhum item encontrado.
                </td>
            </tr>
        `;

        return;
    }

    tabela.innerHTML = itens.map(item => `
        <tr>
            <td>${obterId(item) ?? "-"}</td>

            <td>
                ${obterNomeItem(item)}
            </td>

            <td>
                ${obterTipoItem(item)}
            </td>

            <td>
                ${formatarMoeda(obterPrecoItem(item))}
            </td>
        </tr>
    `).join("");
}

async function cadastrarItem(evento) {
    evento.preventDefault();

    const tipo = document
        .getElementById("tipoItem")
        .value;

    const nome = document
        .getElementById("nomeItem")
        .value
        .trim();

    const preco = Number(
        document
            .getElementById("precoItem")
            .value
            .replace(",", ".")
    );

    if (
        !tipo ||
        !nome ||
        !Number.isFinite(preco) ||
        preco <= 0
    ) {
        mostrarMensagem(
            "Preencha corretamente todos os campos.",
            "erro"
        );

        return;
    }

    const dados = {
        tipo: tipo,
        nome: nome,
        precoBase: preco
    };

    if (tipo === "prato") {
        const tempo = Number(
            document
                .getElementById("tempoPreparo")
                .value
        );

        if (
            !Number.isFinite(tempo) ||
            tempo <= 0
        ) {
            mostrarMensagem(
                "Informe um tempo de preparo válido.",
                "erro"
            );

            return;
        }

        dados.tempoPreparo = tempo;
    }

    if (tipo === "bebida") {
        const volume = Number(
            document
                .getElementById("volumeBebida")
                .value
        );

        if (
            !Number.isFinite(volume) ||
            volume <= 0
        ) {
            mostrarMensagem(
                "Informe um volume válido.",
                "erro"
            );

            return;
        }

        dados.volumeMl = volume;
    }

    if (tipo === "sobremesa") {
        dados.gelada =
            document
                .getElementById("refrigerada")
                .value === "true";
    }

    try {
        await requisicao("/Cardapio", {
            method: "POST",
            body: JSON.stringify(dados)
        });

        mostrarMensagem(
            "Item cadastrado com sucesso.",
            "sucesso"
        );

        document
            .getElementById("formItem")
            .reset();

        atualizarCamposEspecificos();

        document
            .getElementById("formularioItem")
            .classList.add("oculto");

        await carregarItens();

    } catch (erro) {
        mostrarMensagem(
            `Não foi possível cadastrar o item: ${erro.message}`,
            "erro"
        );
    }
}