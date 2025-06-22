$(function () {
    // Department Bar Chart
    const ctxBar = document.getElementById('departmentBarChart');

    if (ctxBar && typeof dashboardChartData !== 'undefined' && dashboardChartData.departmentData && dashboardChartData.departmentData.Labels && dashboardChartData.departmentData.Labels.length > 0) {
        new Chart(ctxBar, {
            type: 'bar',
            data: {
                labels: dashboardChartData.departmentData.Labels,
                datasets: [{
                    label: '# of Employees',
                    data: dashboardChartData.departmentData.Values,
                    backgroundColor: [
                        'rgba(54, 162, 235, 0.6)',
                        'rgba(75, 192, 192, 0.6)',
                        'rgba(255, 206, 86, 0.6)',
                        'rgba(153, 102, 255, 0.6)',
                        'rgba(255, 159, 64, 0.6)',
                        'rgba(255, 99, 132, 0.6)',
                        'rgba(201, 203, 207, 0.6)'
                    ],
                    borderColor: [
                        'rgb(54, 162, 235)',
                        'rgb(75, 192, 192)',
                        'rgb(255, 206, 86)',
                        'rgb(153, 102, 255)',
                        'rgb(255, 159, 64)',
                        'rgb(255, 99, 132)',
                        'rgb(201, 203, 207)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            precision: 0
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
    } else if (ctxBar && typeof isUserAdmin !== 'undefined' && isUserAdmin) {
        $(ctxBar).closest('.card-body').find('p.text-muted').text('No department data available or error loading chart.');
    }

    // User Roles Pie Chart
    const ctxPie = document.getElementById('userRolesPieChart');

    if (ctxPie && typeof dashboardChartData !== 'undefined' && dashboardChartData.userRoleData && dashboardChartData.userRoleData.Labels && dashboardChartData.userRoleData.Labels.length > 0) {
        new Chart(ctxPie, {
            type: 'pie',
            data: {
                labels: dashboardChartData.userRoleData.Labels,
                datasets: [{
                    label: 'User Roles',
                    data: dashboardChartData.userRoleData.Values,
                    backgroundColor: [
                        'rgba(75, 192, 192, 0.7)',
                        'rgba(153, 102, 255, 0.7)',
                        'rgba(255, 99, 132, 0.7)',
                        'rgba(255, 205, 86, 0.7)',
                        'rgba(54, 162, 235, 0.7)'
                    ],
                    borderColor: [
                        'rgb(75, 192, 192)',
                        'rgb(153, 102, 255)',
                        'rgb(255, 99, 132)',
                        'rgb(255, 205, 86)',
                        'rgb(54, 162, 235)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                let label = context.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                if (context.parsed !== null) {
                                    label += context.parsed;
                                }
                                let total = context.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                                let currentValue = context.raw;
                                let percentage = total > 0 ? ((currentValue / total) * 100).toFixed(1) + '%' : '0%';
                                label += ' (' + percentage + ')';
                                return label;
                            }
                        }
                    }
                }
            }
        });
    }
    else if (ctxPie && typeof isUserAdmin !== 'undefined' && isUserAdmin) {
        $(ctxPie).closest('.card-body').find('p.text-muted').text('No user role data available or error loading chart.');
    }
});