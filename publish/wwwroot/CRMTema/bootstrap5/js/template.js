$(function () {
    ("use strict");
    skinChanger();

    // Page Loader
    setTimeout(function () {
        $(".page-loader-wrapper").fadeOut();
    }, 50);

    // Main Menu in MetisMenu
    $(".sidebar").metisMenu();

    // Navbar Search Form Width
    $('.navbar-form.search-form input[type="text"]')
        .on("focus", function () {
        $(this).animate(
            {
                width: "+=50px",
            },
            300
        );
        })
        .on("focusout", function () {
        $(this).animate(
            {
                width: "-=50px",
            },
            300
        );
    });

    // toggle fullwidth layout
    $(".btn-toggle-fullwidth").on("click", function () {
        if (!$("body").hasClass("layout-fullwidth")) {
            $("body").addClass("layout-fullwidth");
            $(this).find(".fa").toggleClass("fa-arrow-left fa-arrow-right");
        } else {
            $("body").removeClass("layout-fullwidth");
            $(this).find(".fa").toggleClass("fa-arrow-left fa-arrow-right");
        }
    });

    // off-canvas menu toggle
    $(".btn-toggle-offcanvas").on("click", function () {
        $("body").toggleClass("offcanvas-active");
    });

    $("#main-content").on("click", function () {
        $("body").removeClass("offcanvas-active");
    });

    //Skin changer
    function skinChanger() {
        $(".choose-skin li").on("click", function () {
        var $body = $("#layout");
        var $this = $(this);

        var existTheme = $(".choose-skin li.active").data("theme");
        $(".choose-skin li").removeClass("active");
        $body.removeClass("theme-" + existTheme);
        $this.addClass("active");
        $body.addClass("theme-" + $this.data("theme"));
        });
    }

    // LTR/RTL js
    $(".theme-rtl input:checkbox").on("click", function () {
        if ($(this).is(":checked")) {
            $("body").addClass("rtl_mode");
        } else {
            $("body").removeClass("rtl_mode");
        }
    }); 

    // Mini sidebar 
    $(".minisidebar-active input:checkbox").on("click", function () {
        if ($(this).is(":checked")) {
            $("body").addClass("sidebar-mini");
        } else {
            $("body").removeClass("sidebar-mini");
        }
    }); 

    // Mini sidebar hover js
	$(".sidebar-mini #left-sidebar").hover(
        function () {
            $(this).removeClass("mini");
        },
        function () {
            $(this).addClass("mini");
            $(".sidebar-mini .sidebar .tab-pane").removeClass("active show");
            $(".sidebar-mini .sidebar .nav-link").removeClass("active");
            $(".sidebar-mini .sidebar #hr_menu_nav_link").addClass("active");
            $(".sidebar-mini .sidebar #hr_menu").addClass("active show");
        }
    );

    // Tooltip
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // light and dark theme setting js
    var toggleSwitch = document.querySelector(
        '.theme-switch input[type="checkbox"]'
    );
    var toggleHcSwitch = document.querySelector(
        '.theme-high-contrast input[type="checkbox"]'
    );
    var currentTheme = localStorage.getItem("theme");
    if (currentTheme) {
        document.documentElement.setAttribute("data-theme", currentTheme);

        if (currentTheme === "dark") {
            toggleSwitch.checked = true;
        }
        if (currentTheme === "high-contrast") {
            toggleHcSwitch.checked = true;
            toggleSwitch.checked = false;
        }
    }
    function switchTheme(e) {
        if (e.target.checked) {
            document.documentElement.setAttribute("data-theme", "dark");
            localStorage.setItem("theme", "dark");
            $('.theme-high-contrast input[type="checkbox"]').prop("checked", false);
        } else {
            document.documentElement.setAttribute("data-theme", "light");
            localStorage.setItem("theme", "light");
        }
    }
    function switchHc(e) {
        if (e.target.checked) {
            document.documentElement.setAttribute("data-theme", "high-contrast");
            localStorage.setItem("theme", "high-contrast");
            $('.theme-switch input[type="checkbox"]').prop("checked", false);
        } else {
            document.documentElement.setAttribute("data-theme", "light");
            localStorage.setItem("theme", "light");
        }
    }
    toggleSwitch.addEventListener("change", switchTheme, false);
    toggleHcSwitch.addEventListener("change", switchHc, false);
    // end

    // Block Header Right Mini Bar Chart
    // Chart Visitors
    var options1 = {
        chart: {
            type: "bar",
            width: 50,
            height: 36,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 1,
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
    new ApexCharts(document.querySelector("#bh_visitors"), options1).render();

    // Visits
    var options1 = {
        chart: {
            type: "bar",
            width: 50,
            height: 36,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 1,
        },
        colors: ['var(--chart-color2)'],
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
    new ApexCharts(document.querySelector("#bh_visits"), options1).render();

    // Chats
    var options1 = {
        chart: {
            type: "bar",
            width: 50,
            height: 36,
            sparkline: {
                enabled: true,
            },
        },
        stroke: {
            width: 1,
        },
        colors: ['var(--chart-color3)'],
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
    new ApexCharts(document.querySelector("#bh_chats"), options1).render();
});