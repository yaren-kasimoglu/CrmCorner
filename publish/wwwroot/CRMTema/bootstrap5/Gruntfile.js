module.exports = function(grunt) {
const sass = require('node-sass');
grunt.initConfig({
    sass: {
        options: {
            implementation: sass,
            includePaths: ["node_modules/bootstrap/scss/bootstrap.scss"],
        },
        dist: {
            options: {
                outputStyle: "compressed",
                removeComments: true,
                collapseWhitespace: true
            },
            files: [{
                "dist/assets/css/main.css": ["scss/main.scss"],
                
                // plugin scss file
                "dist/assets/css/chartist.min.css":     ["scss/element/chartist.scss"],
                "dist/assets/css/toastr.min.css":       ["scss/element/toastr.scss"],

                // plugin css file node modules path
                "dist/assets/css/fullcalendar.min.css": ["node_modules/fullcalendar/main.min.css"],
                "dist/assets/css/summernote.min.css":   ["node_modules/summernote/dist/summernote.css"],
                "dist/assets/css/dropify.min.css":      ["node_modules/dropify/dist/css/dropify.min.css"],
                "dist/assets/css/datepicker.min.css":   ["node_modules/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css"],
                "dist/assets/css/multiselect.min.css":  ["node_modules/bootstrap-multiselect/dist/css/bootstrap-multiselect.css"],
                "dist/assets/css/sweetalert2.min.css":  ["node_modules/sweetalert2/dist/sweetalert2.css"],
                "dist/assets/css/rangeSlider.min.css":  ["node_modules/ion-rangeslider/css/ion.rangeSlider.css"],
                "dist/assets/css/roundslider.min.css":  ["node_modules/round-slider/dist/roundslider.min.css"],
                "dist/assets/css/cropper.min.css":      ["node_modules/cropper/dist/cropper.css"],
                "dist/assets/css/nouislider.min.css":   ["node_modules/nouislider/dist/nouislider.min.css"],
                "dist/assets/css/tagsinput.min.css":    ["node_modules/bootstrap-tagsinput/dist/bootstrap-tagsinput.css"],
                "dist/assets/css/select2.min.css":      ["node_modules/select2/dist/css/select2.css"],
                "dist/assets/css/morris.min.css":       ["node_modules/morris.js/morris.css"],
            },],
        },
    },
    uglify: {
        my_target: {
            files: {

                // Template core js file path
                "dist/assets/bundles/libscripts.bundle.js":     ["node_modules/jquery/dist/jquery.js", "node_modules/bootstrap/dist/js/bootstrap.bundle.js"],
                // Template page js file path
                "dist/assets/bundles/mainscripts.bundle.js":    ["js/template.js","node_modules/metismenu/dist/metisMenu.min.js", "node_modules/apexcharts/dist/apexcharts.min.js"],

                // Chart js file path
                "dist/assets/bundles/flotscripts.bundle.js":    ["node_modules/flot-charts/jquery.flot.js","node_modules/flot-charts/jquery.flot.resize.js","node_modules/flot-charts/jquery.flot.pie.js","node_modules/flot-charts/jquery.flot.categories.js","node_modules/flot-charts/jquery.flot.time.js","node_modules/flot-charts/jquery.flot.selection.js"],
                //"dist/assets/bundles/chartist.bundle.js":       ["node_modules/chartist/dist/index.js"],
                "dist/assets/bundles/morrischart.bundle.js":    ["node_modules/raphael/raphael.js","node_modules/morris.js/morris.js"],
                "dist/assets/bundles/knobchart.bundle.js":      ["node_modules/jquery-knob/dist/jquery.knob.min.js"],
                "dist/assets/bundles/sparkline.bundle.js":      ["node_modules/jquery-sparkline/jquery.sparkline.js"],
                
                // Template more plugin js file path
                "dist/assets/bundles/fullcalendar.bundle.js":   ["node_modules/fullcalendar/index.global.min.js"],
                "dist/assets/bundles/summernote.bundle.js":     ["node_modules/summernote/dist/summernote.js"],
                "dist/assets/bundles/dropify.bundle.js":        ["node_modules/dropify/dist/js/dropify.js"],
                "dist/assets/bundles/datepicker.bundle.js":     ["node_modules/bootstrap-datepicker/dist/js/bootstrap-datepicker.js"],
                "dist/assets/bundles/sweetalert2.bundle.js":    ["node_modules/sweetalert2/dist/sweetalert2.all.min.js"],
                "dist/assets/bundles/nestable.bundle.js":       ["node_modules/jquery-nestable/jquery.nestable.js"],
                "dist/assets/bundles/rangeSlider.bundle.js":    ["node_modules/ion-rangeslider/js/ion.rangeSlider.js"],
                "dist/assets/bundles/roundslider.bundle.js":    ["node_modules/round-slider/dist/roundslider.min.js"],
                "dist/assets/bundles/cropper.bundle.js":        ["node_modules/cropper/dist/cropper.min.js"],
                "dist/assets/bundles/nouislider.bundle.js":     ["node_modules/nouislider/dist/nouislider.min.js"],
                "dist/assets/bundles/tagsinput.bundle.js":      ["node_modules/bootstrap-tagsinput/dist/bootstrap-tagsinput.js"],
                "dist/assets/bundles/select2.bundle.js":        ["node_modules/select2/dist/js/select2.js"],
                "dist/assets/bundles/dataTables.bundle.js":     ["node_modules/datatables.net/js/jquery.dataTables.js","node_modules/datatables.net-bs5/js/dataTables.bootstrap5.js","node_modules/datatables.net-responsive/js/dataTables.responsive.js"],                
                "dist/assets/bundles/inputmask.bundle.js":      ["node_modules/inputmask/dist/jquery.inputmask.min.js"],
                "dist/assets/bundles/maskedinput.bundle.js":    ["node_modules/jquery.maskedinput/src/jquery.maskedinput.js"],
                "dist/assets/bundles/toastr.bundle.js":         ["node_modules/toastr/build/toastr.min.js"],
                "dist/assets/bundles/jqueryvalidate.bundle.js": ["node_modules/jquery-validation/dist/jquery.validate.js"],
                "dist/assets/bundles/jquerysteps.bundle.js":    ["node_modules/jquery-steps/build/jquery.steps.js"],
            },
        },
    },
});
grunt.loadNpmTasks("grunt-sass");
grunt.loadNpmTasks('grunt-contrib-uglify');

grunt.registerTask("buildcss", ["sass"]);	
grunt.registerTask("buildjs", ["uglify"]);
};