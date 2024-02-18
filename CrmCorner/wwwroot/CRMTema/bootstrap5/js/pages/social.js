$(function () {
  ("use strict");

    // Socail Statistics Chart
    var options = {
        series: [
            {
                name: "Facebook",
                data: [
                2350,
                3205,
                3520,
                2351,
                3632,
                3205,
                2541,
                2583,
                1592,
                2674,
                2323,
                2583,
                ],
            },
            {
                name: "Linkedin",
                data: [
                2541,
                2583,
                1592,
                2674,
                2323,
                2583,
                2350,
                3205,
                3520,
                2351,
                3632,
                3205,
                ],
            },
            {
                name: "Twitter",
                data: [
                1212,
                3214,
                2325,
                4235,
                2519,
                3214,
                2541,
                2583,
                1592,
                2674,
                2323,
                2583,
                ],
            },
        ],
        colors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)'],
        chart: {
            type: "bar",
            height: 350,
            stacked: true,
            toolbar: {
                show: false,
            },
            zoom: {
                enabled: false,
            },
        },
        responsive: [
            {
                breakpoint: 480,
                options: {
                legend: {
                    position: "bottom",
                    offsetX: -10,
                    offsetY: 0,
                },
                },
            },
        ],
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: '55%',
                endingShape: 'rounded'
            },
        },
        dataLabels: {
            enabled: false,
        },
        xaxis: {
            // type: "datetime",
            categories: [ "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ],
        },
        legend: {
            position: "top",
            horizontalAlign: "right",
            offsetY: 0,
        },
        fill: {
            opacity: 1,
        },
    };
    var chart = new ApexCharts(document.querySelector("#social_statistics"),options); chart.render();
});