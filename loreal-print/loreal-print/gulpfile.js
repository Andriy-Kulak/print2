/// <binding BeforeBuild='min' />
"use strict";

var gulp = require("gulp"),
  rimraf = require("rimraf"),
  concat = require("gulp-concat"),
  cssmin = require("gulp-cssmin"),
  rename = require("gulp-rename"),
  uglify = require("gulp-uglify");

var jsPaths = ["./Public/scripts/**/*.js", "!./Public/scripts/**/*.spec.js", "!./Public/scripts/testing/**"];
var jsDestpaths = ["./Public/dist/concatMinifiedDistFile.min.js", "./Public/dist/concatMinifiedDistFile.js"];

// tasks

// A task that uses the rimraf Node deletion module to remove the minified version of the site.js file.
gulp.task("clean:js", function (cb) {
    rimraf(jsDestpaths, cb);
});

// task that calls the clean:js task, followed by the clean:css task.
gulp.task("clean", ["clean:js"]);

// task that minifies and concatenates all .js files within the js folder. The .min.js files are excluded.
gulp.task("min:js", function () {
    gulp.src(jsPaths)
    .pipe(concat("concatMinifiedDistFile.js"))   // Combine into 1 file
    .pipe(gulp.dest("./Public/dist"))            // Write non-minified to disk
    .pipe(uglify())                     // Minify
    .pipe(rename({extname: ".min.js"})) // Rename to ng-quick-date.min.js
    .pipe(gulp.dest("./Public/dist"))            // Write minified to disk

});

//  task that calls the min:js task, followed by the min:css task.
gulp.task("min", ["min:js"]);
