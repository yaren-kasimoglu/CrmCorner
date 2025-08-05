$(function () {
("use strict");

    // Development_income
    var options = {
        series: [
            {
                name: "Development",
                data: [2350, 3205, 4520, 2351, 5632, 3205, 4520],
            },{
                name: "Marketing",
                data: [2541, 2583, 1592, 2674, 2323, 1592, 2674],
            },{
                name: "Developer",
                data: [1212, 5214, 2325, 4235, 2519, 1212, 5214],
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
        },
        dataLabels: {
            enabled: false,
        },
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: "40%",
            },
        },
        stroke: {
            width: 1,
            colors: ["#fff"],
        },
        fill: {
            opacity: 1,
        },
        legend: {
            position: "top",
            horizontalAlign: "right",
        },
        xaxis: {
            categories: ["Q1", "Q2", "Q3", "Q4", "Q5", "Q6", "Q7"],
        },
    };
    var chart = new ApexCharts(document.querySelector("#Development_income"),options);chart.render();

    // Total Income
    var options = {
        series: [44, 55],
        colors: ['var(--chart-color1)','var(--chart-color2)'],
        chart: {
            type: "donut",
        },
        dataLabels: {
            enabled: false,
        },
        plotOptions: {
            pie: {
                startAngle: -110,
                endAngle: 110,
                offsetX: 10,
                donut: {
                    labels: {
                        show: true,
                        name: {
                            offsetY: 5,
                        },
                        total: {
                            show: true,
                            showAlways: true,
                            label: "Total",
                            fontWeight: 600,
                            color: "#000",
                        },
                    },
                },
            },
        },
        grid: {
            padding: {
                bottom: -70,
            },
        },
        legend: {
            show: false,
            position: "top",
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 200,
                },
                legend: {
                    position: "bottom",
                },
            },
        },],
    };
    var chart = new ApexCharts(document.querySelector("#total_income"), options);chart.render();

    // Weekly Income Chart
    var options1 = {
        chart: {
            type: "bar",
            width: "95%",
            height: 30,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 2,
        },
        colors: ['var(--chart-color1)'],
        series: [{
            data: [2, 5, 8, 3, 5, 7, 1, 6],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts(document.querySelector("#weekly_income"), options1).render();

    // Monthly Income Chart
    var options1 = {
        chart: {
            type: "bar",
            width: "95%",
            height: 40,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 2,
        },
        colors: ['var(--chart-color2)'],
        series: [{
            data: [3,1,5,4,7,8,2,3,1,4,6,5,4,,1,5,4,7,8,2,3,2,3,1,4,6,5,4,1],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            y: {
                title: {
                    formatter: function (seriesName) {
                        return "";
                    },
                },
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts(document.querySelector("#monthly_income"), options1).render();

    // Income Analysis
    var options = {
        chart: {
            width: 190,
            height: 190,
            type: "pie",
        },
        series: [6, 4, 8],
        labels: ["Dev", "SEO", "Design"],
        colors: ['var(--chart-color2)','var(--chart-color3)','var(--chart-color1)'],
        legend: {
            show: false,
        },
        dataLabels: {
            enabled: false,
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: 200,
                },
                legend: {
                    position: "bottom",
                },
            },
        },],
    };
    var chart = new ApexCharts(document.querySelector("#sparkline-pie"), options);chart.render();

    // Income Analysis Line Chart
    var optionsSpark1 = {
        series: [{
            name: "Design",
            data: [8, 4, 0, 6, 0, 8, 1, 4, 4, 10, 5, 6],
        },{
            name: "Dev",
            data: [10, 4, 3, 0, 7, 0, 4, 6, 5, 9, 4, 3],
        },{
            name: "SEO",
            data: [9, 5, 0, 6, 2, 5, 8, 9, 1, 5, 6, 2],
        },],
        colors: ['var(--chart-color1)','var(--chart-color3)','var(--chart-color2)'],
        chart: {
            type: "line",
            height: 55,
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
            // lineCap: "butt",
            colors: undefined,
            width: 1,
            dashArray: 0,
        },
        dataLabels: {
            enabled: false,
        },
        tooltip: {
            y: {
                formatter: function (val) {
                return val + "K";
                },
            },
        },
        xaxis: {
            show: true,
            categories: ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"],
        },
        yaxis: {
            show: true,
            categories: ["12K","11K","10K","9K","8K","7K","6K","5K","4K","3K","2K","1K"],
        },
        legend: {
            position: "top",
            horizontalAlign: "center",
        },
    };
    var chartSpark1 = new ApexCharts(document.querySelector("#income_analysis"),optionsSpark1);chartSpark1.render();

    // Visitors Statistics Chart
    var options = {
        annotations: {
        yaxis: [
            {
            y: 30,
            borderColor: "#ffd55d",
            label: {
                show: true,
                text: "Support",
                style: {
                color: "#fff",
                background: "#ffd55d",
                },
            },
            },
        ],
        xaxis: [
            {
            x: new Date("14 Nov 2012").getTime(),
            borderColor: "#ffd55d",
            yAxisIndex: 0,
            label: {
                show: true,
                text: "Rally",
                style: {
                color: "#fff",
                background: "#ffd55d",
                },
            },
            },
        ],
        },
        chart: {
        type: "area",
        height: 288,
        toolbar: {
            show: false,
        },
        },
        colors: ["#ffd55d"],
        dataLabels: {
        enabled: false,
        },

        series: [{
            data: [
            [1327359600000, 30.95],
            [1327446000000, 31.34],
            [1327532400000, 31.18],
            [1327618800000, 31.05],
            [1327878000000, 31.0],
            [1327964400000, 30.95],
            [1328050800000, 31.24],
            [1328137200000, 31.29],
            [1328223600000, 31.85],
            [1328482800000, 31.86],
            [1328569200000, 32.28],
            [1328655600000, 32.1],
            [1328742000000, 32.65],
            [1328828400000, 32.21],
            [1329087600000, 32.35],
            [1329174000000, 32.44],
            [1329260400000, 32.46],
            [1329346800000, 32.86],
            [1329433200000, 32.75],
            [1329778800000, 32.54],
            [1329865200000, 32.33],
            [1329951600000, 32.97],
            [1330038000000, 33.41],
            [1330297200000, 33.27],
            [1330383600000, 33.27],
            [1330470000000, 32.89],
            [1330556400000, 33.1],
            [1330642800000, 33.73],
            [1330902000000, 33.22],
            [1330988400000, 31.99],
            [1331074800000, 32.41],
            [1331161200000, 33.05],
            [1331247600000, 33.64],
            [1331506800000, 33.56],
            [1331593200000, 34.22],
            [1331679600000, 33.77],
            [1331766000000, 34.17],
            [1331852400000, 33.82],
            [1332111600000, 34.51],
            [1332198000000, 33.16],
            [1332284400000, 33.56],
            [1332370800000, 33.71],
            [1332457200000, 33.81],
            [1332712800000, 34.4],
            [1332799200000, 34.63],
            [1332885600000, 34.46],
            [1332972000000, 34.48],
            [1333058400000, 34.31],
            [1333317600000, 34.7],
            [1333404000000, 34.31],
            [1333490400000, 33.46],
            [1333576800000, 33.59],
            [1333922400000, 33.22],
            [1334008800000, 32.61],
            [1334095200000, 33.01],
            [1334181600000, 33.55],
            [1334268000000, 33.18],
            [1334527200000, 32.84],
            [1334613600000, 33.84],
            [1334700000000, 33.39],
            [1334786400000, 32.91],
            [1334872800000, 33.06],
            [1335132000000, 32.62],
            [1335218400000, 32.4],
            [1335304800000, 33.13],
            [1335391200000, 33.26],
            [1335477600000, 33.58],
            [1335736800000, 33.55],
            [1335823200000, 33.77],
            [1335909600000, 33.76],
            [1335996000000, 33.32],
            [1336082400000, 32.61],
            [1336341600000, 32.52],
            [1336428000000, 32.67],
            [1336514400000, 32.52],
            [1336600800000, 31.92],
            [1336687200000, 32.2],
            [1336946400000, 32.23],
            [1337032800000, 32.33],
            [1337119200000, 32.36],
            [1337205600000, 32.01],
            [1337292000000, 31.31],
            [1337551200000, 32.01],
            [1337637600000, 32.01],
            [1337724000000, 32.18],
            [1337810400000, 31.54],
            [1337896800000, 31.6],
            [1338242400000, 32.05],
            [1338328800000, 31.29],
            [1338415200000, 31.05],
            [1338501600000, 29.82],
            [1338760800000, 30.31],
            [1338847200000, 30.7],
            [1338933600000, 31.69],
            [1339020000000, 31.32],
            [1339106400000, 31.65],
            [1339365600000, 31.13],
            [1339452000000, 31.77],
            [1339538400000, 31.79],
            [1339624800000, 31.67],
            [1339711200000, 32.39],
            [1339970400000, 32.63],
            [1340056800000, 32.89],
            [1340143200000, 31.99],
            [1340229600000, 31.23],
            [1340316000000, 31.57],
            [1340575200000, 30.84],
            [1340661600000, 31.07],
            [1340748000000, 31.41],
            [1340834400000, 31.17],
            [1340920800000, 32.37],
            [1341180000000, 32.19],
            [1341266400000, 32.51],
            [1341439200000, 32.53],
            [1341525600000, 31.37],
            [1341784800000, 30.43],
            [1341871200000, 30.44],
            [1341957600000, 30.2],
            [1342044000000, 30.14],
            [1342130400000, 30.65],
            [1342389600000, 30.4],
            [1342476000000, 30.65],
            [1342562400000, 31.43],
            [1342648800000, 31.89],
            [1342735200000, 31.38],
            [1342994400000, 30.64],
            [1343080800000, 30.02],
            [1343167200000, 30.33],
            [1343253600000, 30.95],
            [1343340000000, 31.89],
            [1343599200000, 31.01],
            [1343685600000, 30.88],
            [1343772000000, 30.69],
            [1343858400000, 30.58],
            [1343944800000, 32.02],
            [1344204000000, 32.14],
            [1344290400000, 32.37],
            [1344376800000, 32.51],
            [1344463200000, 32.65],
            [1344549600000, 32.64],
            [1344808800000, 32.27],
            [1344895200000, 32.1],
            [1344981600000, 32.91],
            [1345068000000, 33.65],
            [1345154400000, 33.8],
            [1345413600000, 33.92],
            [1345500000000, 33.75],
            [1345586400000, 33.84],
            [1345672800000, 33.5],
            [1345759200000, 32.26],
            [1346018400000, 32.32],
            [1346104800000, 32.06],
            [1346191200000, 31.96],
            [1346277600000, 31.46],
            [1346364000000, 31.27],
            [1346709600000, 31.43],
            [1346796000000, 32.26],
            [1346882400000, 32.79],
            [1346968800000, 32.46],
            [1347228000000, 32.13],
            [1347314400000, 32.43],
            [1347400800000, 32.42],
            [1347487200000, 32.81],
            [1347573600000, 33.34],
            [1347832800000, 33.41],
            [1347919200000, 32.57],
            [1348005600000, 33.12],
            [1348092000000, 34.53],
            [1348178400000, 33.83],
            [1348437600000, 33.41],
            [1348524000000, 32.9],
            [1348610400000, 32.53],
            [1348696800000, 32.8],
            [1348783200000, 32.44],
            [1349042400000, 32.62],
            [1349128800000, 32.57],
            [1349215200000, 32.6],
            [1349301600000, 32.68],
            [1349388000000, 32.47],
            [1349647200000, 32.23],
            [1349733600000, 31.68],
            [1349820000000, 31.51],
            [1349906400000, 31.78],
            [1349992800000, 31.94],
            [1350252000000, 32.33],
            [1350338400000, 33.24],
            [1350424800000, 33.44],
            [1350511200000, 33.48],
            [1350597600000, 33.24],
            [1350856800000, 33.49],
            [1350943200000, 33.31],
            [1351029600000, 33.36],
            [1351116000000, 33.4],
            [1351202400000, 34.01],
            [1351638000000, 34.02],
            [1351724400000, 34.36],
            [1351810800000, 34.39],
            [1352070000000, 34.24],
            [1352156400000, 34.39],
            [1352242800000, 33.47],
            [1352329200000, 32.98],
            [1352415600000, 32.9],
            [1352674800000, 32.7],
            [1352761200000, 32.54],
            [1352847600000, 32.23],
            [1352934000000, 32.64],
            [1353020400000, 32.65],
            [1353279600000, 32.92],
            [1353366000000, 32.64],
            [1353452400000, 32.84],
            [1353625200000, 33.4],
            [1353884400000, 33.3],
            [1353970800000, 33.18],
            [1354057200000, 33.88],
            [1354143600000, 34.09],
            [1354230000000, 34.61],
            [1354489200000, 34.7],
            [1354575600000, 35.3],
            [1354662000000, 35.4],
            [1354748400000, 35.14],
            [1354834800000, 35.48],
            [1355094000000, 35.75],
            [1355180400000, 35.54],
            [1355266800000, 35.96],
            [1355353200000, 35.53],
            [1355439600000, 37.56],
            [1355698800000, 37.42],
            [1355785200000, 37.49],
            [1355871600000, 38.09],
            [1355958000000, 37.87],
            [1356044400000, 37.71],
            [1356303600000, 37.53],
            [1356476400000, 37.55],
            [1356562800000, 37.3],
            [1356649200000, 36.9],
            [1356908400000, 37.68],
            [1357081200000, 38.34],
            [1357167600000, 37.75],
            [1357254000000, 38.13],
            [1357513200000, 37.94],
            [1357599600000, 38.14],
            [1357686000000, 38.66],
            [1357772400000, 38.62],
            [1357858800000, 38.09],
            [1358118000000, 38.16],
            [1358204400000, 38.15],
            [1358290800000, 37.88],
            [1358377200000, 37.73],
            [1358463600000, 37.98],
            [1358809200000, 37.95],
            [1358895600000, 38.25],
            [1358982000000, 38.1],
            [1359068400000, 38.32],
            [1359327600000, 38.24],
            [1359414000000, 38.52],
            [1359500400000, 37.94],
            [1359586800000, 37.83],
            [1359673200000, 38.34],
            [1359932400000, 38.1],
            [1360018800000, 38.51],
            [1360105200000, 38.4],
            [1360191600000, 38.07],
            [1360278000000, 39.12],
            [1360537200000, 38.64],
            [1360623600000, 38.89],
            [1360710000000, 38.81],
            [1360796400000, 38.61],
            [1360882800000, 38.63],
            [1361228400000, 38.99],
            [1361314800000, 38.77],
            [1361401200000, 38.34],
            [1361487600000, 38.55],
            [1361746800000, 38.11],
            [1361833200000, 38.59],
            [1361919600000, 39.6],
            ],
        },],
        markers: {
        size: 0,
        style: "hollow",
        },
        xaxis: {
        type: "datetime",
        min: new Date("01 Mar 2012").getTime(),
        tickAmount: 6,
        show: false,
        },
        tooltip: {
        x: {
            format: "dd MMM yyyy",
        },
        },
        fill: {
        type: "gradient",
        gradient: {
            shadeIntensity: 1,
            opacityFrom: 0.7,
            opacityTo: 0.9,
            stops: [0, 100],
        },
        },
        stroke: {
        show: true,
        curve: "smooth",
        width: 2,
        },
        grid: {
        yaxis: {
            lines: {
            show: false,
            },
        },
        },
    };
    var chart = new ApexCharts(document.querySelector("#Visitors_chart"),options);chart.render();

    var resetCssClasses = function (activeEl) {
        var els = document.querySelectorAll("button");
        Array.prototype.forEach.call(els, function (el) {
        el.classList.remove("active");
        });

        activeEl.target.classList.add("active");
    };

    document.querySelector("#one_month").addEventListener("click", function (e) {
        resetCssClasses(e);
        chart.updateOptions({
            xaxis: {
                min: new Date("28 Jan 2013").getTime(),
                max: new Date("27 Feb 2013").getTime(),
            },
        });
    });
    document.querySelector("#six_months").addEventListener("click", function (e) {
        resetCssClasses(e);
        chart.updateOptions({
            xaxis: {
                min: new Date("27 Sep 2012").getTime(),
                max: new Date("27 Feb 2013").getTime(),
            },
        });
    });
    document.querySelector("#one_year").addEventListener("click", function (e) {
        resetCssClasses(e);
        chart.updateOptions({
            xaxis: {
                min: new Date("27 Feb 2012").getTime(),
                max: new Date("27 Feb 2013").getTime(),
            },
        });
    });
    document.querySelector("#ytd").addEventListener("click", function (e) {
        resetCssClasses(e);
        chart.updateOptions({
            xaxis: {
                min: new Date("01 Jan 2013").getTime(),
                max: new Date("27 Feb 2013").getTime(),
            },
        });
    });
    document.querySelector("#all").addEventListener("click", function (e) {
        resetCssClasses(e);
        chart.updateOptions({
            xaxis: {
                min: undefined,
                max: undefined,
            },
        });
    });
    document.querySelector("#ytd").addEventListener("click", function () {});

    //  Social Marketing Chart
    var options = {
        series: [{
            name: "Social Marketing",
            data: [1, 8, 2, 5, 6, 7, 3, 4, 1, 9, 3, 7, 2],
        },],
        chart: {
            height: 150,
            type: "area",
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            curve: "straight",
            colors: "#a095e5",
            width: 2,
            dashArray: 0,
        },
        fill: {
            colors: "#a095e5",
            opacity: 1,
            type: "solid",
        },
    };
    var chart = new ApexCharts(document.querySelector("#social_marketing"),options);chart.render();

    // Sales Department Chart
    var options1 = {
        chart: {
            type: "bar",
            width: "80%",
            height: 80,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 2,
        },
        colors: ['var(--chart-color1)'],
        series: [{
            data: [2, 8, 3, 4, 6, 2, 3, 8, 7, 6, 5, 2, 1, 8],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            y: {
                title: {
                formatter: function (seriesName) {
                    return "";
                },
                },
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts( document.querySelector("#sales_department"),options1).render();

    // New Line Chart
    var optionsSpark1 = {
        series: [{
            name: "Design",
            data: [8, 4, 0, 6, 0, 8, 1, 4, 4, 10, 5, 6],
        },
        {
            name: "Dev",
            data: [10, 4, 3, 0, 7, 0, 4, 6, 5, 9, 4, 3],
        },
        {
            name: "SEO",
            data: [9, 5, 0, 6, 2, 5, 8, 9, 1, 5, 6, 2],
        },],
        colors: ['var(--chart-color2)','var(--chart-color3)','var(--chart-color1)'],
        chart: {
            type: "line",
            height: 340,
            toolbar: {
                show: false,
            },
        },
        stroke: {
            show: true,
            curve: "smooth",
            colors: undefined,
            width: 1,
            dashArray: 0,
        },
        dataLabels: {
            enabled: false,
        },
        tooltip: {
            y: {
                formatter: function (val) {
                return val + "K";
                },
            },
        },
        xaxis: {
            show: true,
            categories: ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",],
        },
        yaxis: {
            show: true,
            categories: ["12K","11K","10K","9K","8K","7K","6K","5K","4K","3K","2K","1K",],
        },
        legend: {
            show: false,
            position: "top",
            horizontalAlign: "center",
        },
    };
    var chartSpark1 = new ApexCharts(document.querySelector("#newline"),optionsSpark1);chartSpark1.render();

    // SEO Department Chart
    var options = {
        series: [{
            name: "SEO Department",
            data: [6, 1, 3, 3, 6, 3, 2, 2, 8, 2],
        },],
        chart: {
            height: 50,
            type: "area",
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            curve: "straight",
            colors: "#7868da",
            width: 2,
            dashArray: 0,
        },
        fill: {
            colors: "#7868da",
            opacity: 1,
            type: "solid",
        },
        tooltip: {
            fixed: {
                enabled: true,
                position: "topLeft",
                offsetX: 0,
                offsetY: -80,
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#seo_dep"), options);chart.render();

    // Account Chart
    var options = {
        series: [{
            name: "Account",
            data: [6, 4, 7, 8, 4, 3, 2, 2, 5, 6, 7, 4, 1, 5, 7, 9, 9, 8, 7, 6],
        },],
        chart: {
            height: 50,
            type: "area",
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            curve: "straight",
            colors: "#f55c78",
            width: 2,
            dashArray: 0,
        },
        fill: {
            colors: "#f55c78",
            opacity: 1,
            type: "solid",
        },
        tooltip: {
            fixed: {
                enabled: true,
                position: "topLeft",
                offsetX: 0,
                offsetY: -80,
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#account"), options);chart.render();

    //  Visit By Traffic
    var options = {
        series: [6, 4, 8],
        chart: {
            type: "donut",
            width: 200,
            height: 200,
        },
        legend: {
            show: false,
        },
        dataLabels: {
            enabled: false,
        },
        labels: ["Quarter 1", "Quarter 2", "Quarter 3"],
        colors: ['var(--chart-color2)','var(--chart-color3)','var(--chart-color1)'],
        plotOptions: {
            pie: {
                donut: {
                size: "0%",
                },
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#visit_by"), options);chart.render();

    
    // Page Views
    var options1 = {
        chart: {
            type: "bar",
            height: 40,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 2,
        },
        colors: ['var(--chart-color3)'],
        series: [{
            data: [2, 3, 5, 6, 9, 8, 7, 8, 7, 4, 6, 5],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            y: {
                title: {
                    formatter: function (seriesName) {
                        return "";
                    },
                },
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts(document.querySelector("#page_views"), options1).render();

    // Site
    var options1 = {
        chart: {
            type: "bar",
            height: 40,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 2,
        },
        colors: ['var(--chart-color2)'],
        series: [{
            data: [8, 5, 3, 2, 2, 3, 5, 6, 4, 5, 7, 5],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            y: {
                title: {
                    formatter: function (seriesName) {
                        return "";
                    },
                },
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts(document.querySelector("#site"), options1).render();

    // Clicks
    var options1 = {
        chart: {
            type: "bar",
            height: 40,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 2,
        },
        colors: ['var(--chart-color1)'],
        series: [{
            data: [6, 5, 4, 6, 5, 5, 2, 3, 1, 8, 4, 2],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            y: {
                title: {
                formatter: function (seriesName) {
                    return "";
                },
                },
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts(document.querySelector("#clicks"), options1).render();

    //  Visit By Traffic
    var options = {
        series: [86, 36, 23, 57, 42],
        chart: {
            type: "donut",
            width: 200,
            height: 200,
        },
        legend: {
            show: false,
        },
        dataLabels: {
            enabled: false,
        },
        labels: ["America", "Canada", "UK", "India", "Australia"],
        colors: ['var(--chart-color2)','var(--chart-color3)','var(--chart-color1)','var(--chart-color4)','var(--chart-color5)'],
        plotOptions: {
            pie: {
                donut: {
                    size: "80%",
                    labels: {
                        show: true,
                        total: {
                            show: true,
                            showAlways: true,
                            label: "Total",
                            fontSize: "18px",
                            fontWeight: 600,
                            color: "#000",
                        },
                    },
                },
            },
        },
    };
    var chart = new ApexCharts(document.querySelector("#donut_chart"), options);chart.render();

    // Daily Sales
    var options1 = {
        chart: {
            type: "bar",
            width: "85%",
            height: 100,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 0,
        },
        colors: ["#ffffff"],
        series: [{
            data: [7,5,6,4,8,7,5,6,2,3,5,11,2,3,4,1,4,7,2,3,6,4,5,5,6,2,3,5,6,2,3,4,2,4,],
        },],

        tooltip: {
            fixed: {
                enabled: false,
            },
            x: {
                show: false,
            },
            y: {
                title: {
                    formatter: function (seriesName) {
                        return "";
                    },
                },
            },
            marker: {
                show: false,
            },
        },
    };
    new ApexCharts(document.querySelector("#daily_sales"), options1).render();

    
    // BTC Chart
    var optionsSpark1 = {
        series: [{
            name: "Tasks2",
            data: [155, 161, 170, 205, 198, 245, 279, 301, 423],
        },{
            name: "Tasks3",
            data: [105, 140, 150, 170, 205, 190, 245, 279, 300],
        },],
        colors: ['var(--chart-color1)','var(--chart-color4)'],
        chart: {
            type: "area",
            height: 120,
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
            width: 2,
            dashArray: 0,
        },
        dataLabels: {
            enabled: false,
        },
        xaxis: {
            show: true,
            categories: ["2012","2013","2014","2015","2016","2017","2021","2019","2020","2021",],
        },
        yaxis: {
            show: true,
        },
        legend: {
            position: "top",
            horizontalAlign: "center",
        },
    };
    var chartSpark1 = new ApexCharts(document.querySelector("#btc"),optionsSpark1);chartSpark1.render();
    
});
