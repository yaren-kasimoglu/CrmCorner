$(function () {
  ("use strict");

  // Income Analysis Chart
  var options = {
    series: [6, 4, 8],
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    chart: {
      width: 190,
      height: 190,
      type: "pie",
    },
    labels: ["Dev", "SEO", "Design"],
    colors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)'],
    responsive: [
      {
        breakpoint: 480,
        options: {
          chart: {
            width: 200,
          },
          legend: {
            position: "bottom",
          },
        },
      },
    ],
  };
  var chart = new ApexCharts(document.querySelector("#sparkline-pie"), options); chart.render();

  // Salary_Statistics
  var options = {
    series: [
      {
        name: "Developer",
        data: [23, 32, 45, 23, 56, 20],
      },
      {
        name: "Marketing",
        data: [25, 83, 92, 26, 23, 25],
      },
      {
        name: "Sales",
        data: [12, 52, 25, 35, 19, 52],
      },
    ],
    chart: {
      type: "bar",
      height: 245,
      stacked: true,
      toolbar: {
        show: false,
      },
    },
    plotOptions: {
      bar: {
        horizontal: false,
      },
    },
    stroke: {
      width: 1,
      colors: ["#fff"],
    },
    colors: ['var(--chart-color3)','var(--chart-color1)','var(--chart-color2)'],
    dataLabels: {
      enabled: false,
    },
    xaxis: {
      categories: ["Q1", "Q2", "Q3", "Q4", "Q5", "Q6"],
      labels: {
        formatter: function (val) {
          return val + "";
        },
      },
    },
    yaxis: {
      title: {
        text: undefined,
      },
    },
    tooltip: {
      y: {
        formatter: function (val) {
          return val + "K";
        },
      },
    },
    fill: {
      opacity: 1,
    },
    legend: {
      position: "top",
      horizontalAlign: "center",
      offsetX: 0,
    },
  };
  var chart = new ApexCharts(document.querySelector("#Salary_Statistics"), options); chart.render();

  // Total Salary Chart
  var optionsSpark1 = {
    series: [
      {
        name: "Sales",
        data: [82, 38, 35, 62, 41, 45, 57, 40, 55, 22, 75, 90],
      },
      {
        name: "Marketing",
        data: [21, 35, 25, 38, 40, 24, 48, 28, 55, 45, 70, 49],
      },
      {
        name: "Design",
        data: [59, 35, 30, 36, 28, 45, 58, 59, 19, 58, 70, 28],
      },
      {
        name: "Support",
        data: [49, 30, 36, 20, 40, 27, 37, 53, 12, 20, 25, 80],
      },
      {
        name: "Develpment",
        data: [38, 30, 26, 38, 40, 35, 19, 53, 17, 39, 70, 89],
      },
    ],
    colors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)','var(--chart-color4)','var(--chart-color5)'],
    chart: {
      type: "line",
      height: 245,
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
      categories: [
        "Jan",
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Oct",
        "Nov",
        "Dec",
      ],
    },
    yaxis: {
      show: true,
      categories: [
        "12K",
        "11K",
        "10K",
        "9K",
        "8K",
        "7K",
        "6K",
        "5K",
        "4K",
        "3K",
        "2K",
        "1K",
      ],
    },
    legend: {
      position: "top",
      horizontalAlign: "center",
    },
  };
  var chartSpark1 = new ApexCharts(document.querySelector("#total_Salary"),optionsSpark1); chartSpark1.render();

  // Male Female
  var options = {
    chart: {
      height: 245,
      type: "donut",
    },
    plotOptions: {
      pie: {
        donut: {
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
    dataLabels: {
      enabled: false,
    },
    legend: {
      position: "top",
      horizontalAlign: "center",
      show: false,
    },
    colors: ['var(--chart-color1)','var(--chart-color2)'],
    series: [73, 27],
    labels: ["Male", "Female"],
  };
  var chart = new ApexCharts( document.querySelector("#apex-TotalStudent"),options); chart.render();

  // Chart Employee Performance
  // Ui Developer
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 2,
    },
    colors: ['var(--chart-color1)'],
    series: [
      {
        data: [25, 66, 41, 89, 63, 25, 44, 12],
      },
    ],

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
  new ApexCharts(document.querySelector("#sparkbar_uideveloper"), options1).render();

  // Designer
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 2,
    },
    colors: ['var(--chart-color1)'],
    series: [
      {
        data: [20, 18, 38, 95, 55, 23, 29, 36],
      },
    ],

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
  new ApexCharts(document.querySelector("#sparkbar_designer1"), options1).render();

  // Leader
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 2,
    },
    colors: ['var(--chart-color1)'],
    series: [
      {
        data: [35, 76, 51, 99, 73, 35, 54, 22],
      },
    ],

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
  new ApexCharts(document.querySelector("#sparkbar_leader"), options1).render();
  
  // Developer
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 2,
    },
    colors: ['var(--chart-color1)'],
    series: [
      {
        data: [65, 43, 87, 22, 45, 55, 44, 11],
      },
    ],

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
  new ApexCharts(document.querySelector("#sparkbar_developer"), options1).render();
  
  // Designer
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 2,
    },
    colors: ['var(--chart-color1)'],
    series: [
      {
        data: [23, 45, 67, 99, 32, 21, 56, 44],
      },
    ],

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
  new ApexCharts(document.querySelector("#sparkbar_designer"), options1).render();
});