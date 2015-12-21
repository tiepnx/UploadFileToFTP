'use strict'
var namespace = 'nift';
var nift_app_setting = {
    'elementStartApp': '#niftApp',
    'app_name': namespace,
    'partialUrl': '/_layouts/UploadFileToFTP/partials/',
    'partials':{
        header:  'header.html',
        start: 'start.html'
    },
    'services': {
        sharepointJsom: namespace + 'jsom',
        globalService: namespace + 'global',
        startService: namespace + '.start',
    },
    'controllers': {
        start: namespace + '.start'
    }
};
(function (define, angular) {
    window.angular.module(nift_app_setting.app_name, ['ui.router']);
})(window.define, window.angular);
