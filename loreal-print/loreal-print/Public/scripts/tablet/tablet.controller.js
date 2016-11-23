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