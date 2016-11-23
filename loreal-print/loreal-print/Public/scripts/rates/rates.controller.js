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
