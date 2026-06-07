window.healthChart = (function () {
    const instances = {};

    function makeGradient(ctx, color) {
        const gradient = ctx.createLinearGradient(0, 0, 0, 220);
        gradient.addColorStop(0, color.replace(')', ', 0.25)').replace('rgb', 'rgba'));
        gradient.addColorStop(1, color.replace(')', ', 0)').replace('rgb', 'rgba'));
        return gradient;
    }

    function buildConfig(labels, values, lineColor, label, unit) {
        return {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: label,
                    data: values,
                    borderColor: lineColor,
                    borderWidth: 2,
                    pointRadius: 3,
                    pointHoverRadius: 5,
                    pointBackgroundColor: lineColor,
                    pointBorderColor: 'transparent',
                    fill: true,
                    backgroundColor: function (context) {
                        const chart = context.chart;
                        const { ctx: c, chartArea } = chart;
                        if (!chartArea) return 'transparent';
                        const gradient = c.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
                        gradient.addColorStop(0, lineColor + '44');
                        gradient.addColorStop(1, lineColor + '00');
                        return gradient;
                    },
                    tension: 0.4,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#1a1a1a',
                        borderColor: '#2a2a2a',
                        borderWidth: 1,
                        titleColor: '#9ca3af',
                        bodyColor: '#e5e7eb',
                        callbacks: {
                            label: function (ctx) {
                                return ' ' + ctx.parsed.y + ' ' + unit;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { color: '#1f1f1f' },
                        ticks: {
                            color: '#4b5563',
                            font: { size: 10 },
                            maxRotation: 0,
                            maxTicksLimit: 6,
                        }
                    },
                    y: {
                        grid: { color: '#1f1f1f' },
                        ticks: {
                            color: '#4b5563',
                            font: { size: 10 },
                            callback: function (val) { return val + ' ' + unit; }
                        },
                        beginAtZero: true,
                    }
                },
                animation: { duration: 400, easing: 'easeInOutQuart' }
            }
        };
    }

    return {
        init: function (canvasId, labels, values, lineColor, label, unit) {
            const canvas = document.getElementById(canvasId);
            if (!canvas) return;
            if (instances[canvasId]) {
                instances[canvasId].destroy();
            }
            instances[canvasId] = new Chart(canvas, buildConfig(labels, values, lineColor, label, unit));
        },

        update: function (canvasId, labels, values) {
            const chart = instances[canvasId];
            if (!chart) return;
            chart.data.labels = labels;
            chart.data.datasets[0].data = values;
            chart.update('active');
        },

        destroy: function (canvasId) {
            if (instances[canvasId]) {
                instances[canvasId].destroy();
                delete instances[canvasId];
            }
        }
    };
})();
