$(function () {
  ("use strict");

  // Likes Chart
  var options = {
    series: [
      {
        name: "Like",
        data: [31, 40, 28, 51, 42, 109, 100],
      },
    ],
    chart: {
      height: 80,
      type: "area",
      sparkline: {
        enabled: true,
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
      type: "datetime",
      categories: [
        "2021-09-19T00:00:00.000Z",
        "2021-09-19T01:30:00.000Z",
        "2021-09-19T02:30:00.000Z",
        "2021-09-19T03:30:00.000Z",
        "2021-09-19T04:30:00.000Z",
        "2021-09-19T05:30:00.000Z",
        "2021-09-19T06:30:00.000Z",
      ],
    },
    tooltip: {
      x: {
        format: "dd/MM/yy HH:mm",
      },
    },
  };

  var chart = new ApexCharts(document.querySelector("#chart"), options);
  chart.render();

  // Comments
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
      type: "bar",
      height: 80,
      sparkline: {
        enabled: true,
      },
    },
    plotOptions: {
      bar: {
        horizontal: false,
        columnWidth: "55%",
        endingShape: "rounded",
      },
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      show: true,
      width: 2,
      colors: ["transparent"],
    },
    xaxis: {
      categories: [
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Oct",
      ],
    },
    yaxis: {
      title: {
        text: "$ (thousands)",
      },
    },
    fill: {
      opacity: 1,
    },
    tooltip: {
      y: {
        formatter: function (val) {
          return "$ " + val + " thousands";
        },
      },
    },
  };

  var chart = new ApexCharts(document.querySelector("#comments"), options);
  chart.render();

  // Share
  var options = {
    series: [
      {
        name: "Share",
        data: [10, 41, 35, 51, 49, 62, 69, 91, 148],
      },
    ],
    chart: {
      height: 80,
      type: "line",
      zoom: {
        enabled: false,
      },
      sparkline: {
        enabled: true,
      },
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      curve: "straight",
      width: 1,
    },
    xaxis: {
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
      ],
    },
  };

  var chart = new ApexCharts(document.querySelector("#share"), options);
  chart.render();

  // View
  var options = {
    series: [
      {
        name: "View",
        data: [
          2,
          5,
          8,
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
        ],
      },
    ],
    chart: {
      type: "bar",
      height: 80,
      sparkline: {
        enabled: true,
      },
    },
    plotOptions: {
      bar: {
        horizontal: false,
        columnWidth: "55%",
        endingShape: "rounded",
      },
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      show: true,
      width: 2,
      colors: ["transparent"],
    },
    xaxis: {
      categories: [
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Oct",
      ],
    },
    fill: {
      opacity: 1,
    },
  };

  var chart = new ApexCharts(document.querySelector("#view"), options);
  chart.render();

  // Categories Statistics Chart
  var optionsSpark1 = {
    series: [
      {
        name: "Total Leads",
        data: [70, 25, 63, 22, 85, 77, 68],
      },
      {
        name: "Connections",
        data: [90, 49, 90, 61, 62, 90, 75],
      },
      {
        name: "Articles",
        data: [85, 36, 100, 45, 77, 50, 100],
      },
      {
        name: "Categories",
        data: [45, 90, 55, 90, 22, 90, 25],
      },
    ],
    chart: {
      type: "line",
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
      categories: ["2015", "2016", "2017", "2021", "2019", "2020", "2021"],
    },
    yaxis: {
      show: true,
    },
    legend: {
      show: false,
      position: "top",
      horizontalAlign: "center",
    },
  };

  var chartSpark1 = new ApexCharts(
    document.querySelector("#categories_statistics"),
    optionsSpark1
  );
  chartSpark1.render();

  // Word Map
  var mapData = {
    US: 298,
    SA: 200,
    AU: 760,
    IN: 2000000,
    GB: 120,
  };

  if ($("#world-map-markers").length > 0) {
    $("#world-map-markers").vectorMap({
      map: "world_mill_en",
      backgroundColor: "transparent",
      borderColor: "#fff",
      borderOpacity: 0.25,
      borderWidth: 0,
      color: "#e6e6e6",
      regionStyle: {
        initial: {
          fill: "#eaeaea",
        },
      },

      markerStyle: {
        initial: {
          r: 5,
          fill: "#fff",
          "fill-opacity": 1,
          stroke: "#000",
          "stroke-width": 1,
          "stroke-opacity": 0.4,
        },
      },

      markers: [
        {
          latLng: [21.0, 78.0],
          name: "INDIA : 350",
        },
        {
          latLng: [-33.0, 151.0],
          name: "Australia : 250",
        },
        {
          latLng: [36.77, -119.41],
          name: "USA : 250",
        },
        {
          latLng: [55.37, -3.41],
          name: "UK   : 250",
        },
        {
          latLng: [25.2, 55.27],
          name: "UAE : 250",
        },
      ],

      series: {
        regions: [
          {
            values: {
              US: "#49c5b6",
              SA: "#667add",
              AU: "#50d38a",
              IN: "#60bafd",
              GB: "#ff758e",
            },
            attribute: "fill",
          },
        ],
      },
      hoverOpacity: null,
      normalizeFunction: "linear",
      zoomOnScroll: false,
      scaleColors: ["#000000", "#000000"],
      selectedColor: "#000000",
      selectedRegions: [],
      enableZoom: false,
      hoverColor: "#fff",
    });
  }

  if ($("#india").length > 0) {
    $("#india").vectorMap({
      map: "in_mill",
      backgroundColor: "transparent",
      regionStyle: {
        initial: {
          fill: "#f4f4f4",
        },
      },
    });
  }

  if ($("#usa").length > 0) {
    $("#usa").vectorMap({
      map: "us_aea_en",
      backgroundColor: "transparent",
      regionStyle: {
        initial: {
          fill: "#f4f4f4",
        },
      },
    });
  }

  if ($("#australia").length > 0) {
    $("#australia").vectorMap({
      map: "au_mill",
      backgroundColor: "transparent",
      regionStyle: {
        initial: {
          fill: "#f4f4f4",
        },
      },
    });
  }

  if ($("#uk").length > 0) {
    $("#uk").vectorMap({
      map: "uk_mill_en",
      backgroundColor: "transparent",
      regionStyle: {
        initial: {
          fill: "#f4f4f4",
        },
      },
    });
  }

  // Visitors Statistics
  var options = {
    series: [15, 45, 40],
    labels: ["Tablet", "Desktops", "Mobile"],
    chart: {
      type: "donut",
      width: 250,
    },
    dataLabels: {
      enabled: false,
    },
    legend: {
      show: false,
    },
    tooltip: {
      enabled: false,
    },
    plotOptions: {
      pie: {
        donut: {
          labels: {
            show: true,
          },
          labels: {
            show: true,
            total: {
              show: true,
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
  var chart = new ApexCharts(document.querySelector("#donut_chart"), options);
  chart.render();

  // Twitter
  var options = {
    series: [200, 56],
    labels: ["Twitter"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#00aced", "#a5a5a5"],
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
    dataLabels: {
      enabled: false,
    },
    legend: {
      show: false,
    },
  };
  var chart = new ApexCharts(document.querySelector("#tw"), options);
  chart.render();

  // Google+
  var options = {
    series: [315, 87],
    labels: ["Twitter"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#dd4b39", "#a5a5a5"],
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
    dataLabels: {
      enabled: false,
    },
    legend: {
      show: false,
    },
  };
  var chart = new ApexCharts(document.querySelector("#gp"), options);
  chart.render();

  // Facebook
  var options = {
    series: [100, 66],
    labels: ["Twitter"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#3b5998", "#a5a5a5"],
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
    dataLabels: {
      enabled: false,
    },
    legend: {
      show: false,
    },
  };
  var chart = new ApexCharts(document.querySelector("#fb"), options);
  chart.render();

  // Instagram
  var options = {
    series: [135, 37],
    labels: ["Twitter"],
    chart: {
      type: "donut",
      width: 180,
    },
    colors: ["#517fa4", "#a5a5a5"],
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
    dataLabels: {
      enabled: false,
    },
    legend: {
      show: false,
    },
  };
  var chart = new ApexCharts(document.querySelector("#in"), options);
  chart.render();
});
