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
