$(function () {
  ("use strict");

  // Twitter Donut
  var options = {
    series: [200, 56],
    labels: ["Twitter"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#4CAF50", "#a5a5a5"],
    plotOptions: {
      pie: {
        donut: {
          size: "85%",
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
      show: false,
    },
  };
  var chart = new ApexCharts(document.querySelector("#twitter"), options);
  chart.render();

  // Facebook Donut
  var options = {
    series: [451, 48],
    labels: ["Facebook"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#7b69ec", "#a5a5a5"],
    plotOptions: {
      pie: {
        donut: {
          size: "85%",
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
      show: false,
    },
  };
  var chart = new ApexCharts(document.querySelector("#fb"), options);
  chart.render();
});
