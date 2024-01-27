$(function() {
  ("use strict");

  // Project Design
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
    colors: ["#7460ee"],
    series: [
      {
        data: [2, 5, 8, 3, 5, 7, 1, 6],
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
  new ApexCharts(document.querySelector("#project_design"), options1).render();

  // Project Marketing
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
    colors: ["#f96332"],
    series: [
      {
        data: [6, 2, 3, 4, 8, 7, 6, 2],
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
  new ApexCharts(document.querySelector("#project_marketing"), options1).render();

  // Project Developer
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
    colors: ["#2CA8FF"],
    series: [
      {
        data: [8, 2, 3, 4, 6, 5, 2, 7],
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
  new ApexCharts(document.querySelector("#project_dev"), options1).render();
});