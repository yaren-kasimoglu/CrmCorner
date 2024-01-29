$(function () {
  ("use strict");

  // Total Ticket Chart
  var options = {
    series: [5, 3, 2],
    chart: {
      width: 150,
      type: "pie",
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
  };

  var chart = new ApexCharts(document.querySelector("#total_ticket"), options);
  chart.render();

  // Resolve Chart
  var options = {
    series: [240, 1038],
    chart: {
      width: 150,
      type: "donut",
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    labels: ["Resolved", "Solved"],
    colors: ["#00ca70", "#6c757d"],
  };

  var chart = new ApexCharts(document.querySelector("#resolve"), options);
  chart.render();

  // Pending Chart
  var options = {
    series: [521, 1021],
    chart: {
      width: 150,
      type: "donut",
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    labels: ["Pending", "All"],
    colors: ["#4b78b8", "#6c757d"],
  };

  var chart = new ApexCharts(document.querySelector("#pending"), options);
  chart.render();

  // Responded Chart
  var options = {
    series: [978, 822],
    chart: {
      width: 150,
      type: "donut",
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    labels: ["Responded", "All"],
    colors: ["#ffbf00", "#6c757d"],
  };

  var chart = new ApexCharts(document.querySelector("#responded"), options);
  chart.render();
});