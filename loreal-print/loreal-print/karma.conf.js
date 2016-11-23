// Karma configuration
// Generated on Tue Oct 04 2016 17:05:35 GMT-0400 (Eastern Daylight Time)

module.exports = function(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: '',


    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine'],


    // list of files / patterns to load in the browser
    files: [
        
        'node_modules/angular/angular.js',
        'node_modules/angular-mocks/angular-mocks.js',
        'node_modules/angular-ui-router/release/angular-ui-router.js',
        'node_modules/angularjs-toaster/toaster.js',
        'Public/scripts/print.module.js',
        'Public/scripts/testing/stateMock.js',
        //'Public/scripts/books/books.controller.js',
        //'Public/scripts/books/books.service.js',
        //'Public/scripts/circulation/circulation.controller.js',
        //'Public/scripts/editorialCalendar/editorialCalendar.controller.js',
        //'Public/scripts/email/email.controller.js',
        //'Public/scripts/editorialCalendar/editorialCalendar.controller.js',
        //'Public/scripts/rates/rates.controller.js',
        //'Public/scripts/ratesReport/ratesRaport.controller.js',
        //'Public/scripts/signSubmit/signSubmit.controller.js',
        //'Public/scripts/terms/terms.controller.js',
        //'Public/scripts/tablet/tablet.controller.js',
        //'Public/scripts/sharedViews/printNavbar.html',

        'Public/scripts/**/*.js',
        'Public/scripts/**/*.html',
        'Public/scripts/**/**/*.html',
        'Public/scripts/**/*spec.js'

    ],


    // list of files to exclude
    exclude: [
    ],


    // preprocess matching files before serving them to the browser
    // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
    //preprocessors: {
    //    '**/*.html': ['ng-html2js']
    //},
    ngHtml2JsPreprocessor: {
        // strip this from the file path
        stripPrefix: 'public/',
        stripSuffix: '.ext',
        // prepend this to the
        prependPrefix: 'served/',

        // or define a custom transform function
        // - cacheId returned is used to load template
        //   module(cacheId) will return template at filepath
        cacheIdFromPath: function(filepath) {
            // example strips 'public/' from anywhere in the path
            // module(app/templates/template.html) => app/public/templates/template.html
            var cacheId = filepath.strip('public/', '');
            return cacheId;
        },
        moduleName: 'print.module'
    },


    // test results reporter to use
    // possible values: 'dots', 'progress'
    // available reporters: https://npmjs.org/browse/keyword/karma-reporter
    reporters: ['progress'],


    // web server port
    port: 9876,


    // enable / disable colors in the output (reporters and logs)
    colors: true,


    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_INFO,


    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: true,


    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    browsers: ['PhantomJS'],


    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: false,

    // Concurrency level
    // how many browser should be started simultaneous
    concurrency: Infinity
  })
}
