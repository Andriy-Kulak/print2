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