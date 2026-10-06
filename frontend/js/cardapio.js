let itensCardapio = [];

document.addEventListener("DOMContentLoaded", () => {
    const tipoItem = document.getElementById("tipoItem");
    const btnNovoItem = document.getElementById("btnNovoItem");
    const btnCancelarItem = document.getElementById("btnCancelarItem");
    const formItem = document.getElementById("formItem");
    const btnBuscar = document.getElementById("btnBuscar");
    const inputBusca = document.getElementById("buscaCardapio");
    const btnListar = document.getElementById("btnListar");

    if (tipoItem) {
        tipoItem.addEventListener("change", atualizarCamposEspecificos);
        atualizarCamposEspecificos();
    }

    if (btnNovoItem) {
        btnNovoItem.addEventListener("click", () => {
            document.getElementById("formularioItem")?.classList.remove("oculto");
            document.getElementById("nomeItem")?.focus();
        });
    }

    if (btnCancelarItem) {
        btnCancelarItem.addEventListener("click", () => {
            formItem.reset();
            atualizarCamposEspecificos();
            document.getElementById("formularioItem")?.classList.add("oculto");
        });
    }

    if (formItem) {
        formItem.addEventListener("submit", cadastrarItem);
    }

    if (btnBuscar) {
        btnBuscar.addEventListener("click", buscarItens);
    }

    if (inputBusca) {
        inputBusca.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                e.preventDefault();
                buscarItens();
            }
        });
    }

    if (btnListar) {
        btnListar.addEventListener("click", () => {
            if (inputBusca) inputBusca.value = "";
            carregarItens();
        });
    }

    carregarItens();
});

function atualizarCamposEspecificos() {
    const tipo = document.getElementById("tipoItem")?.value?.toLowerCase();

    const campoPrato = document.getElementById("campoPrato");
    const campoBebida = document.getElementById("campoBebida");
    const campoSobremesa = document.getElementById("campoSobremesa");

    if (campoPrato) campoPrato.classList.toggle("oculto", tipo !== "prato");
    if (campoBebida) campoBebida.classList.toggle("oculto", tipo !== "bebida");
    if (campoSobremesa) campoSobremesa.classList.toggle("oculto", tipo !== "sobremesa");

    const tempoInput = document.getElementById("tempoPreparo");
    const volumeInput = document.getElementById("volumeBebida");
    if (tempoInput) tempoInput.required = (tipo === "prato");
    if (volumeInput) volumeInput.required = (tipo === "bebida");
}

async function carregarItens() {
    try {
        const dados = await requisicao("/cardapio");
        itensCardapio = obterLista(dados);
        renderizarItens(itensCardapio);
    } catch (erro) {
        itensCardapio = [];
        renderizarItens([]);
        mostrarMensagem(`Erro ao carregar cardápio: ${erro.message}`, "erro");
    }
}

async function buscarItens() {
    const busca = document.getElementById("buscaCardapio")?.value?.trim() ?? "";

    if (!busca) {
        carregarItens();
        return;
    }

    try {
        const dados = await requisicao(`/cardapio/buscar?nome=${encodeURIComponent(busca)}`);
        itensCardapio = obterLista(dados);
        renderizarItens(itensCardapio);
    } catch (erro) {
        const resultado = itensCardapio.filter(item =>
            obterNomeItem(item).toLowerCase().includes(busca.toLowerCase())
        );
        renderizarItens(resultado);
        mostrarMensagem(`Busca: ${erro.message}`, "erro");
    }
}

function obterNomeItem(item) {
    return item?.nome ?? item?.Nome ?? "Sem nome";
}

function obterTipoItem(item) {
    return item?.categoria ?? item?.Categoria ?? "-";
}

function obterPrecoItem(item) {
    return item?.precoBase ?? item?.PrecoBase ?? 0;
}

function obterDetalhesItem(item) {
    return item?.detalhes ?? item?.Detalhes ?? "-";
}

function renderizarItens(itens) {
    const tabela = document.getElementById("tabelaCardapio");
    if (!tabela) return;

    if (!itens.length) {
        tabela.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; color: var(--texto-secundario); padding: 24px;">
                    Nenhum item encontrado no cardápio.
                </td>
            </tr>
        `;
        return;
    }

    tabela.innerHTML = itens.map(item => {
        const cat = obterTipoItem(item);
        const classeTag = cat.toLowerCase() === "prato" ? "tag-prato" : (cat.toLowerCase() === "bebida" ? "tag-bebida" : "tag-sobremesa");

        return `
            <tr>
                <td><strong>#${obterId(item) ?? "-"}</strong></td>
                <td>${obterNomeItem(item)}</td>
                <td><span class="badge ${classeTag}">${cat}</span></td>
                <td><strong>${formatarMoeda(obterPrecoItem(item))}</strong></td>
                <td><em>${obterDetalhesItem(item)}</em></td>
            </tr>
        `;
    }).join("");
}

async function cadastrarItem(evento) {
    evento.preventDefault();

    const tipoRaw = document.getElementById("tipoItem")?.value ?? "";
    const nome = document.getElementById("nomeItem")?.value?.trim() ?? "";
    const preco = Number(document.getElementById("precoItem")?.value?.replace(",", ".") ?? 0);

    if (!tipoRaw || !nome || !Number.isFinite(preco) || preco <= 0) {
        mostrarMensagem("Preencha corretamente o tipo, nome e preço base.", "erro");
        return;
    }

    const mapaTipos = {
        prato: "Prato",
        bebida: "Bebida",
        sobremesa: "Sobremesa"
    };
    const tipo = mapaTipos[tipoRaw.toLowerCase()] || tipoRaw;

    const dados = {
        tipo: tipo,
        nome: nome,
        precoBase: preco
    };

    if (tipo === "Prato") {
        const tempo = Number(document.getElementById("tempoPreparo")?.value);
        if (!Number.isFinite(tempo) || tempo <= 0) {
            mostrarMensagem("Informe um tempo de preparo válido em minutos.", "erro");
            return;
        }
        dados.tempoPreparo = tempo;
    } else if (tipo === "Bebida") {
        const volume = Number(document.getElementById("volumeBebida")?.value);
        if (!Number.isFinite(volume) || volume <= 0) {
            mostrarMensagem("Informe um volume válido em ml.", "erro");
            return;
        }
        dados.volumeMl = volume;
    } else if (tipo === "Sobremesa") {
        dados.gelada = document.getElementById("refrigerada")?.value === "true";
    }

    try {
        await requisicao("/cardapio", {
            method: "POST",
            body: JSON.stringify(dados)
        });

        mostrarMensagem("Item adicionado ao cardápio com sucesso!", "sucesso");

        document.getElementById("formItem")?.reset();
        atualizarCamposEspecificos();
        document.getElementById("formularioItem")?.classList.add("oculto");

        await carregarItens();
    } catch (erro) {
        mostrarMensagem(`Não foi possível cadastrar o item: ${erro.message}`, "erro");
    }
}