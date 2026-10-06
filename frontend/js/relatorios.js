document.addEventListener("DOMContentLoaded", () => {
    const btnAtualizarRelatorio = document.getElementById("btnAtualizarRelatorio");

    if (btnAtualizarRelatorio) {
        btnAtualizarRelatorio.addEventListener("click", carregarRelatorio);
    }

    carregarRelatorio();
});

async function carregarRelatorio() {
    try {
        const dados = await requisicao("/relatorios/gerencial");
        const relatorio = dados?.data ?? dados;

        preencherResumo(relatorio);
        preencherItensMaisVendidos(relatorio);
        mostrarMensagem("Relatório gerencial atualizado com sucesso.", "sucesso");
    } catch (erro) {
        limparRelatorio();
        mostrarMensagem(`Não foi possível carregar o relatório: ${erro.message}`, "erro");
    }
}

function preencherResumo(relatorio) {
    const faturamento = relatorio?.faturamentoTotal ?? relatorio?.FaturamentoTotal ?? 0;
    const tempoMedio = relatorio?.tempoMedioEntregaMinutos ?? relatorio?.TempoMedioEntregaMinutos ?? null;
    const mensagemTempo = relatorio?.mensagemTempoMedio ?? relatorio?.MensagemTempoMedio ?? "-";
    const pedidosEntregues = relatorio?.totalPedidosEntregues ?? relatorio?.TotalPedidosEntregues ?? 0;
    const pedidosGerais = relatorio?.totalPedidosGeral ?? relatorio?.TotalPedidosGeral ?? relatorio?.totalPedidos ?? relatorio?.TotalPedidos ?? 0;

    const faturamentoElemento = document.getElementById("faturamentoTotal");
    const tempoElemento = document.getElementById("tempoMedioEntrega");
    const mensagemTempoElemento = document.getElementById("mensagemTempoMedio");
    const pedidosEntreguesElemento = document.getElementById("pedidosEntregues");
    const pedidosGeraisElemento = document.getElementById("pedidosGerais");

    if (faturamentoElemento) {
        faturamentoElemento.textContent = formatarMoeda(faturamento);
    }

    if (tempoElemento) {
        tempoElemento.textContent = formatarTempo(tempoMedio);
    }

    if (mensagemTempoElemento) {
        mensagemTempoElemento.textContent = mensagemTempo;
    }

    if (pedidosEntreguesElemento) {
        pedidosEntreguesElemento.textContent = pedidosEntregues;
    }

    if (pedidosGeraisElemento) {
        pedidosGeraisElemento.textContent = pedidosGerais;
    }
}

function preencherItensMaisVendidos(relatorio) {
    const tabela = document.getElementById("tabelaItensMaisVendidos");
    if (!tabela) return;

    const itens = relatorio?.itensMaisVendidos ?? relatorio?.ItensMaisVendidos ?? [];

    if (!Array.isArray(itens) || itens.length === 0) {
        tabela.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; color: var(--texto-secundario); padding: 24px;">
                    Nenhum item vendido ainda nos pedidos registrados.
                </td>
            </tr>
        `;
        return;
    }

    tabela.innerHTML = itens.map((item, index) => {
        const codigo = item?.codigo ?? item?.Codigo ?? "-";
        const nome = item?.nome ?? item?.Nome ?? "Sem nome";
        const categoria = item?.categoria ?? item?.Categoria ?? "-";
        const quantidade = item?.quantidadeVendida ?? item?.QuantidadeVendida ?? 0;
        const totalFaturado = item?.totalVendido ?? item?.TotalVendido ?? item?.totalFaturado ?? item?.TotalFaturado ?? 0;

        const cat = String(categoria).toLowerCase();
        const badgeClasse = cat === "prato" ? "tag-prato" : (cat === "bebida" ? "tag-bebida" : "tag-sobremesa");

        return `
            <tr>
                <td><strong>#${index + 1}</strong> (${codigo})</td>
                <td>${nome}</td>
                <td><span class="badge ${badgeClasse}">${categoria}</span></td>
                <td><strong>${quantidade} un.</strong></td>
                <td><strong>${formatarMoeda(totalFaturado)}</strong></td>
            </tr>
        `;
    }).join("");
}

function limparRelatorio() {
    const faturamentoElemento = document.getElementById("faturamentoTotal");
    const tempoElemento = document.getElementById("tempoMedioEntrega");
    const mensagemTempoElemento = document.getElementById("mensagemTempoMedio");
    const pedidosEntreguesElemento = document.getElementById("pedidosEntregues");
    const pedidosGeraisElemento = document.getElementById("pedidosGerais");
    const tabela = document.getElementById("tabelaItensMaisVendidos");

    if (faturamentoElemento) faturamentoElemento.textContent = "R$ 0,00";
    if (tempoElemento) tempoElemento.textContent = "-";
    if (mensagemTempoElemento) mensagemTempoElemento.textContent = "-";
    if (pedidosEntreguesElemento) pedidosEntreguesElemento.textContent = "0";
    if (pedidosGeraisElemento) pedidosGeraisElemento.textContent = "0";

    if (tabela) {
        tabela.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; color: var(--texto-secundario); padding: 24px;">
                    Não foi possível obter os dados do relatório.
                </td>
            </tr>
        `;
    }
}