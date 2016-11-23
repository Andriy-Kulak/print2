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
(function () {
    'use strict';

    angular.module('print.module')
    .controller('emailCtrl', ['$http', '$scope', '$rootScope', '$state', function emailCtrl($http, $scope, $rootScope, $state) {
        var vm = this;
        var dataService = $http;
        vm.$scope = $scope;

        //function emailCtrl($http) {
            // Hook up public events
            //vm.addClick = addClick;
            //vm.sendEmail = sendEmail;

            //vm.Emails = [];
            vm.email = {
                EmailTo: '',
                EmailSubject: '',
                EmailBody: ''
            };

            vm.sendEmail = function () {
                if (vm.emailForm.$valid) {
                    console.log("Email to be sent: ", vm.email);
                    dataService.post("api/Utilities/SendEmail/email",
                        vm.email)
                    .then(function (result) {
                        console.log("Email sent", result);
                    }, function (error) {
                        alert("There was a problem in the return from the email call: " + error);
                        handleException(error);
                    });
                }
            }

            function handleException(error) {
                vm.uiState.isValid = false;
                var msg = {
                    property: 'Error',
                    message: ''
                };

                vm.uiState.messages = [];

                switch (error.status) {
                    case 400:  // Bad Request
                        // Model state erros
                        var errors = error.data.ModelState;
                        debugger;

                        // Loop through and get all validation errors
                        for (var key in errors) {
                            for (var i = 0; i < errors[key].length; i++) {
                                addValidationMessage(key, errors[key][i]);
                            }
                            console.log("handleException errors case 400: ", errors);
                        }
                        break;

                    case 404:  // 'Not Found'
                        msg.message = 'The product you were ' +
                                      'requesting could not be found';
                        vm.uiState.messages.push(msg);

                        break;

                    case 500:  // 'Internal Error'
                        msg.message =
                          error.data.ExceptionMessage;
                        vm.uiState.messages.push(msg);

                        break;

                    default:
                        msg.message = 'Status: ' +
                                    error.status +
                                    ' - Error Message: ' +
                                    error.statusText;
                        vm.uiState.messages.push(msg);

                        break;
                }
            }

            function addValidationMessage(prop, msg) {
                vm.uiState.messages.push({
                    property: prop,
                    message: msg
                });
            }
        //}
    }]
)})();
(function () {
    'use strict';

    angular.module('print.module').controller('ratesCtrl', ['$http', '$scope', '$rootScope', 'toaster', 'modalService', 'ratesService', function ($http, $scope, $rootScope, toaster, modalService, ratesService) {

        var vm = this;

        // method for rendering rates data
        vm.getRatesData = function () {
            vm.showRatesTab = false; // prevents tabs from showing until data is loaded
            ratesService.getRatesDataService().then(function (result) {
                vm.ratesData = result.data;
                vm.showRatesTab = true; // removes the loading info

                //shortened versions of object for Page Rates - General
                vm.parentP4C = vm.ratesData.RateParentAdTypes[0];
                vm.adTypeP4CnonBleed = vm.ratesData.RateParentAdTypes[0].RateRateTypes[0].RateAdTypes[1];
                vm.adTypeP4CB = vm.ratesData.RateParentAdTypes[0].RateRateTypes[0].RateAdTypes[2];

                // method for calculating Open Bleed Premium (in %) = [(2017 NET P4CB Open Rate) - (2017 NET P4C Open Rate)] / (2017 NET P4C Open Rate)
                vm.openBleedFormula = function () {
                    if (vm.adTypeP4CB.RateTiers[0].RateEditionTypes[0].Rate === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate === 0) {
                        vm.parentP4C.BleedOpenPercentPremium = null;
                    } else if (vm.adTypeP4CB.RateTiers[0].RateEditionTypes[0].Rate >= 0 && vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate > 0) {
                        vm.parentP4C.BleedOpenPercentPremium = ((vm.adTypeP4CB.RateTiers[0].RateEditionTypes[0].Rate - vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate) / vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate * 100).toFixed(2);
                    }
                }

                // method for Earned NET P4CB *CPM*
                vm.earnedNetP4CBcpm = function () {
                    //Earned NET P4CB Rate"/ 2017 Guaranteed Rate Base
                    for (var i = 1; i < vm.adTypeP4CnonBleed.RateTiers.length; i++) {
                        if (vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                            vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].CPM = null;
                        } else if (vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                            vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].CPM = (vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate / vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                        }
                    }
                }

                // method for Earned NET P4C *CPM* calculation
                vm.earnedNetP4Cformula = function () {
                    // "Earned NET P4C Rate"/ 2017 Guaranteed Rate Base
                    for (var i = 1; i < vm.adTypeP4CnonBleed.RateTiers.length; i++) {
                        if (vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                            vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].CPM = null;
                        } else if (vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                            vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].CPM = (vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate / vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                        }
                    }
                }


                // method for Earned NET P4CB *Rate*
                vm.earnedNetP4CBrate = function () {
                    // Earned NET P4C Rate * (1 + Earned Bleed Premium/100)
                    for (var i = 1; i < vm.adTypeP4CnonBleed.RateTiers.length; i++) {
                        if (vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate === null || vm.parentP4C.BleedEarnedPercentPremium === null) {
                            vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate = null;
                        } else if (vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.parentP4C.BleedEarnedPercentPremium >= 0) {

                            vm.adTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate = (vm.adTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate * (1 + (vm.parentP4C.BleedEarnedPercentPremium / 100))).toFixed(2);
                        }
                    }
                }


                // if retail information exists, then calculate values. Otherwise data will break with undefined values
                if (vm.ratesData.RateParentAdTypes[0].RateRateTypes[1] !== undefined) {

                    vm.retailAdTypeP4CB = vm.ratesData.RateParentAdTypes[0].RateRateTypes[1].RateAdTypes[2];
                    vm.retailAdTypeP4CnonBleed = vm.ratesData.RateParentAdTypes[0].RateRateTypes[1].RateAdTypes[1];


                    // method for Retail Earned NET P4CB *Rate*
                    vm.retailEarnedNetP4CBrate = function () {
                        // Retail Earned NET P4C Rate * (1 + Earned Bleed Premium/100)
                        for (var i = 1; i < vm.retailAdTypeP4CnonBleed.RateTiers.length; i++) {
                            if (vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate === null || vm.parentP4C.BleedEarnedPercentPremium === null) {
                                vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate = null;
                            } else if (vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.parentP4C.BleedEarnedPercentPremium >= 0) {
                                vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate = (vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate * (1 + (vm.parentP4C.BleedEarnedPercentPremium / 100))).toFixed(2);
                            }
                        }
                    }

                    // method for Retail Earned NET P4C *CPM* calculation
                    vm.retailEarnedNetP4Cformula = function () {
                        // "Retail Earned NET P4C Rate"/ 2017 Guaranteed Rate Base
                        for (var i = 1; i < vm.retailAdTypeP4CnonBleed.RateTiers.length; i++) {
                            if (vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                                vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].CPM = null;
                            } else if (vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                                vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].CPM = (vm.retailAdTypeP4CnonBleed.RateTiers[i].RateEditionTypes[0].Rate / vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }

                    // method for Retail Earned NET P4CB *CPM*
                    vm.retailEarnedNetP4CBcpm = function () {
                        //Earned NET P4CB Rate/ 2017 Guaranteed Rate Base
                        for (var i = 1; i < vm.retailAdTypeP4CnonBleed.RateTiers.length; i++) {
                            if (vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                                vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].CPM = null;
                            } else if (vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                                vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].CPM = (vm.retailAdTypeP4CB.RateTiers[i].RateEditionTypes[0].Rate / vm.adTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }

                    // *** Retail OPEN Net P4CB Rate Formula NEGMAT ***
                    vm.retailOpenNetP4CBrate = function () {
                        // Retail Open NET P4C Rate * (1 + Open Bleed Premium/100)
                        if (vm.ratesData.RateParentAdTypes[0].RateRateTypes[1]) {
                            if (vm.retailAdTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate === null || vm.parentP4C.BleedOpenPercentPremium === null) {
                                vm.retailAdTypeP4CB.RateTiers[0].RateEditionTypes[0].Rate = null;
                            } else if (vm.retailAdTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate >= 0 && vm.parentP4C.BleedOpenPercentPremium >= 0) {
                                vm.retailAdTypeP4CB.RateTiers[0].RateEditionTypes[0].Rate = (vm.retailAdTypeP4CnonBleed.RateTiers[0].RateEditionTypes[0].Rate * (1 + (vm.parentP4C.BleedOpenPercentPremium / 100))).toFixed(2);
                            }
                        }
                    }
                }

                //shortened versions of object for Scent Strip
                vm.parentScentStrip = vm.ratesData.RateParentAdTypes[3];
                vm.openScentStrip = vm.ratesData.RateParentAdTypes[3].RateRateTypes[0].RateAdTypes[0].RateTiers[0];

                //method for Scent Strip -> Full Run NET Earned CPM
                // = (Full Run Earned Rate) / (Full Run Rate Base Circulation Guarantee)
                vm.fullRunEarnedCPM = function () {
                    for (var x = 0; x < vm.parentScentStrip.RateRateTypes[0].RateAdTypes.length; x++) {
                        for (var i = 0; i < vm.parentScentStrip.RateRateTypes[0].RateAdTypes[0].RateTiers.length; i++) {
                            if (vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate === null || vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                                vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].CPM = null;
                            } else if (vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                                vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].CPM = (vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate / vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }
                }

                //method for Scent Strip -> SUBSCRIPTION ONLY NET Earned CPM
                // = (Subscription Only Earned Rate) / (Subscription Rate Base Circulation Guarantee)
                vm.subscriptionEarnedCPM = function () {
                    for (var x = 0; x < vm.parentScentStrip.RateRateTypes[0].RateAdTypes.length; x++) {
                        for (var i = 0; i < vm.parentScentStrip.RateRateTypes[0].RateAdTypes[0].RateTiers.length; i++) {
                            if (vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate === null || vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee === null || vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee === 0) {
                                vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].CPM = null;
                            } else if (vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate >= 0 && vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee > 0) {
                                vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].CPM = (vm.parentScentStrip.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate / vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }
                }

                //shortened versions of object for Inserts
                vm.parentInsert = vm.ratesData.RateParentAdTypes[1];
                vm.openInsert = vm.ratesData.RateParentAdTypes[1].RateRateTypes[0].RateAdTypes[0].RateTiers[0];


                //method for Inserts -> Full Run NET Earned CPM
                //// = (Full Run Earned Rate) / (Full Run Rate Base Circulation Guarantee)
                vm.insertsFullRunEarnedCPM = function () {
                    for (var x = 0; x < vm.parentInsert.RateRateTypes[0].RateAdTypes.length; x++) {
                        for (var i = 0; i < vm.parentInsert.RateRateTypes[0].RateAdTypes[0].RateTiers.length; i++) {
                            if (vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate === null || vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                                vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].CPM = null;
                            } else if (vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                                vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].CPM = (vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate / vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }
                }

                //method for Inserts -> SUBSCRIPTION ONLY NET Earned CPM
                // = (Subscription Only Earned Rate) / (Subscription Rate Base Circulation Guarantee)
                vm.insertsSubscriptionEarnedCPM = function () {
                    for (var x = 0; x < vm.parentInsert.RateRateTypes[0].RateAdTypes.length; x++) {
                        for (var i = 0; i < vm.parentInsert.RateRateTypes[0].RateAdTypes[0].RateTiers.length; i++) {
                            if (vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate === null || vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee === null || vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee === 0) {
                                vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].CPM = null;
                            } else if (vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate >= 0 && vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee > 0) {
                                vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].CPM = (vm.parentInsert.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate / vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }
                }

                //shortened versions of object for BRC Cards
                vm.parentBrcCards = vm.ratesData.RateParentAdTypes[2];
                vm.openBrcCards = vm.ratesData.RateParentAdTypes[2].RateRateTypes[0].RateAdTypes[0].RateTiers[0];

                //method for BRC Cards -> Full Run NET Earned CPM
                vm.brcCardsFullRunEarnedCPM = function () {
                    for (var x = 0; x < vm.parentBrcCards.RateRateTypes[0].RateAdTypes.length; x++) {
                        for (var i = 0; i < vm.parentBrcCards.RateRateTypes[0].RateAdTypes[0].RateTiers.length; i++) {
                            if (vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate === null || vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee === null || vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee === 0) {
                                vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].CPM = null;
                            } else if (vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate >= 0 && vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee > 0) {
                                vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].CPM = (vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[0].Rate / vm.openScentStrip.RateEditionTypes[0].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }
                }

                //method for BRC Cards -> SUBSCRIPTION ONLY NET Earned CPM
                vm.brcCardsSubscriptionEarnedCPM = function () {
                    for (var x = 0; x < vm.parentBrcCards.RateRateTypes[0].RateAdTypes.length; x++) {
                        for (var i = 0; i < vm.parentBrcCards.RateRateTypes[0].RateAdTypes[0].RateTiers.length; i++) {
                            if (vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate === null || vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee === null || vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee === 0) {
                                console.log("Hitting null or zero", vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee);
                                vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].CPM = null;
                            } else if (vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate >= 0 && vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee > 0) {
                                console.log("hitting > 0 ", vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee);
                                vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].CPM = (vm.parentBrcCards.RateRateTypes[0].RateAdTypes[x].RateTiers[i].RateEditionTypes[1].Rate / vm.openScentStrip.RateEditionTypes[1].RateBaseCirculationGuarantee).toFixed(2);
                            }
                        }
                    }
                }

            }, function (error) {
                console.log("GET Method not working");
                vm.loadHide = true; //hides the load modal
            });
        }
        vm.getRatesData();
       
        // method for saving rates data
        vm.saveRates = function () {
            console.log("stuff to be submitted", vm.ratesData);

            vm.isTimingOut = true;
            ratesService.saveRatesDataService(vm.ratesData).then(function (result) {
                console.log("pass")
                modalService.saveSuccessModal();
                vm.isTimingOut = false;
            }, function (error) {

                modalService.saveErrorModal(error.data.Message);
                console.log("Error", error);
                vm.isTimingOut = false;
            });
        }



    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module').service('ratesService', ['$http', function ($http) {
        var vm = this;

         vm.getRatesDataService = function() {
            return $http.get("api/Rates/GetRates");
         }

         vm.saveRatesDataService = function (data) {
             return $http.post("/api/Rates/saveRates/RatesModel", data);
         }

         vm.openBleedFormula = function (netP4CBopenRate, netP4CopenRate, bleedValue) {
             if (netP4CBopenRate && netP4CopenRate) {
                 bleedValue = ((netP4CBopenRate - netP4CopenRate) / netP4CopenRate * 100).toFixed(2);
             }
         }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module')
    .controller('signsubmitCtrl', ['$http', '$scope', '$rootScope', '$state', '$sce', '$window', 'modalService', function signsubmitCtrl($http, $scope, $rootScope, $state, $sce, $window, modalService) {

        var vm = this;
        var dataService = $http;
        vm.$scope = $scope;

        vm.validateInputs = function () {           
            dataService.get("api/PDFOutput/ValidateAllInputs")
                .then(function (result) {
                    vm.validationArray = result.data[0].split(";");
                    vm.validationArray = vm.validationArray.filter(function (n) { return n !== "" });
                    console.log("valid output result", vm.validationArray);
                }, function (err) {
                    console.log(err);
                    modalService.saveErrorModal();
                }
                );

        }

        vm.validateInputs();

        vm.printPDF = function () {
            modalService.loadingModal();
            vm.loadHide = false;
            dataService.get('api/PDFOutput/PrintPDFOutput', { responseType: 'arraybuffer' })
            .success(function (data) {
                $scope.info = "Read'" + $scope.URL + "' with " + data.byteLength
                + " bytes in a variable of type '" + typeof (data) + "'";
                var file = new Blob([data], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(file);
                $window.open(fileURL, '_blank');
                modalService.pdfSuccessModal();
                vm.loadHide = true;
            }).
            error(function (data, status) {
                vm.loadHide = true;
                modalService.saveErrorModal(data);
                $scope.info = "Request failed with status: " + status;
            });

        }
    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module')
    .controller('editorialCalendarCtrl', ['$http', '$scope', 'modalService', 'editCalService', function ($http, $scope, modalService, editCalService) {
        var vm = this;
        var dataService = $http;
        vm.answers = []; // array for storing answer objects

        //Stores Publisher Agreement Responses to Records
        vm.calendarRecords = [];

        // get editorial calendar data
        vm.getCalendarOutput = function () {
            editCalService.getCalData().then(function (result) {
                vm.records = result.data;

            }, function (error) {
                console.log("GET Method not working");
            });
        }
        vm.getCalendarOutput();
        
        // method that asks user to confirm if they want to delete a row
        vm.cofirmDelete = function (record) {
            modalService.confirmDeleteModal();
            vm.recordDelete = record;
        }

        vm.completeDelete = function (id) {
            editCalService.deleteCalData(id).then(function (result) {
                vm.getCalendarOutput();
                modalService.successDeleteModal();

            }, function (error) {
                modalService.saveErrorModal(error);
            });
        }

        vm.rowTouched = function(index){
           return vm.Test = index;
        }
        
        // save data for editorial calendar
        vm.saveEditorialCalendar = function (data, validUpdate) {
            //if form is valid, then submit the request for new row addition or row update
            try {
                if (vm.newCal.$valid === true || validUpdate === true) {
                    console.log("VALID", vm.newCal.$valid);

                    $http.post('api/EditorialCalendar/saveEditorialCalendar/calendarRecord', data).then(function (result) {
                        modalService.changesSaved(); //modal for saving changes
                        vm.calendarRecord = ""; // clears record from row inserted
                        vm.getCalendarOutput(); //triggers GET request and refreshes the data in UI
                        vm.newCal.$setPristine(); //clears validation for input
                        vm.newCal.$setUntouched(); //clears validation for input
                        vm.rowTouched = [];
                        console.log("PASS");
                    }), function (error) {

                        modalService.saveErrorModal(error);
                        console.log("POST FAIL");
                    }
                } else {
                    modalService.incorrectSubmitModal();
                    console.log("Incorrect");
                };
            }
            catch (err) {
                modalService.saveErrorModal(error);
                console.log("POST FAIL");
            }
        }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module').service('editCalService', ['$http', function ($http) {
        var vm = this;

        // gets editorial calendar row
        vm.getCalData = function () {
            return $http.get("api/EditorialCalendar/GetCalendar");
        }

        // deletes the selected row
        vm.deleteCalData = function (id) {
            return $http.delete('api/EditorialCalendar/' + id);
        }

        // saved or updates editorial calendar data
        vm.saveCalData = function (data) {

            return $http.post('api/EditorialCalendar/saveEditorialCalendar/calendarRecord', data);
        }
       
    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module')
    .controller('tabletCtrl', ['$http', '$scope', '$rootScope','modalService', function ($http, $scope, $rootScope, modalService) {
        var vm = this;
        var dataService = $http;
      
        // Controller Functions
        function getTabletRateOutput() {
            dataService.get("api/Tablet/GetTabletRates")
            .then(function (result) {
                vm.records = result.data;
                console.log("Tablet Rates output: ", vm.records);
                console.log("length", Object.keys(vm.records).length);

            }, function (error) {
                handleException(error);
            });
        }
        getTabletRateOutput();
        
        //stores all the changes for Tablet Data
        vm.tabletChanges = [];
        vm.getTabletAnswers = function (data) {
            for (var i = 0; i < vm.tabletChanges.length; i++) {
                if (data.TabletFunctionalityID === vm.tabletChanges[i].TabletFunctionalityID) {
                    vm.tabletChanges.splice(i, 1);
                }
            }

            vm.tabletChanges.push({
                TabletFunctionalityID: data.TabletFunctionalityID,
                EarnedRate: data.EarnedRate,
                OpenRate: data.OpenRate
            })

            console.log("array", vm.tabletChanges);
        };

        vm.saveTabletAnswers = function () {
            vm.isTimingOut = true;
            dataService.post('api/Circulation/saveTabletRates/rates', vm.tabletChanges).then(function (result) {
                console.log("pass", result);
                modalService.saveSuccessModal();
                vm.tabletRateRecords = [];
                vm.isTimingOut = false;
            }, function (error) {
                console.log("post fail");
                modalService.saveErrorModal(error.data.Message);
                console.log(error);
                vm.isTimingOut = false;
            });
        };

    }]
)
})();
(function () {
    'use strict';

    angular.module('print.module')
    .controller('circulationCtrl', ['$http', 'modalService', 'circService',
        function ($http, modalService, circService) {
        var vm = this;

        //gets circulation output answers
        vm.getCirculationOutput = function() {
            circService.getCircData().then(function (result) {
                vm.circ = result.data;

            }, function (error) {
                handleException(error);
            });
        }
        vm.getCirculationOutput();

        //stores all the changes for Circulation in circChanges array
        vm.circChanges = [];
        vm.getCircAnswers = function (data) {
            for (var i = 0; i < vm.circChanges.length; i++) {
                if (data.CirculationSubTypeID === vm.circChanges[i].CirculationSubTypeID) {
                    vm.circChanges.splice(i, 1);
                }
            }

            vm.circChanges.push({
                CirculationSubTypeID: data.CirculationSubTypeID,
                PrintCirculation: data.PrintCirculation,
                DigitalReplicaCirculation: data.DigitalReplicaCirculation,
                DigitalNonReplicaCirculation: data.DigitalNonReplicaCirculation
            })
        };

        // saves circulation data submitted by user
        vm.saveCircAnswers = function () {

            // checks if array is empty -> if yes, tells user that there are no answers to be submitted
            if (Object.keys(vm.circChanges).length === 0) {
                vm.isTimingOut = false;
                return  modalService.noNewAnswersModal();
            }

            vm.isTimingOut = true;
            circService.saveCircData(vm.circChanges).then(function (result) {
                console.log("pass", result);
                modalService.saveSuccessModal();
                vm.circChanges = [];
                vm.isTimingOut = false;
            }, function (error) {
                console.log("post fail");
                modalService.saveErrorModal(error.data.Message);
                vm.isTimingOut = false;
            });
        }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module').service('circService', ['$http', function ($http) {
        var vm = this;

        // retrive data
        vm.getCircData = function () {
            return $http.get("api/circulation/GetCirculation");
        }

        // save data
        vm.saveCircData = function (answers) {
            return $http.post('api/Circulation/saveCirculationRecords/CircContent', answers);
        }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module')
    .controller('booksCtrl', ['$http', '$state', 'modalService', 'bookService',
        function booksCtrl($http, $state, modalService, bookService) {

        var vm = this;
        vm.books = []; //array of book objects

        vm.selectedBookOptions = function () {
            
            if (vm.booksForm.$valid) {
                bookService.selectedBook(vm.selectedBook.ID)
                .then(function (result) {                  
                    // Redirect to Terms tab
                     $state.go('print.terms');
                }, function (err) {
                    console.log(err);
                    modalService.saveErrorModal(err);
                }
                );
            }
        }        

        function getBooksList() {
            bookService.getBooks().then(function (result) {
                vm.books = result.data;
                console.log(vm.books);
            }, function (error) {
                handleException(error);
            });
        }
        getBooksList();
    }]
)})();

(function () {
    'use strict';

    angular.module('print.module').service('bookService', ['$http', function ($http) {
        var vm = this;

        vm.getBooks = function () {
            return $http.get("/api/Books/GetBooks/2017");
        }

        vm.selectedBook = function (bookId) {
            return $http.put('api/books/' + bookId);
        }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module').service('modalService', ['toaster', function(toaster) {
        var vm = this;

        vm.saveSuccessModal = function () {
            toaster.pop({
                type: 'success',
                title: 'Saved!',
                body: 'Your answers have been stored.',
                toasterId: 1
            });
        }

        vm.pdfSuccessModal = function () {
            toaster.pop({
                type: 'success',
                title: 'Success!',
                body: 'Your PDF has been generated in a new tab.',
                toasterId: 1
            });
        }

        vm.saveErrorModal = function (message) {
            if(message===undefined){ message = "Please contact the administrator."}
            toaster.pop({
                type: 'error',
                title: 'Error!',
                toasterId: 1,
                body: 'There was an error saving your request. ' + message});
        }

        vm.successDeleteModal = function () {
            toaster.pop({
                type:'success',
                title: 'Success!',
                toasterId: 1,
                body: 'The selected row has been deleted'});
        }

        vm.noNewAnswersModal = function () {
            toaster.pop({
                type:'info',
                title: "No changes to Save!",
                body: "Please submit answers before saving.",
                toasterId: 1
            });
        }

        vm.incorrectSubmitModal = function () {
            toaster.pop({
                type: 'info',
                title: "Warning!",
                toasterId: 2,
                body: "Please input all fields correctly before submitting.",
            });
        }

        vm.confirmDeleteModal = function () {
            toaster.pop({
                type: 'error',
                title: "Warning!",
                body: 'Public/scripts/sharedServices/confirmDelete.html',
                toasterId: 2,
                bodyOutputType: 'template'
            });
        }

        vm.loadingModal = function () {
            toaster.pop({
                type: 'info',
                title: "Loading...",
                body: 'Public/scripts/sharedServices/loadingModal.html',
                toasterId: 3,
                bodyOutputType: 'template'
            });
        }

        vm.changesSaved = function () {
            toaster.pop({
                type: 'success',
                title: "Success!",
                toasterId: 1,
                body: "You changes have been saved!"});
        }

        vm.noNewAnswersModal = function () {
            toaster.pop({
                type: 'info',
                title: "No changes to Save!",
                toasterId: 1,
                body: "Please submit answers before saving."});
        }

        vm.noNewAnswersModal = function () {
            toaster.pop({
                type: 'info',
                title: "No changes to Save!",
                toasterId: 1,
                body: "Please submit answers before saving."});
        }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module')
    .controller('ratesReportCtrl', ['$http', '$scope', '$rootScope', 'modalService', 'ratesReportService', function ($http, $scope, $rootScope, modalService, ratesReportService) {
        var vm = this;
        var dataService = $http;
        vm.$scope = $scope;

        // get rates report chart data
        vm.getRatesReportList = function () {
            ratesReportService.getRatesRepData().then(function (result) {
                vm.records = result.data;

            }, function (error) {
                handleException(error);
            });
        }
        vm.getRatesReportList();

        //stores all the changes for Publisher Agreement in an array
        // the array will only add values if the answers have been changed from current entries
        vm.rateReportChanges = [];
        vm.getValues = function (data, rateTypeId) {
            for (var i = 0; i < vm.rateReportChanges.length; i++) {
                if (data.AdTypeID === vm.rateReportChanges[i].AdTypeID && data.RateTypeID === vm.rateReportChanges[i].RateTypeID) {
                    vm.rateReportChanges.splice(i, 1);
                }
            }

            vm.rateReportChanges.push({
                RateTypeID : rateTypeId,
                AdTypeID: data.AdTypeID,
                EditionTypeID: data.EditionTypeID,
                Answer: data.PublisherAgreement
            })
        };

        //save publisher agreement entries/answers
        vm.savePubAgreement = function () {
            if (Object.keys(vm.rateReportChanges).length === 0) {
                return modalService.noNewAnswersModal();
            }

            ratesReportService.savePubAgreementData(vm.rateReportChanges).then(function (result) {
                    modalService.saveSuccessModal();
                    vm.rateReportChanges = [];
                }, function (error) {
                    modalService.saveErrorModal(error.data.Message);
                    console.log(error);
                });
            }
    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module').service('ratesReportService', ['$http', function ($http) {
        var vm = this;

        vm.getRatesRepData = function () {
            return $http.get("api/RatesReport/GetRatesReport");
        }

        vm.savePubAgreementData = function (data) {
            return $http.post("api/RatesReport/savePublisherAgreements/answers", data);
        }

    }]
)
})();

(function () {
    'use strict';

    angular.module('print.module')
    .controller('termsCtrl', ['$http', 'toaster', 'modalService', 'termsService', function ($http, toaster, modalService, termsService) {
        var vm = this;

        //Stores Top Level Answers to Questions
        vm.termAnswers;

        // Controller Functions
        vm.getQuestionsList = function() {
            termsService.getTermsData().then(function (result) {
                vm.terms = result.data;
                           
            }, function (error) {
                modalService.saveErrorModal(error.data.Message);
            });
        }
        vm.getQuestionsList();

        vm.getAnswersList = function() {
            termsService.getTermsAnswers().then(function (result) {

                var getAnsIndex = result.data;
                vm.termYesNoAns = [];
                vm.termFreeFormAns = [];

                for (var i = 0; i < Object.keys(getAnsIndex).length; i++) {

                    vm.termYesNoAns[getAnsIndex[i].QuestionID] = getAnsIndex[i].AnswerYesNo;
                    vm.termFreeFormAns[getAnsIndex[i].QuestionID] = getAnsIndex[i].AnswerFreeForm;
                }
            }, function (error) {
                console.log("no answers");
            });
        }
        vm.getAnswersList();

        //method for saving answers      
        vm.saveAnswers = function() {
            //combines the index of YesNo and FreeForm Answers
            if (!vm.termYesNoAns) { vm.termYesNoAns = []; }
            if (!vm.termFreeFormAns) { vm.termFreeFormAns = []; }

            vm.isTimingOut = true;

            var AnsIndex = Object.keys(vm.termYesNoAns).concat(Object.keys(vm.termFreeFormAns));
            //creates a unique index AnsIndex
            var uniqueAnsIndex = AnsIndex.filter(function (item, i, ar) { return ar.indexOf(item) === i; })

            //termAnswers is where all the answer objects will be stored for QuestionID, AnswerFreeForm, AnswerYesNo
            vm.termAnswers = [];
            //loop that pushes all the answers into termAnswers
            for (var i = 0; i < uniqueAnsIndex.length; i++) {
                vm.termAnswers.push({
                    QuestionID: uniqueAnsIndex[i],
                    AnswerYesNo: vm.termYesNoAns[uniqueAnsIndex[i]],
                    AnswerFreeForm: vm.termFreeFormAns[uniqueAnsIndex[i]]
                });
            }
          
            termsService.saveTermsAnswers(vm.termAnswers).then(function (result) {
                vm.answers = result.data;
                modalService.saveSuccessModal();
                vm.isTimingOut = false;


            }, function (error) {
                handleException(error);
                modalService.saveErrorModal();
                vm.isTimingOut = false;
            });
        }
      
    }]
)})();

(function () {
    'use strict';

    angular.module('print.module').service('termsService', ['$http', function ($http) {
        var vm = this;

        vm.getTermsData = function () {
            return $http.get("/api/Terms");
        }

        vm.getTermsAnswers = function () {
            return $http.get("api/Terms/GetAnswers");
        }

        vm.saveTermsAnswers = function (answers) {
            return $http.post("/api/terms/saveAnswers/answers", answers);
        }
    }]
)
})();
