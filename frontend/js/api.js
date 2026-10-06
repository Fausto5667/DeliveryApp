const API_BASE_URL = "http://localhost:5173/api";

async function requisicao(url, opcoes = {}) {
    try {
        const resposta = await fetch(`${API_BASE_URL}${url}`, {
            headers: {
                "Content-Type": "application/json",
                ...(opcoes.headers || {})
            },
            ...opcoes
        });

        const texto = await resposta.text();

        let dados = null;

        if (texto) {
            try {
                dados = JSON.parse(texto);
            } catch {
                dados = texto;
            }
        }

        if (!resposta.ok) {
            const mensagem =
                dados?.message ||
                dados?.mensagem ||
                dados?.title ||
                "Ocorreu um erro na comunicação com a API.";

            throw new Error(mensagem);
        }

        return dados;
    } catch (erro) {
        console.error("Erro na API:", erro);
        throw erro;
    }
}

function mostrarMensagem(mensagem, tipo = "info") {
    const existente = document.querySelector(".mensagem");

    if (existente) {
        existente.remove();
    }

    const elemento = document.createElement("div");

    elemento.className = `mensagem mensagem-${tipo}`;
    elemento.textContent = mensagem;

    const container = document.querySelector(".container");

    if (container) {
        container.prepend(elemento);
    }
}

function formatarMoeda(valor) {
    return Number(valor || 0).toLocaleString("pt-BR", {
        style: "currency",
        currency: "BRL"
    });
}

function obterId(objeto) {
    return objeto?.id ??
        objeto?.codigo ??
        objeto?.Id ??
        objeto?.Codigo;
}

function obterLista(dados) {
    if (Array.isArray(dados)) {
        return dados;
    }

    if (Array.isArray(dados?.data)) {
        return dados.data;
    }

    if (Array.isArray(dados?.items)) {
        return dados.items;
    }

    if (Array.isArray(dados?.result)) {
        return dados.result;
    }

    return [];
}