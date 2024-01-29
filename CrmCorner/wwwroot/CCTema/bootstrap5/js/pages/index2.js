$(function () {
    ("use strict");

    // Wrok Report Chart
    var optionsSpark1 = {
        series: [
            {
                name: "Tasks1",
                data: [38, 35, 62, 41, 45, 57, 40, 55, 22, 95],
            },
            {
                name: "Tasks2",
                data: [35, 25, 38, 40, 24, 48, 28, 55, 45, 60],
            },
            {
                name: "Tasks3",
                data: [35, 30, 36, 28, 45, 58, 59, 19, 58, 70],
            },
        ],
        colors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)'],
        chart: {
            type: "area",
            height: 300,
            sparkline: {
                enabled: false,
            },
            toolbar: {
                show: false,
            },
        },
        stroke: {
            show: true,
            curve: "smooth",
            colors: undefined,
            width: 2,
            dashArray: 0,
        },
        dataLabels: {
            enabled: false,
        },
        xaxis: {
            show: true,
            categories: [
                "2012",
                "2013",
                "2014",
                "2015",
                "2016",
                "2017",
                "2021",
                "2019",
                "2020",
                "2021",
            ],
        },
        yaxis: {
            show: true,
        },
        legend: {
            position: "top",
            horizontalAlign: "center",
        },
    };
    var chartSpark1 = new ApexCharts(document.querySelector("#wrok_report"), optionsSpark1); chartSpark1.render();

    // Project List Table small Chart
    var options = {
        chart: {
            height: 40,
            width: 100,
            type: "line",
            stacked: false,
            sparkline: {
                enabled: true,
            },
        },
        series: [{data: [3, 5, 1, 6, 5, 4, 8, 3],},],
        colors: ['var(--chart-color1)'],
        stroke: {
            width: [1],
        },
        xaxis: {
            categories: [2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016],
        },
    };
    var chart = new ApexCharts(document.querySelector("#aplino"), options); chart.render();
    // Compass
    var options = {
        chart: {
            height: 40,
            width: 100,
            type: "line",
            stacked: false,
            sparkline: {
                enabled: true,
            },
        },
        series: [{data: [4, 6, 3, 2, 5, 6, 5, 4],},],
        colors: ['var(--chart-color1)'],
        stroke: {
            width: [1],
        },
        xaxis: {
            categories: [2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016],
        },
    };
    var chart = new ApexCharts(document.querySelector("#compass"), options); chart.render();
    // Nexa
    var options = {
        chart: {
            height: 40,
            width: 100,
            type: "line",
            stacked: false,
            sparkline: {
                enabled: true,
            },
        },
        series: [{data: [7, 3, 2, 1, 5, 4, 6, 8],},],
        colors: ['var(--chart-color1)'],
        stroke: {
            width: [1],
        },
        xaxis: {
            categories: [2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016],
        },
    };
    var chart = new ApexCharts(document.querySelector("#nexa"), options); chart.render();
    // Oreo
    var options = {
        chart: {
            height: 40,
            width: 100,
            type: "line",
            stacked: false,
            sparkline: {
                enabled: true,
            },
        },
        series: [{data: [3, 1, 2, 5, 4, 6, 2, 3],},],
        colors: ['var(--chart-color1)'],
        stroke: {
            width: [1],
        },
        xaxis: {
            categories: [2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016],
        },
    };
    var chart = new ApexCharts(document.querySelector("#oreo"), options); chart.render();


    // Income Analysis
    var options = {
        series: [44, 55, 13, 43],
        colors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)','var(--chart-color4)'],
        chart: {
            width: 200,
            type: "pie",
        },
        legend: {
            show: false,
        },
        dataLabels: {
            enabled: false,
        },
        labels: ["Quarter 1", "Quarter 2", "Quarter 3", "Quarter 4"],
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 100,
                },
                legend: {
                    show: false,
                    position: "bottom",
                },
            },
        },],
    };
    var chart = new ApexCharts(document.querySelector("#income_analysis"), options); chart.render();
	
	// sales_income
	var optionsSpark1 = {
        series: [{
            name: "Sales",
            data: [2, 4, 3, 1, 5, 7, 3, 2],
        },],
        colors: ['var(--chart-color1)'],
        chart: {
            type: "area",
            height: 90,
            sparkline: {
                enabled: true,
            },
            toolbar: {
                show: false,
            },
        },
        stroke: {
            show: true,
            curve: "smooth",
            colors: undefined,
            width: 0,
            dashArray: 0,
        },
        fill: {
            colors: undefined,
            opacity: 0.5,
            type: 'solid',
        },
        dataLabels: {
            enabled: false,
        },
        xaxis: {
            show: false,
            categories: ["2014","2015","2016","2017","2021","2019","2020","2021",],
        },
    };
    var chartSpark1 = new ApexCharts(document.querySelector("#sales_income"), optionsSpark1); chartSpark1.render();
});