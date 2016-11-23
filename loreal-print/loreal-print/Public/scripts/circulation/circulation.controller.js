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
