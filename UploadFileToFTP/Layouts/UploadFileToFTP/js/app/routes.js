'use strict';
(function (define, angular) {
    var _setting = nift_app_setting;
    _setting = $.extend(_setting, { routes: _setting.app_name + '.routes' });
    var version = "01";
    window.angular.module(_setting.app_name)
        .constant(_setting.routes, {
            start: 'start',
        })
        .config(routesconfig);
    routesconfig.$inject = ['$stateProvider', '$urlRouterProvider', _setting.routes];
    function routesconfig($stateProvider, $urlRouterProvider, routes) {
        $stateProvider
            .state(routes.start, {
                url: '/' + routes.start,
                views: {
                    //'header': {
                    //    templateUrl: _setting.partialUrl + _setting.partials.header
                    //},
                    'content': {
                        templateUrl: _setting.partialUrl + _setting.partials.start + "?" + version,
                        controller: _setting.controllers.start + ' as vm'
                    }
                }
            });
            
        $urlRouterProvider.otherwise('/' + routes.start);
    }
})(window.define, window.angular);