let todasAcoes = [];
let apenasIbovAtivo = false;
let metodologiaAtual = 'graham'; // 'graham', 'bazin', 'magic' ou 'gordon'

const tickersIbov = [
    "VALE3", "PETR4", "PETR3", "ITUB4", "BBDC4", "BBDC3", "B3SA3", "ABEV3",
    "WEGE3", "ITSA4", "BPAC11", "BBAS3", "SANB11", "RADL3", "SUZB3", "JBSS3",
    "RENT3", "EQTL3", "VBBR3", "HAPV3", "COGN3", "LIGT3", "TIMS3", "VIVT3",
    "PRIO3", "RECV3", "CSAN3", "UGPA3", "RAIL3", "CCRO3", "ENEV3", "CMIG4",
    "CPFE3", "EGIE3", "SBSP3", "CSNA3", "USIM5", "GGBR4", "GOAU4", "EMBR3"
];

async function carregarDados() {
    const tbody = document.getElementById('tabela-acoes');
    tbody.innerHTML = `<tr><td colspan="6" class="loading">Carregando dados da metodologia...</td></tr>`;

    try {
        const endpoints = {
            'graham': 'http://localhost:5263/api/Asset/analysis/graham',
            'bazin': 'http://localhost:5263/api/Asset/analysis/bazin',
            'magic': 'http://localhost:5263/api/Asset/analysis/magic-formula',
            'gordon': 'http://localhost:5263/api/Asset/analysis/gordon'
        };

        const response = await fetch(endpoints[metodologiaAtual]);
        if (!response.ok) throw new Error("Erro na resposta da API");

        todasAcoes = await response.json();
        aplicarFiltros();
    } catch (erro) {
        console.error("Erro ao buscar dados:", erro);
        tbody.innerHTML = `<tr><td colspan="6" class="no-results" style="color: var(--danger-text);">Erro ao conectar na API. Verifique se o backend está rodando.</td></tr>`;
    }
}

function mudarMetodologia(tipo) {
    metodologiaAtual = tipo;
    const theadTr = document.getElementById('tableHeaders');
    const titulo = document.getElementById('tituloMetodologia');

    const headers = {
        'graham': ['Ticker', 'Empresa', 'Preço Atual', 'Preço Justo', 'Margem Seg.', 'Status'],
        'bazin': ['Ticker', 'Empresa', 'Preço Atual', 'Preço Teto (6%)', 'Margem Seg.', 'Status'],
        'magic': ['Ticker', 'Empresa', 'Ranking', 'ROIC', 'Earnings Yield', 'Status'],
        'gordon': ['Ticker', 'Empresa', 'Preço Atual', 'Preço Justo', 'Margem Seg.', 'Status']
    };

    const titulos = {
        'graham': '| Benjamin Graham',
        'bazin': '| Décio Bazin',
        'magic': '| Magic Formula (Greenblatt)',
        'gordon': '| Modelo de Gordon'
    };

    titulo.innerText = titulos[tipo];
    theadTr.innerHTML = headers[tipo].map(h => `<th>${h}</th>`).join('');

    carregarDados();
}

function toggleFiltroIbov() {
    apenasIbovAtivo = !apenasIbovAtivo;
    document.getElementById('btnIbov').classList.toggle('active', apenasIbovAtivo);
    aplicarFiltros();
}

function filtrarAcoes() { aplicarFiltros(); }

function aplicarFiltros() {
    const termo = document.getElementById('searchInput').value.toLowerCase();
    let filtradas = todasAcoes.filter(acao => {
        const matchTexto = acao.stockTicker?.toLowerCase().includes(termo) ||
            acao.companyName?.toLowerCase().includes(termo);
        const matchIbov = apenasIbovAtivo ? tickersIbov.includes(acao.stockTicker) : true;
        return matchTexto && matchIbov;
    });
    renderizarTabela(filtradas);
}

function renderizarTabela(dados) {
    const tbody = document.getElementById('tabela-acoes');
    tbody.innerHTML = '';

    if (dados.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="no-results">Nenhuma ação encontrada.</td></tr>`;
        return;
    }

    dados.forEach(acao => {
        const row = document.createElement('tr');

        if (metodologiaAtual === 'magic') {
            const ranking = acao.ranking || 0;
            const classeStatus = ranking <= 20 ? 'barata' : 'cara';
            row.innerHTML = `
                <td><strong>${acao.stockTicker}</strong></td>
                <td>${acao.companyName}</td>
                <td style="color: var(--primary);"><strong>${ranking}º</strong></td>
                <td>${acao.roic}%</td>
                <td>${acao.earningsYield}%</td>
                <td><span class="badge ${classeStatus}">${ranking <= 20 ? 'Top 20' : 'Fora do Top'}</span></td>
            `;
        } else {
            const precoJusto = acao.fairPrice ?? acao.fairPriceBazin ?? acao.fairPriceGordon ?? 0;
            const status = acao.valuationStatus || "Indefinido";
            const isBarata = status.toLowerCase().includes('descontada') || status.toLowerCase().includes('excelente') || status.toLowerCase().includes('preço justo');

            row.innerHTML = `
                <td><strong>${acao.stockTicker}</strong></td>
                <td>${acao.companyName}</td>
                <td>R$ ${acao.currentPrice?.toFixed(2) ?? '0.00'}</td>
                <td style="color: var(--success-text);">R$ ${precoJusto.toFixed(2)}</td>
                <td>${acao.safetyMarginPercentage ?? 0}%</td>
                <td><span class="badge ${isBarata ? 'barata' : 'cara'}">${status}</span></td>
            `;
        }
        tbody.appendChild(row);
    });
}

carregarDados();