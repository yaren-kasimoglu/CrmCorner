$(function () {
    ("use strict");

    // File Reports
    var options = {
        series: [{
            name: "Documents",
            data: [7, 5, 23, 2, 26, 9, 21],
        },
        {
            name: "Media",
            data: [0, 12, 1, 12, 9, 51, 7],
        },
        {
            name: "Images",
            data: [0, 22, 10, 27, 17, 39, 22],
        },],
        chart: {
            height: 300,
            type: "area",
        },
        colors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)'],
        dataLabels: {
            enabled: false,
        },
        stroke: {
            curve: "smooth",
        },
        xaxis: {
            categories: ["2015","2016","2017","2021","2019","2020","2021",],
        },
        tooltip: {
            x: {
                format: "dd/MM/yy HH:mm",
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#file_reports"), options); chart.render();
});