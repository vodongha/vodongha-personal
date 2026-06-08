window.healthChart = (function () {
    const instances = {};

    function makeGradient(ctx, color) {
        const gradient = ctx.createLinearGradient(0, 0, 0, 220);
        gradient.addColorStop(0, color.replace(')', ', 0.25)').replace('rgb', 'rgba'));
        gradient.addColorStop(1, color.replace(')', ', 0)').replace('rgb', 'rgba'));
        return gradient;
    }

    function getThemeColors() {
        const isLight = document.documentElement.getAttribute('data-theme') === 'light';
        return {
            grid:    isLight ? 'rgba(0,0,0,0.07)'  : '#1f1f1f',
            ticks:   isLight ? '#64748b'            : '#4b5563',
            ttBg:    isLight ? '#ffffff'            : '#1a1a1a',
            ttBorder:isLight ? '#e2e8f0'            : '#2a2a2a',
            ttTitle: isLight ? '#64748b'            : '#9ca3af',
            ttBody:  isLight ? '#0f172a'            : '#e5e7eb',
        };
    }

    function buildConfig(labels, values, lineColor, label, unit) {
        const c = getThemeColors();
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
                        const { ctx: ch, chartArea } = chart;
                        if (!chartArea) return 'transparent';
                        const gradient = ch.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
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
                        backgroundColor: c.ttBg,
                        borderColor: c.ttBorder,
                        borderWidth: 1,
                        titleColor: c.ttTitle,
                        bodyColor: c.ttBody,
                        callbacks: {
                            label: function (ctx) {
                                return ' ' + ctx.parsed.y + ' ' + unit;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { color: c.grid },
                        ticks: {
                            color: c.ticks,
                            font: { size: 10 },
                            maxRotation: 0,
                            maxTicksLimit: 6,
                        }
                    },
                    y: {
                        grid: { color: c.grid },
                        ticks: {
                            color: c.ticks,
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
        },

        // Rebuild all active charts with new theme colors (call after theme toggle)
        onThemeChange: function () {
            for (const id of Object.keys(instances)) {
                const chart = instances[id];
                if (!chart) continue;
                const th = getThemeColors();
                // Update grid + tick colors
                chart.options.scales.x.grid.color  = th.grid;
                chart.options.scales.y.grid.color  = th.grid;
                chart.options.scales.x.ticks.color = th.ticks;
                chart.options.scales.y.ticks.color = th.ticks;
                // Update tooltip colors
                chart.options.plugins.tooltip.backgroundColor = th.ttBg;
                chart.options.plugins.tooltip.borderColor     = th.ttBorder;
                chart.options.plugins.tooltip.titleColor      = th.ttTitle;
                chart.options.plugins.tooltip.bodyColor       = th.ttBody;
                chart.update('none'); // 'none' = skip animation
            }
        }
    };
})();
