$(function () {
  ("use strict");

  // Events
  var options = {
    series: [200, 56],
    labels: ["Events"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#01b2c6", "#a5a5a5"],
    plotOptions: {
      pie: {
        donut: {
          size: "80%",
          labels: {
            show: true,
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
  var chart = new ApexCharts(document.querySelector("#Events"), options);
  chart.render();

  // Birthday
  var options = {
    series: [315, 87],
    labels: ["Birthday", "All"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#2196f3", "#a5a5a5"],
    plotOptions: {
      pie: {
        donut: {
          size: "80%",
          labels: {
            show: true,
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
  var chart = new ApexCharts(document.querySelector("#Birthday"), options);
  chart.render();

  // Conferences
  var options = {
    series: [100, 66],
    labels: ["Conferences"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#f44336", "#a5a5a5"],
    plotOptions: {
      pie: {
        donut: {
          size: "80%",
          labels: {
            show: true,
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
  var chart = new ApexCharts(document.querySelector("#Conferences"), options);
  chart.render();

  // Seminars
  var options = {
    series: [135, 37],
    labels: ["Seminars"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#4caf50", "#a5a5a5"],
    plotOptions: {
      pie: {
        donut: {
          size: "80%",
          labels: {
            show: true,
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
  var chart = new ApexCharts(document.querySelector("#Seminars"), options);
  chart.render();
});