document.addEventListener("DOMContentLoaded", () => {

    const btnAtualizarRelatorio =
        document.getElementById(
            "btnAtualizarRelatorio"
        );

    if (btnAtualizarRelatorio) {

        btnAtualizarRelatorio.addEventListener(
            "click",
            carregarRelatorio
        );
    }

    carregarRelatorio();
});


async function carregarRelatorio() {

    try {

        mostrarMensagemRelatorio(
            "Carregando relatório...",
            "info"
        );

        const dados =
            await requisicao(
                "/relatorios/gerencial"
            );

        const relatorio =
            dados?.data ??
            dados;

        preencherResumo(relatorio);

        preencherItensMaisVendidos(
            relatorio
        );

        mostrarMensagemRelatorio(
            "Relatório atualizado com sucesso.",
            "sucesso"
        );

    } catch (erro) {

        limparRelatorio();

        mostrarMensagemRelatorio(
            `Não foi possível carregar o relatório: ${erro.message}`,
            "erro"
        );
    }
}


function preencherResumo(relatorio) {

    const faturamento =
        relatorio?.faturamentoTotal ??
        relatorio?.FaturamentoTotal ??
        0;

    const tempoMedio =
        relatorio?.tempoMedioEntregaMinutos ??
        relatorio?.TempoMedioEntregaMinutos ??
        0;

    const mensagemTempo =
        relatorio?.mensagemTempoMedio ??
        relatorio?.MensagemTempoMedio ??
        "-";

    const pedidosEntregues =
        relatorio?.totalPedidosEntregues ??
        relatorio?.TotalPedidosEntregues ??
        0;

    const pedidosGerais =
        relatorio?.totalPedidosGeral ??
        relatorio?.TotalPedidosGeral ??
        0;


    const faturamentoElemento =
        document.getElementById(
            "faturamentoTotal"
        );

    const tempoElemento =
        document.getElementById(
            "tempoMedioEntrega"
        );

    const mensagemTempoElemento =
        document.getElementById(
            "mensagemTempoMedio"
        );

    const pedidosEntreguesElemento =
        document.getElementById(
            "pedidosEntregues"
        );

    const pedidosGeraisElemento =
        document.getElementById(
            "pedidosGerais"
        );


    if (faturamentoElemento) {

        faturamentoElemento.textContent =
            formatarMoeda(faturamento);
    }


    if (tempoElemento) {

        tempoElemento.textContent =
            formatarTempo(tempoMedio);
    }


    if (mensagemTempoElemento) {

        mensagemTempoElemento.textContent =
            mensagemTempo;
    }


    if (pedidosEntreguesElemento) {

        pedidosEntreguesElemento.textContent =
            pedidosEntregues;
    }


    if (pedidosGeraisElemento) {

        pedidosGeraisElemento.textContent =
            pedidosGerais;
    }
}


function preencherItensMaisVendidos(
    relatorio
) {

    const tabela =
        document.getElementById(
            "tabelaItensMaisVendidos"
        );

    if (!tabela) {
        return;
    }


    const itens =
        relatorio?.itensMaisVendidos ??
        relatorio?.ItensMaisVendidos ??
        [];


    if (!Array.isArray(itens) ||
        itens.length === 0) {

        tabela.innerHTML = `
            <tr>

                <td colspan="5">
                    Nenhum item vendido ainda.
                </td>

            </tr>
        `;

        return;
    }


    tabela.innerHTML =
        itens.map(item => {

            const codigo =
                item?.codigo ??
                item?.Codigo ??
                "-";

            const nome =
                item?.nome ??
                item?.Nome ??
                "Sem nome";

            const categoria =
                item?.categoria ??
                item?.Categoria ??
                "-";

            const quantidade =
                item?.quantidadeVendida ??
                item?.QuantidadeVendida ??
                0;

            const totalFaturado =
                item?.totalFaturado ??
                item?.TotalFaturado ??
                0;


            return `
                <tr>

                    <td>
                        ${codigo}
                    </td>

                    <td>
                        ${nome}
                    </td>

                    <td>
                        ${categoria}
                    </td>

                    <td>
                        ${quantidade}
                    </td>

                    <td>
                        ${formatarMoeda(
                            totalFaturado
                        )}
                    </td>

                </tr>
            `;

        }).join("");
}


function limparRelatorio() {

    const faturamento =
        document.getElementById(
            "faturamentoTotal"
        );

    const tempo =
        document.getElementById(
            "tempoMedioEntrega"
        );

    const mensagemTempo =
        document.getElementById(
            "mensagemTempoMedio"
        );

    const pedidosEntregues =
        document.getElementById(
            "pedidosEntregues"
        );

    const pedidosGerais =
        document.getElementById(
            "pedidosGerais"
        );

    const tabela =
        document.getElementById(
            "tabelaItensMaisVendidos"
        );


    if (faturamento) {
        faturamento.textContent =
            "R$ 0,00";
    }

    if (tempo) {
        tempo.textContent =
            "0 min";
    }

    if (mensagemTempo) {
        mensagemTempo.textContent =
            "-";
    }

    if (pedidosEntregues) {
        pedidosEntregues.textContent =
            "0";
    }

    if (pedidosGerais) {
        pedidosGerais.textContent =
            "0";
    }

    if (tabela) {

        tabela.innerHTML = `
            <tr>

                <td colspan="5">
                    Não foi possível carregar os dados.
                </td>

            </tr>
        `;
    }
}


function mostrarMensagemRelatorio(
    mensagem,
    tipo
) {

    const elemento =
        document.getElementById(
            "mensagemRelatorio"
        );

    if (!elemento) {
        return;
    }


    let classe =
        "mensagem-info";


    if (tipo === "sucesso") {

        classe =
            "mensagem-sucesso";

    } else if (tipo === "erro") {

        classe =
            "mensagem-erro";
    }


    elemento.innerHTML = `
        <div class="mensagem ${classe}">
            ${mensagem}
        </div>
    `;


    if (tipo === "sucesso") {

        setTimeout(() => {

            elemento.innerHTML = "";

        }, 3000);
    }
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


function formatarTempo(minutos) {

    const valor =
        Number(minutos);


    if (!Number.isFinite(valor) ||
        valor <= 0) {

        return "0 min";
    }


    if (valor < 60) {

        return `${valor.toFixed(1)} min`;
    }


    const horas =
        Math.floor(valor / 60);

    const minutosRestantes =
        Math.round(valor % 60);


    if (minutosRestantes === 0) {

        return `${horas}h`;
    }


    return `${horas}h ${minutosRestantes}min`;
}