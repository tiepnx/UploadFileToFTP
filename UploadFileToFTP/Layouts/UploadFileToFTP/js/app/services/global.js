'use strict';
(function (define, angular) {
    var _settingservice = nift_app_setting;
    _settingservice = $.extend(_settingservice, { 'constantsrv': nift_app_setting.app_name + '.service.constant' });
    window.angular.module(_settingservice.app_name)
        .factory(_settingservice.services.globalService, globalServiceFn);
    function globalServiceFn() {
        var waitDialog = null;
        return {
            showWaiting: showWaiting,
            closeWaiting: closeWaiting,
            isUndefinedOrNull: isUndefinedOrNull
        }
        function showWaiting() {
            waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose('Working on it...', '');
        }
        function closeWaiting() {
            waitDialog.close();
            waitDialog = null;
        }
        function isUndefinedOrNull(val) {
            return angular.isUndefined(val) || val === null;
        }
    }
})(window.define, window.angular);