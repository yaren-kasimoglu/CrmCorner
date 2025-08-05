$(function () {
  ("use strict");

  //  Widgets Earnings Chart
  var options = {
    series: [
      {
        name: "Earnings",
        data: [1, 4, 1, 3, 7, 1],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#f79647",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#fac091",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-earnings"), options);
  chart.render();

  //  Widgets Sales Chart
  var options = {
    series: [
      {
        name: "Sales",
        data: [1, 4, 2, 3, 6, 2],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#a092b0",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#a092b0",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-sales"), options);
  chart.render();

  //  Widgets Visit Chart
  var options = {
    series: [
      {
        name: "Visit",
        data: [1, 4, 2, 3, 1, 5],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#92cddc",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#92cddc",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-visit"), options);
  chart.render();

  //  Widgets Likes Chart
  var options = {
    series: [
      {
        name: "Likes",
        data: [1, 3, 5, 1, 4, 2],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#95b3d7",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#95b3d7",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-likes"), options);
  chart.render();

  //  Widgets Like Chart
  var options = {
    series: [
      {
        name: "Likes",
        data: [
          2,
          3,
          1,
          5,
          4,
          7,
          8,
          2,
          3,
          1,
          4,
          6,
          5,
          4,
          4,
          2,
          3,
          1,
          5,
          4,
          7,
          8,
          2,
          3,
          1,
          4,
          6,
          5,
          4,
          4,
        ],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#62a6ef",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#62a6ef",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-like"), options);
  chart.render();

  //  Widgets Comments Chart
  var options = {
    series: [
      {
        name: "Comments",
        data: [
          2,
          3,
          1,
          5,
          4,
          7,
          8,
          2,
          3,
          1,
          4,
          6,
          5,
          4,
          4,
          2,
          3,
          1,
          5,
          4,
          7,
          8,
          2,
          3,
          1,
          4,
          6,
          5,
          4,
          4,
        ],
      },
    ],
    chart: {
      height: 50,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#e66d7e",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#e66d7e",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-comments"), options);
  chart.render();

  //  Widgets Share Chart
  var options = {
    series: [
      {
        name: "Share",
        data: [
          12,
          4,
          6,
          15,
          5,
          5,
          5,
          6,
          8,
          9,
          7,
          2,
          11,
          5,
          4,
          8,
          17,
          10,
          18,
          0,
          2,
          0,
          1,
          8,
          3,
          8,
          9,
          6,
        ],
      },
    ],
    chart: {
      height: 50,
      type: "line",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#23c596",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#23c596",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Share"), options);
  chart.render();

  //  Widgets View Chart
  var options = {
    series: [
      {
        name: "View",
        data: [
          10,
          18,
          0,
          2,
          0,
          1,
          8,
          3,
          8,
          9,
          6,
          3,
          2,
          5,
          1,
          4,
          2,
          3,
          1,
          5,
          4,
          7,
          8,
          2,
          3,
          12,
          4,
          6,
          15,
          5,
          5,
          5,
          6,
          8,
          9,
          7,
          2,
          11,
          5,
          4,
          8,
          17,
          10,
          18,
          0,
          2,
          0,
          1,
          8,
          3,
          8,
          9,
          6,
          3,
          2,
        ],
      },
    ],
    chart: {
      height: 50,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#f7cf5c",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#f7cf5c",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-View"), options);
  chart.render();

  //  Widgets Population Chart
  var options = {
    series: [
      {
        name: "Population",
        data: [6, 4, 8, 6, 8, 10, 5, 6, 7, 9, 5],
      },
    ],
    chart: {
      //   height: 50,
      width: 100,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#f7cf5c",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#f7cf5c",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Population"), options);
  chart.render();

  //  Widgets Usage Chart
  var options = {
    series: [44, 55, 41, 17],
    chart: {
      type: "donut",
      width: 100,
      height: 90,
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    labels: ["Quarter 1", "Quarter 2", "Quarter 3", "Quarter 4"],
    colors: ["#01b8aa", "#f2c80f", "#fd625e", "#374649"],
    plotOptions: {
      pie: {
        donut: {
          size: "5%",
        },
      },
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Usage"), options);
  chart.render();

  // Widgets Page Views
  var options = {
    series: [
      {
        name: "Page Views",
        data: [4, 6, -3, -1, 2, -2, 4, 3, 6, 7, -2, 3],
      },
    ],
    chart: {
      type: "bar",
      height: 70,
      width: 100,
      stacked: true,
      sparkline: {
        enabled: true,
      },
    },
    plotOptions: {
      bar: {
        colors: {
          ranges: [
            {
              from: -100,
              to: 0,
              color: "#F15B46",
            },
          ],
        },
        columnWidth: "100%",
        horizontal: false,
        barHeight: "80%",
      },
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      width: 1,
      colors: ["#fff"],
    },
    tooltip: {
      shared: false,
      x: {
        formatter: function (val) {
          return val;
        },
      },
      y: {
        formatter: function (val) {
          return Math.abs(val) + "%";
        },
      },
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-pageView"), options);
  chart.render();

  //  Widgets Growth Chart
  var options = {
    series: [
      {
        name: "Growth",
        data: [9, 4, 6, 5, 6, 4, 7, 3],
      },
    ],
    chart: {
      height: 70,
      type: "line",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#e66d7e",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#e66d7e",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Growth"), options);
  chart.render();

  //  Widgets Average1 Chart
  var options = {
    series: [
      {
        name: "Average1",
        data: [1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 5, 5, 4, 4, 3, 3, 2, 2, 1],
      },
    ],
    chart: {
      height: 50,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#00ced1",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#00ced1",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Average1"), options);
  chart.render();

  //  Widgets Average2 Chart
  var options = {
    series: [
      {
        name: "Average2",
        data: [1, 2, 3, 8, 7, 8, 3, 2, 1, 2, 3, 4, 5, 6, 4, 4, 7, 6, 5, 4],
      },
    ],
    chart: {
      height: 50,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#e4d354",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#e4d354",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Average2"), options);
  chart.render();

  //  Widgets Average3 Chart
  var options = {
    series: [
      {
        name: "Average3",
        data: [1, 2, 3, 4, 5, 4, 3, 2, 1, 2, 3, 4, 5, 6, 7, 8, 7, 6, 5, 4],
      },
    ],
    chart: {
      height: 50,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#7cb5ec",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#7cb5ec",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Average3"), options);
  chart.render();

  //  Widgets Average4 Chart
  var options = {
    series: [
      {
        name: "Average4",
        data: [8, 7, 6, 5, 4, 3, 2, 2, 3, 4, 5, 6, 7, 8, 7, 6, 5, 4],
      },
    ],
    chart: {
      height: 50,
      type: "bar",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#f15c80",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#f15c80",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Average4"), options);
  chart.render();

  // Satisfaction Rate
  var options = {
    chart: {
      type: "donut",
      width: 180,
    },
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
    series: [200, 56],
    labels: ["Satisfaction Rate", "Rate"],
    colors: ["#26dad2", "#a5a5a5"],
  };
  var chart = new ApexCharts(
    document.querySelector("#Satisfaction_Rate"),
    options
  );
  chart.render();

  // Project Panding
  var options = {
    series: [87, 315],
    labels: ["Project Panding", "Project Complated"],
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
  var chart = new ApexCharts(
    document.querySelector("#Project_Panding"),
    options
  );
  chart.render();

  // Productivity Goal
  var options = {
    series: [100, 66],
    labels: ["Productivity Goal", "Productivity"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#f9bd53", "#a5a5a5"],
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
  var chart = new ApexCharts(
    document.querySelector("#Productivity_Goal"),
    options
  );
  chart.render();

  // Total Revenue
  var options = {
    series: [135, 37],
    labels: ["Total Revenue", "Revenue"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#00adef", "#a5a5a5"],
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
  var chart = new ApexCharts(document.querySelector("#Total_Revenue"), options);
  chart.render();

  //  Widgets Orders Received Chart
  var options = {
    series: [
      {
        name: "Orders Received",
        data: [1, 4, 2, 6, 5, 2, 3, 8, 5, 2],
      },
    ],
    chart: {
      height: 50,
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
  var chart = new ApexCharts(
    document.querySelector("#w-Orders_Received"),
    options
  );
  chart.render();

  //  Widgets Total Sales Chart
  var options = {
    series: [
      {
        name: "Total Sales",
        data: [2, 9, 5, 5, 8, 5, 4, 2, 6],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#75c3f2",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#75c3f2",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(document.querySelector("#w-Total_Sales"), options);
  chart.render();

  //  Widgets Total Profit Chart
  var options = {
    series: [
      {
        name: "Total Profit",
        data: [1, 5, 3, 6, 6, 3, 6, 8, 4, 2],
      },
    ],
    chart: {
      height: 50,
      type: "area",
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      curve: "straight",
      colors: "#fc7b92",
      width: 2,
      dashArray: 0,
    },
    fill: {
      colors: "#fc7b92",
      opacity: 1,
      type: "solid",
    },
  };
  var chart = new ApexCharts(
    document.querySelector("#w-Total_Profit"),
    options
  );
  chart.render();
});
