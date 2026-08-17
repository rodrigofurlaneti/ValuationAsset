async function carregarDados() {
    try {
        // Substitua pela porta correta que sua API está usando
        const response = await fetch('http://localhost:5263/api/Asset/analysis/graham');
        const dados = await response.json();

        const tbody = document.getElementById('tabela-acoes');
        tbody.innerHTML = '';

        dados.forEach(acao => {
            const row = document.createElement('tr');

            // Define a classe CSS baseada no status
            const classeStatus = acao.valuationStatus.includes('Descontada') ? 'barata' : 'cara';

            row.innerHTML = `
                <td>${acao.stockTicker}</td>
                <td>${acao.companyName}</td>
                <td>R$ ${acao.currentPrice.toFixed(2)}</td>
                <td>R$ ${acao.fairPrice.toFixed(2)}</td>
                <td>${acao.safetyMarginPercentage}%</td>
                <td class="${classeStatus}">${acao.valuationStatus}</td>
            `;
            tbody.appendChild(row);
        });
    } catch (erro) {
        console.error("Erro ao buscar dados:", erro);
        alert("Erro ao conectar na API. Verifique se o backend está rodando.");
    }
}

carregarDados();