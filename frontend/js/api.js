const API_BASE_URL = window.location.origin.includes(":5173")
    ? "/api"
    : (window.location.protocol.startsWith("http")
        ? `${window.location.origin}/api`
        : "http://localhost:5173/api");

async function requisicao(url, opcoes = {}) {
    try {
        const rotaFormatada = url.startsWith("/") ? url : `/${url}`;
        const resposta = await fetch(`${API_BASE_URL}${rotaFormatada}`, {
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
                dados?.erro ||
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

let timeoutMensagem = null;

function mostrarMensagem(mensagem, tipo = "info") {
    const existente = document.querySelector(".mensagem");
    if (existente) {
        existente.remove();
    }
    if (timeoutMensagem) {
        clearTimeout(timeoutMensagem);
    }

    const icones = {
        sucesso: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink:0;margin-right:10px;"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>`,
        erro: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink:0;margin-right:10px;"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>`,
        info: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink:0;margin-right:10px;"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>`
    };

    const elemento = document.createElement("div");
    elemento.className = `mensagem mensagem-${tipo}`;
    elemento.innerHTML = `
        ${icones[tipo] || icones.info}
        <span class="mensagem-texto">${mensagem}</span>
        <button type="button" class="mensagem-fechar" aria-label="Fechar">&times;</button>
    `;

    elemento.querySelector(".mensagem-fechar").addEventListener("click", () => {
        elemento.classList.add("mensagem-saindo");
        setTimeout(() => elemento.remove(), 250);
    });

    document.body.appendChild(elemento);

    timeoutMensagem = setTimeout(() => {
        if (elemento.parentElement) {
            elemento.classList.add("mensagem-saindo");
            setTimeout(() => elemento.remove(), 250);
        }
    }, 4500);
}

function formatarMoeda(valor) {
    return Number(valor || 0).toLocaleString("pt-BR", {
        style: "currency",
        currency: "BRL"
    });
}

function formatarTempo(minutos) {
    if (minutos === null || minutos === undefined || minutos === "") {
        return "-";
    }
    const num = Number(minutos);
    if (isNaN(num)) return "-";
    return `${num.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} min`;
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

document.addEventListener("DOMContentLoaded", () => {
    inicializarTema();
    inicializarCustomSelects();
});

function inicializarCustomSelects() {
    const selects = document.querySelectorAll("select");
    selects.forEach(select => {
        if (select.dataset.customDropdown === "true") return;
        criarCustomDropdown(select);
    });

    document.addEventListener("click", (e) => {
        if (!e.target.closest(".custom-select-wrapper")) {
            fecharTodosDropdowns();
        }
    });
}

function fecharTodosDropdowns() {
    document.querySelectorAll(".custom-select-wrapper.aberto").forEach(wrapper => {
        wrapper.classList.remove("aberto");
        const painel = wrapper.querySelector(".custom-select-painel");
        if (painel) painel.style.display = "none";
    });
}

function criarCustomDropdown(select) {
    select.dataset.customDropdown = "true";

    const wrapper = document.createElement("div");
    wrapper.className = "custom-select-wrapper select-oculto";

    select.parentNode.insertBefore(wrapper, select);
    wrapper.appendChild(select);

    const trigger = document.createElement("button");
    trigger.type = "button";
    trigger.className = "custom-select-trigger";
    trigger.innerHTML = `
        <span class="custom-select-texto"></span>
        <svg class="custom-select-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m6 9 6 6 6-6"/>
        </svg>
    `;

    const textoSpan = trigger.querySelector(".custom-select-texto");

    const painel = document.createElement("ul");
    painel.className = "custom-select-painel";
    painel.style.display = "none";

    wrapper.appendChild(trigger);
    wrapper.appendChild(painel);

    function atualizarOpcoes() {
        painel.innerHTML = "";
        const options = Array.from(select.options);

        let optionSelecionada = select.options[select.selectedIndex];
        if (!optionSelecionada && options.length > 0) {
            optionSelecionada = options[0];
        }

        if (optionSelecionada) {
            textoSpan.textContent = optionSelecionada.textContent;
            if (!optionSelecionada.value) {
                textoSpan.classList.add("placeholder");
            } else {
                textoSpan.classList.remove("placeholder");
            }
        } else {
            textoSpan.textContent = "Selecione...";
            textoSpan.classList.add("placeholder");
        }

        options.forEach(opt => {
            const li = document.createElement("li");
            li.className = "custom-select-opcao";
            if (opt.disabled) li.classList.add("desabilitada");
            if (opt === optionSelecionada) li.classList.add("selecionada");

            const checkSvg = (opt === optionSelecionada && opt.value)
                ? `<svg class="custom-select-check" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>`
                : "";

            li.innerHTML = `
                <span>${opt.textContent}</span>
                ${checkSvg}
            `;

            if (!opt.disabled) {
                li.addEventListener("click", (e) => {
                    e.stopPropagation();
                    select.value = opt.value;
                    select.dispatchEvent(new Event("change", { bubbles: true }));
                    select.dispatchEvent(new Event("input", { bubbles: true }));
                    atualizarOpcoes();
                    fecharTodosDropdowns();
                });
            }

            painel.appendChild(li);
        });
    }

    trigger.addEventListener("click", (e) => {
        e.stopPropagation();
        const estavaAberto = wrapper.classList.contains("aberto");
        fecharTodosDropdowns();
        if (!estavaAberto) {
            wrapper.classList.add("aberto");
            painel.style.display = "block";
        }
    });

    select.addEventListener("change", () => {
        atualizarOpcoes();
    });

    const formPai = select.closest("form");
    if (formPai) {
        formPai.addEventListener("reset", () => {
            setTimeout(() => atualizarOpcoes(), 15);
        });
    }

    const observer = new MutationObserver(() => {
        atualizarOpcoes();
    });
    observer.observe(select, { childList: true });

    atualizarOpcoes();
}

function alternarTema() {
    const temaAtual = document.documentElement.getAttribute("data-tema") || "claro";
    const novoTema = temaAtual === "escuro" ? "claro" : "escuro";
    aplicarTema(novoTema);
}

function aplicarTema(tema) {
    document.documentElement.setAttribute("data-tema", tema);
    try {
        localStorage.setItem("delivery_tema", tema);
    } catch (e) {
        console.warn("Não foi possível salvar o tema no localStorage:", e);
    }

    const btn = document.getElementById("btnTemaToggle");
    if (btn) {
        btn.setAttribute("title", tema === "escuro" ? "Alternar para tema claro" : "Alternar para tema escuro");
        btn.setAttribute("aria-label", tema === "escuro" ? "Alternar para tema claro" : "Alternar para tema escuro");
    }
}

function inicializarTema() {
    let tema = "claro";
    try {
        const salvo = localStorage.getItem("delivery_tema");
        if (salvo === "escuro" || salvo === "claro") {
            tema = salvo;
        } else if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
            tema = "escuro";
        }
    } catch (e) {
        tema = "claro";
    }

    aplicarTema(tema);

    const btn = document.getElementById("btnTemaToggle");
    if (btn) {
        btn.addEventListener("click", () => {
            alternarTema();
        });
    }

    if (window.matchMedia) {
        window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", (e) => {
            if (!localStorage.getItem("delivery_tema")) {
                aplicarTema(e.matches ? "escuro" : "claro");
            }
        });
    }
}