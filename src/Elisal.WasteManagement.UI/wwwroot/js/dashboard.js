window.dashboardCharts = {
    renderLineChart: function (canvasId, labels, data) {
        const ctx = document.getElementById(canvasId).getContext('2d');
        if (window.myLineChart) window.myLineChart.destroy();

        window.myLineChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Volume (Ton)',
                    data: data,
                    borderColor: '#2f7f34',
                    backgroundColor: 'rgba(47, 127, 52, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: { beginAtZero: true, grid: { display: false } },
                    x: { grid: { display: false } }
                }
            }
        });
    },
    downloadFile: function (filename, contentType, content) {
        const file = new File([content], filename, { type: contentType });
        const exportUrl = URL.createObjectURL(file);
        const a = document.createElement("a");
        document.body.appendChild(a);
        a.href = exportUrl;
        a.download = filename;
        a.target = "_self";
        a.click();
        URL.revokeObjectURL(exportUrl);
        document.body.removeChild(a);
    }
};
