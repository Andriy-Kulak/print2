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
