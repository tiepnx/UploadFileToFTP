(function (define, angular) {
    window.angular.module(nift_app_setting.app_name)
        .constant('toastr', toastr)        
        .config(configure)
        .config(exceptionConfig)
        .factory('logger', logger)
        .factory('exception', exception);
    exceptionConfig.$inject = ['$provide'];
    extendExceptionHandler.$inject = ['$delegate', 'toastr'];
    configure.$inject = ['toastr'];
    logger.$inject = ['$log', 'toastr'];
    exception.$inject = ['logger'];
    function exceptionConfig($provide) {
        $provide.decorator('$exceptionHandler', extendExceptionHandler);
    }
    function extendExceptionHandler($delegate, toastr) {
        return function (exception, cause) {
            $delegate(exception, cause);
            var errorData = {
                exception: exception,
                cause: cause
            };
            /**
             * Could add the error to a service's collection,
             * add errors to $rootScope, log errors to remote web server,
             * or log locally. Or throw hard. It is entirely up to you.
             * throw exception;
             */
            toastr.error(exception.message, errorData);
        };
    }
    function configure(toastr) {
        toastr.options.timeOut = 4000;
        toastr.options.positionClass = 'toast-bottom-right';
    };
    function logger($log, toastr) {
        var service = {
            showToasts: true,
            error: error,
            info: info,
            success: success,
            warning: warning,
            //strainght to console; bypass toastr
            log: $log.log
        }
        return service;

        function error(message, data, title) {
            toastr.error(message, title);
            $log.error('Error: ' + message, data);
        }
        function info(message) {
            toastr.info(message, title);
            $log.info('Info: ' + message);
        }
        function success(message, data, title) {
            toastr.success(message, title);
            $log.success('Success: ' + message);
        }
        function warning(message, data, title) {
            toastr.warning(message, title);
            $log.warning('warning: ' + message);
        }
    }
    function exception(logger) {
        var service = {
            catcher: catcher
        };
        return service;

        function catcher(message) {
            return function (reason) {
                logger.error(message, reason);
            };
        }
    }
})(window.define, window.angular);
