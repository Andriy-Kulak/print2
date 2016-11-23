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
