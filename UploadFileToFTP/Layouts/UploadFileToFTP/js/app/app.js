'use strict';
(function (define, angular) {
    window.angular.element(document).ready(function () {
        ExecuteOrDelayUntilScriptLoaded(startApp, "sp.js");
        function startApp() {
            window.angular.bootstrap(nift_app_setting.elementStartApp, [nift_app_setting.app_name]);
        }
    });
})(window.define, window.angular);