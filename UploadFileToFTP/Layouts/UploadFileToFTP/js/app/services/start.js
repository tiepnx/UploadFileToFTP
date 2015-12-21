'use strict';
(function (define, angular) {
    var _settingservice = nift_app_setting;
    _settingservice = $.extend(_settingservice, { 'constantstartsrv': nift_app_setting.app_name + '.service.constant' });
    window.angular.module(_settingservice.name)
        .constant(_settingservice.constantstartsrv, {
            start: 'start',
        })
        .factory(_settingservice.services.startService, startServiceFn);
   
    startServiceFn.$inject = ['$q', _settingservice.services.sharepointJsom, _settingservice.constantstartsrv];
   
    function startServiceFn($http, $q, sharepointJsom, start_constant) {
        return {
            getYear: getYear,
            getAffiliates: getAffiliates,
            createSurvey: createSurvey
        }
        function getYear() {
            return httpService.getData(constant.GetActivedYears);
        }

        function getAffiliates(yearID) {
            return httpService.getData(constant.getAffiliates + '/' + yearID);
        }

        function createSurvey(affID, yearID) {
            var data = { affID: affID, yearID: yearID };
            return httpService.postData(constant.createSurvey, data);
        }
    }
   
})(window.define, window.angular);