(function () {
    "use strict";
    // Test12345
    var module = angular.module('print.module', [
        'ui.router',
        'toaster'
    ]);
    module.config(['$stateProvider', '$urlRouterProvider', '$locationProvider', function ($stateProvider, $urlRouterProvider, $locationProvider) {
        $urlRouterProvider.otherwise('/print');
        $stateProvider
            .state('print', {
                url: '/print',
                templateUrl: "Public/scripts/sharedViews/printNavbar.html"
            })
            .state('books', {
                url: "/print/books",
                templateUrl: "Public/scripts/books/books.view.html",
                controller: 'booksCtrl',
                controllerAs: 'vm'
            })
            .state('email', {
                url: "/print/email",
                templateUrl: "Public/scripts/email/email.view.html",
                controller: 'emailCtrl',
                controllerAs: 'vm'
            })
            .state('pdf', {
                url: "/print/pdf",
                templateUrl: "Public/scripts/signSubmit/signSubmit.view.html",
                controller: 'signsubmitCtrl',
                controllerAs: 'vm'
            })
            .state('print.circulation', {
                url: "/circulation",
                controller: 'circulationCtrl',
                templateUrl: "Public/scripts/circulation/circulation.view.html",
                controllerAs: 'vm'
            })
            .state('print.ratesReport', {
                url: "/ratesreport",
                controller: 'ratesReportCtrl',
                templateUrl: "Public/scripts/ratesReport/ratesReport.view.html",
                controllerAs: 'vm'
            })
            .state('print.editorialCalendar', {
                url: "/editorialcalendar",
                controller: 'editorialCalendarCtrl',
                templateUrl: "Public/scripts/editorialCalendar/editorialCalendar.view.html",
                controllerAs: 'vm'
            })      
            .state('print.rates', {
                url: "/rates",
                controller: 'ratesCtrl',
                controllerAs: 'vm',
                templateUrl: "Public/scripts/rates/views/rates.main.html"
                
            })
            .state('print.terms', {
                url: "/termsconditions",
                templateUrl: "Public/scripts/terms/terms.view.html",
                controller: 'termsCtrl',
                controllerAs: 'vm'
            })
            .state('print.tablet', {
                url: "/tablet",
                templateUrl: "Public/scripts/tablet/tablet.view.html",
                controller: 'tabletCtrl',
                controllerAs: 'vm'
            })
            .state('print.signSubmit', {
                url: "/signsubmit",
                templateUrl: "Public/scripts/signSubmit/signSubmit.view.html",
                controller: 'signsubmitCtrl',
                controllerAs: 'vm'
            });

        $locationProvider.html5Mode(true);
    }]);

}());