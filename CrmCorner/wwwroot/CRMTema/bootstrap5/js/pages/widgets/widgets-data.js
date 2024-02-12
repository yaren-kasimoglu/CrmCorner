$(function () {
  ("use strict");

  //  Widgets Budget Chart
  var options = {
    series: [
      {
        name: "budget",
        data: [2, 5, 8, 3, 5, 7, 1, 6],
      },
    ],
    chart: {
      height: 40,
      width: 100,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#7460ee",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#7460ee",
      opacity: 1,
      type: "solid",
    },
  };

  var chart = new ApexCharts(document.querySelector("#w-budget"), options);
  chart.render();
});
