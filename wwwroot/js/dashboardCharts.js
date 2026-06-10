window.dashboardCharts = (() => {
    const _charts = {};

    // Plugin: transparent canvas background (prevents Chart.js default white fill)
    const transparentBg = {
        id: 'transparentBg',
        beforeDraw: (chart) => {
            const ctx = chart.canvas.getContext('2d');
            ctx.save();
            ctx.clearRect(0, 0, chart.width, chart.height);
            ctx.restore();
        }
    };

    const isDark      = () => (document.documentElement.getAttribute('data-theme') ?? 'dark') === 'dark';
    const lineColor   = () => isDark() ? '#6ee7b7' : '#059669';
    const barColor    = () => isDark() ? 'rgba(110,231,183,0.75)' : 'rgba(5,150,105,0.75)';
    const barBorder   = () => isDark() ? '#6ee7b7' : '#059669';
    const gridColor   = () => isDark() ? 'rgba(255,255,255,0.07)' : 'rgba(0,0,0,0.08)';
    const tickColor   = () => isDark() ? '#9ca3af' : '#4b5563';
    const tooltipBg   = () => isDark() ? '#1f2937' : '#ffffff';
    const tooltipTxt  = () => isDark() ? '#f9fafb' : '#111827';
    const tooltipBorder = () => isDark() ? '#374151' : '#e5e7eb';

    // Gradient for line chart — recreated on each render so it respects chart dimensions
    const makeGradient = (canvas) => {
        const h = canvas.parentElement?.clientHeight || canvas.offsetHeight || 160;
        const grad = canvas.getContext('2d').createLinearGradient(0, 0, 0, h);
        if (isDark()) {
            grad.addColorStop(0,   'rgba(110,231,183,0.45)');
            grad.addColorStop(0.6, 'rgba(110,231,183,0.08)');
            grad.addColorStop(1,   'rgba(110,231,183,0)');
        } else {
            grad.addColorStop(0,   'rgba(5,150,105,0.25)');
            grad.addColorStop(0.6, 'rgba(5,150,105,0.05)');
            grad.addColorStop(1,   'rgba(5,150,105,0)');
        }
        return grad;
    };

    const CATEGORY_COLORS = [
        '#6ee7b7', '#60a5fa', '#f59e0b', '#f472b6', '#a78bfa',
        '#34d399', '#fb923c', '#38bdf8', '#e879f9', '#facc15'
    ];

    const destroy = (id) => {
        if (_charts[id]) { _charts[id].destroy(); delete _charts[id]; }
    };

    const renderDonut = (id, labels, data) => {
        destroy(id);
        const canvas = document.getElementById(id);
        if (!canvas) return;
        _charts[id] = new Chart(canvas, {
            plugins: [transparentBg],
            type: 'doughnut',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: CATEGORY_COLORS.slice(0, labels.length),
                    borderWidth: 2,
                    borderColor: isDark() ? '#141414' : '#ffffff',
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: tickColor(),
                            padding: 14,
                            font: { size: 12 },
                            boxWidth: 12,
                            boxHeight: 12
                        }
                    },
                    tooltip: {
                        backgroundColor: tooltipBg(),
                        titleColor: tooltipTxt(),
                        bodyColor: tooltipTxt(),
                        borderColor: tooltipBorder(),
                        borderWidth: 1,
                        callbacks: { label: ctx => ` ${ctx.parsed} skills` }
                    }
                }
            }
        });
    };

    const renderHBar = (id, labels, data) => {
        destroy(id);
        const canvas = document.getElementById(id);
        if (!canvas) return;
        _charts[id] = new Chart(canvas, {
            plugins: [transparentBg],
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    data,
                    backgroundColor: barColor(),
                    borderColor: barBorder(),
                    borderWidth: 1,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                indexAxis: 'y',
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: tooltipBg(),
                        titleColor: tooltipTxt(),
                        bodyColor: tooltipTxt(),
                        borderColor: tooltipBorder(),
                        borderWidth: 1,
                        callbacks: { label: ctx => ` ${ctx.parsed.x} views` }
                    }
                },
                scales: {
                    x: {
                        grid: { color: gridColor() },
                        ticks: { color: tickColor(), font: { size: 11 } },
                        beginAtZero: true
                    },
                    y: {
                        grid: { display: false },
                        ticks: {
                            color: tickColor(),
                            font: { size: 11 },
                            // `this` refers to the Scale instance — must use function(), not arrow
                            callback: function(val) {
                                const label = this.getLabelForValue(val);
                                return label.length > 28 ? `${label.substring(0, 26)}…` : label;
                            }
                        }
                    }
                }
            }
        });
    };

    const renderLine = (id, labels, data) => {
        destroy(id);
        const canvas = document.getElementById(id);
        if (!canvas) return;

        _charts[id] = new Chart(canvas, {
            plugins: [
                transparentBg,
                // Rebuild gradient after Chart.js resizes the canvas
                {
                    id: 'gradientRefresh',
                    beforeUpdate: (chart) => {
                        chart.data.datasets[0].backgroundColor     = makeGradient(chart.canvas);
                        chart.data.datasets[0].borderColor         = lineColor();
                        chart.data.datasets[0].pointBackgroundColor = lineColor();
                    }
                }
            ],
            type: 'line',
            data: {
                labels,
                datasets: [{
                    data,
                    fill: true,
                    backgroundColor: makeGradient(canvas),
                    borderColor: lineColor(),
                    borderWidth: 2,
                    pointRadius: 3,
                    pointBackgroundColor: lineColor(),
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: tooltipBg(),
                        titleColor: tooltipTxt(),
                        bodyColor: tooltipTxt(),
                        borderColor: tooltipBorder(),
                        borderWidth: 1,
                        callbacks: { label: ctx => ` ${ctx.parsed.y} views` }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: tickColor(), font: { size: 11 }, maxTicksLimit: 7 }
                    },
                    y: {
                        grid: { color: gridColor() },
                        ticks: { color: tickColor(), font: { size: 11 } },
                        beginAtZero: true
                    }
                }
            }
        });
    };

    const onThemeChange = () => {
        Object.keys(_charts).forEach(id => {
            const chart = _charts[id];
            if (!chart) return;
            const ds = chart.data.datasets[0];

            if (chart.config.type === 'doughnut') {
                ds.borderColor = isDark() ? '#141414' : '#ffffff';
            } else if (chart.config.type === 'bar') {
                ds.backgroundColor = barColor();
                ds.borderColor     = barBorder();
            }
            // Line gradient is rebuilt automatically by gradientRefresh plugin on next update

            chart.options.plugins.tooltip.backgroundColor = tooltipBg();
            chart.options.plugins.tooltip.titleColor      = tooltipTxt();
            chart.options.plugins.tooltip.bodyColor       = tooltipTxt();
            chart.options.plugins.tooltip.borderColor     = tooltipBorder();

            if (chart.options.scales) {
                ['x', 'y'].forEach(axis => {
                    const scale = chart.options.scales[axis];
                    if (!scale) return;
                    if (scale.grid) { scale.grid.color = gridColor(); }
                    if (scale.ticks) { scale.ticks.color = tickColor(); }
                });
            }
            if (chart.options.plugins.legend?.labels) {
                chart.options.plugins.legend.labels.color = tickColor();
            }
            chart.update();
        });
    };

    return { renderDonut, renderHBar, renderLine, destroy, onThemeChange };
})();
