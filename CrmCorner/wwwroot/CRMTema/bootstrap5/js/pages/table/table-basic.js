$(function () {
  ("use strict");

  // Alpino
  var optionsSpark1 = {
    series: [
      {
        data: [3, 5, 1, 6, 5, 4, 8, 3],
      },
    ],
    chart: {
      type: "line",
      width: 100,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      colors: "#00c5dc",
      width: 1,
      dashArray: 0,
    },
    tooltip: {
      enabled: false,
    },
  };
  var chartSpark1 = new ApexCharts(document.querySelector("#Alpino"), optionsSpark1);
  chartSpark1.render();
  
  // Compass
  var optionsSpark1 = {
    series: [
      {
        data: [4, 6, 3, 2, 5, 6, 5, 4],
      },
    ],
    chart: {
      type: "line",
      width: 100,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      colors: "#f4516c",
      width: 1,
      dashArray: 0,
    },
    tooltip: {
      enabled: false,
    },
  };
  var chartSpark1 = new ApexCharts(document.querySelector("#Compass"), optionsSpark1);
  chartSpark1.render();
  
  // Nexa
  var optionsSpark1 = {
    series: [
      {
        data: [7, 3, 2, 1, 5, 4, 6, 8],
      },
    ],
    chart: {
      type: "line",
      width: 100,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      colors: "#31db3d",
      width: 1,
      dashArray: 0,
    },
    tooltip: {
      enabled: false,
    },
  };
  var chartSpark1 = new ApexCharts(document.querySelector("#Nexa"), optionsSpark1);
  chartSpark1.render();
  
  // Oreo
  var optionsSpark1 = {
    series: [
      {
        data: [3, 1, 2, 5, 4, 6, 2, 3],
      },
    ],
    chart: {
      type: "line",
      width: 100,
      height: 40,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      colors: "#2d342e",
      width: 1,
      dashArray: 0,
    },
    tooltip: {
      enabled: false,
    },
  };
  var chartSpark1 = new ApexCharts(document.querySelector("#Oreo"), optionsSpark1);
  chartSpark1.render();

  // Twitter
    var options1 = {
      chart: {
        type: "bar",
        width: 60,
        height: 20,
        sparkline: {
          enabled: true,
        },
      },
      stroke: {
        width: 0,
      },
      colors: ["#3366cc"],
      series: [
        {
          data: [5, 8, 6, 3, 5, 9, 2],
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
    new ApexCharts(document.querySelector("#Twitter"), options1).render();

  // FB
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 20,
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
              to: -1,
              color: "#f00",
            },
          ],
        },
        columnWidth: "80%",
      },
    },
    stroke: {
      width: 0,
    },
    colors: ["#3366cc"],
    series: [
      {
        data: [8, 2, 1, 5, -2, 6, -4],
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
  new ApexCharts(
    document.querySelector("#fb"),
    options1
  ).render();

  // MC
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 20,
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
              to: -1,
              color: "#f00",
            },
          ],
        },
        columnWidth: "80%",
      },
    },
    stroke: {
      width: 0,
    },
    colors: ["#3366cc"],
    series: [
      {
        data: [2, 3, 3, -2, -8, 4, 8],
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
  new ApexCharts(document.querySelector("#mc"), options1).render();

  // Google
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 20,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 0,
    },
    colors: ["#3366cc"],
    series: [
      {
        data: [5, 5, 5, 6, 3, 2, 1],
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
  new ApexCharts(
    document.querySelector("#google"),
    options1
  ).render();

  // Other
  var options1 = {
    chart: {
      type: "bar",
      width: 60,
      height: 20,
      sparkline: {
        enabled: true,
      },
    },
    stroke: {
      width: 0,
    },
    colors: ["#3366cc"],
    series: [
      {
        data: [5, 8, 6, 3, 5, 9, 2],
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
  new ApexCharts(
    document.querySelector("#other"),
    options1
  ).render();
});