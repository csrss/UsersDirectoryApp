var app = angular.module('myApp', ['ui.bootstrap', 'ui.bootstrap.modal', 'ngCookies', 'angular-loading-bar']);
app.controller('HomeCtrl', ['$scope', '$http', '$modal', '$cookies', '$cookieStore', function ($scope, $http, $modal, $cookies, $cookieStore) {
    $scope.searchFilter = "";
    $scope.sortType = "FirstName";
    $scope.sortReverse = false;
    $scope.currentPage = 1;
    $scope.numPerPage = 50;
    $scope.numPerPageOptions = [10, 20, 30, 40, 50];
    $scope.totalItems = 0;

    $scope.setup = function () {
        var pageSize = $cookieStore.get('numPerPage');
        if (pageSize !== undefined && pageSize === parseInt(pageSize, 10)) {
            $scope.numPerPage = pageSize;
        }
    }
    $scope.setup();

    /*********************************************************************************************/
    $scope.reloadUsers = function () {
        $http.post('/Home/PostRequestProcessor', {
            request: JSON.stringify({
                ID: 'HttpPostRequest_Users',
                Page: $scope.currentPage,
                TotalPerPage: $scope.numPerPage,
                Filter: $scope.searchFilter,
                SortBy: $scope.sortType,
                SortOrder: !$scope.sortReverse
            })
        }, { headers: { 'Content-Type': 'application/json' } }).then(function (response) {
            if (response.data.Result) {
                // Mamy userków
                $scope.users = response.data.Users;
                $scope.totalItems = response.data.TotalUsersCount;

                // Dekorujemy obiekt usera dodatkowo o potrzebne nam wartości
                angular.forEach($scope.users, function (obj) {
                    obj["showEdit"] = true;
                    obj["DateJS"] = new Date(obj.Birthdate);
                })
            }
            else {
                // Cos sie popsulo
                $scope.showAlert('Failed to fetch users', response.data.Message);
            }
        }, function (error) {
            $scope.showAlert('Failed to contact server', 'HTTP status code: ' + error.status + '. HTTP status text: ' + error.statusText);
        });
    }
    $scope.reloadUsers();
    /*********************************************************************************************/

    $scope.toggleEdit = function (user) {
        user.showEdit = user.showEdit ? false : true;
    }
    $scope.updateUser = function (user) {
        $http.post('/Home/PostRequestProcessor', {
            request: JSON.stringify({
                ID: 'HttpPostRequest_UpdateUser',
                UserID: user.PersonID,
                Firstname: user.FirstName,
                Lastname: user.LastName,
                Birthdate: user.DateJS,
                Height: user.Height,
                ClubMember: user.Member
            })
        }, { headers: { 'Content-Type': 'application/json' } }).then(function (response) {
            if (response.data.Result) {
                // Tutaj to moglibysmy zwrocic rezult ze zmodyfikowanym userem, wtedy zaktualizujemy jego jakies tam dodatkowe dane.
                // W tym przypadku data urodzenia.
                //     user.showEdit = user.showEdit ? false : true;
                $scope.reloadUsers();
            }
            else {
                // Cos sie popsulo
                $scope.showAlert('Failed to update user', response.data.message);
            }
        }, function (error) {
            $scope.showAlert('Failed to contact server', 'HTTP status code: ' + error.status + '. HTTP status text: ' + error.statusText);
        });
    }

    /*
        Pokazuje zwykłe alerty, które można jedyne co to sdismisować.
    */
    $scope.showAlert = function (title, content) {
        var modalInstance = $modal.open({
            template:
                        '<div>' +
                        '<div class="modal-header app-header-colors">' +
                            '<h3>' + title + '</h3>' +
                        '</div>' +
                        '<div class="modal-body">' +
                            content +
                        '</div>' +
                        '<div class="modal-footer app-footer-colors">' +
                            '<button class="btn btn-default app-buton-default-sizes" ng-click="ok()">Close</button>' +
                        '</div>' +
                    '</div>',
            controller: function ($scope, $modalInstance) {
                $scope.ok = function () {
                    $modalInstance.dismiss('ok');
                }
            },
        });
    }

    /*
        Pytamy controller czy czasme nie mogl by usunąc dla nas bazy danych.
    */
    $scope.requestResetDatabase = function (tmpl) {
        var modalInstance = $modal.open({
            template: tmpl,
            controller: function ($scope, $modalInstance) {
                $scope.ok = function () {
                    $modalInstance.close(true);
                };
                $scope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            },
        });

        modalInstance.result.then(function (mResult) {
            // W tym przypadku bedzie tylko jeden result - true, ale dobra no, sprawdze dodatkowo
            if (mResult) {
                $http.post('/Home/PostRequestProcessor', { request: JSON.stringify({ ID: 'HttpPostRequest_DropDatabase' }) }, { headers: { 'Content-Type': 'application/json' } }).then(function (response) {
                    if (response.data.Result) {
                        // Usunieta baza
                        $scope.reloadUsers();
                    }
                    else {
                        // Cos sie popsulo
                        $scope.showAlert('Failed to reset database', response.data.message);
                    }
                }, function (error) {
                    $scope.showAlert('Failed to contact server', 'HTTP status code: ' + error.status + '. HTTP status text: ' + error.statusText);
                });
            }
        }, function () {
            // Dialog został zamkniety na cancel - nic nie robimy
        });
    }


    $scope.requestDeleteUser = function (tmpl, user) {
        var modalInstance = $modal.open({
            template: tmpl,
            controller: function ($scope, $modalInstance, user) {
                $scope.user = user;
                $scope.ok = function () {
                    $modalInstance.close(true);
                };
                $scope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            },
            resolve: {
                user: function () {
                    return user;
                }
            }
        });

        modalInstance.result.then(function (mResult) {
            // W tym przypadku bedzie tylko jeden result - true, ale dobra no, sprawdze dodatkowo
            if (mResult) {
                $http.post('/Home/PostRequestProcessor', {
                    request: JSON.stringify({
                        ID: 'HttpPostRequest_DropUser',
                        UserID: user.PersonID
                    })
                }, { headers: { 'Content-Type': 'application/json' } }).then(function (response) {
                    if (response.data.Result) {
                        // Usunieta baza
                        $scope.reloadUsers();
                    }
                    else {
                        // Cos sie popsulo
                        $scope.showAlert('Failed to delete user', response.data.message);
                    }
                }, function (error) {
                    $scope.showAlert('Failed to contact server', 'HTTP status code: ' + error.status + '. HTTP status text: ' + error.statusText);
                });
            }
        }, function () {
            // Dialog został zamkniety na cancel - nic nie robimy
        });
    }


    $scope.requestCreateUser = function (tmpl) {
        $scope.user_template = {
            firstname: '',
            lastname: '',
            birthdate: new Date(),
            height: 180,
            member: false
        };

        var modalInstance = $modal.open({
            template: tmpl,
            controller: function ($scope, $modalInstance, user_template) {
                $scope.user_template = user_template;
                $scope.ok = function () {
                    if ($scope.user_template.firstname != "" && $scope.user_template.lastname != "")
                        $modalInstance.close(true);
                };
                $scope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            },
            resolve: {
                user_template: function () {
                    return $scope.user_template;
                }
            }
        });

        modalInstance.result.then(function (mResult) {
            // W tym przypadku bedzie tylko jeden result - true, ale dobra no, sprawdze dodatkowo
            if (mResult) {
                $http.post('/Home/PostRequestProcessor', {
                    request: JSON.stringify({
                        ID: 'HttpPostRequest_AddUser',
                        Firstname: $scope.user_template.firstname,
                        Lastname: $scope.user_template.lastname,
                        Birthdate: $scope.user_template.birthdate,
                        Height: $scope.user_template.height,
                        ClubMember: $scope.user_template.member
                    })
                }, { headers: { 'Content-Type': 'application/json' } }).then(function (response) {
                    if (response.data.Result) {
                        // success
                        $scope.reloadUsers();
                    }
                    else {
                        // Cos sie popsulo
                        $scope.showAlert('Failed to create new user', response.data.message);
                    }
                }, function (error) {
                    $scope.showAlert('Failed to contact server', 'HTTP status code: ' + error.status + '. HTTP status text: ' + error.statusText);
                });
            }
        }, function () {
            // Dialog został zamkniety na cancel - nic nie robimy
        });
    }

    $scope.doubleClick = function (tmpl, user) {
        $scope.user_template = {
            firstname: user.FirstName,
            lastname: user.LastName,
            birthdate: user.DateJS,
            height: user.Height,
            member: user.Member
        };

        var modalInstance = $modal.open({
            template: tmpl,
            controller: function ($scope, $modalInstance, user_template) {
                $scope.user_template = user_template;
                $scope.ok = function () {
                    if ($scope.user_template.firstname != "" && $scope.user_template.lastname != "")
                        $modalInstance.close(true);
                };
                $scope.cancel = function () {
                    $modalInstance.dismiss('cancel');
                };
            },
            resolve: {
                user_template: function () {
                    return $scope.user_template;
                }
            }
        });

        modalInstance.result.then(function (mResult) {
            if (mResult) {
                $http.post('/Home/PostRequestProcessor', {
                    request: JSON.stringify({
                        ID: 'HttpPostRequest_UpdateUser',
                        UserID: user.PersonID,
                        Firstname: $scope.user_template.firstname,
                        Lastname: $scope.user_template.lastname,
                        Birthdate: $scope.user_template.birthdate,
                        Height: $scope.user_template.height,
                        ClubMember: $scope.user_template.member
                    })
                }, { headers: { 'Content-Type': 'application/json' } }).then(function (response) {
                    if (response.data.Result) {
                        // Tutaj to moglibysmy zwrocic rezult ze zmodyfikowanym userem, wtedy zaktualizujemy jego jakies tam dodatkowe dane.
                        // W tym przypadku data urodzenia.
                        //     user.showEdit = user.showEdit ? false : true;
                        $scope.reloadUsers();
                    }
                    else {
                        // Cos sie popsulo
                        $scope.showAlert('Failed to update user', response.data.message);
                    }
                }, function (error) {
                    $scope.showAlert('Failed to contact server', 'HTTP status code: ' + error.status + '. HTTP status text: ' + error.statusText);
                });
            }
        }, function () {
            // Dialog został zamkniety na cancel - nic nie robimy
        });
    }

    $scope.selectedRow = null;
    $scope.setClickedRow = function (index) {
        $scope.selectedRow = index;
    }

    $scope.perPageChanged = function () {
        var coockiePolicyAccepted = $cookieStore.get('consent');
        if (coockiePolicyAccepted !== undefined && coockiePolicyAccepted === true) {
            $cookieStore.put('numPerPage', $scope.numPerPage);
        }
        $scope.reloadUsers();
    }

}]).directive('innerHtmlBind', function () {
    return {
        restrict: 'A',
        scope: {
            inner_html: '=innerHtml'
        },
        link: function (scope, element, attrs) {
            scope.inner_html = element.html();
        }
    }
}).directive('consent', function ($cookies) {
    return {
        scope: {},
        template:
          '<div style="position: relative; z-index: 1000">' +
          '<div style="background: steelblue; color: white; position: fixed; bottom: 0; left: 0; right: 0" ng-hide="consent()">' +
          'This website uses cookies to ensure you get the best experience on our website.' +
          ' <a href="" ng-click="consent(true)" style="color:white">Click here</a> to agreed to EU cookies law.' +
          '</div>' +
          '</div>',
        controller: function ($scope) {
            var _consent = $cookies.get('consent');
            $scope.consent = function (consent) {
                if (consent === undefined) {
                    return _consent;
                } else if (consent) {
                    $cookies.put('consent', true);
                    _consent = true;
                }
            };
        }
    };
});