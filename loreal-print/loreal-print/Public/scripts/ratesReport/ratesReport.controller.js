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
