'use strict';
(function (define, angular) {
    var _settingctr = nift_app_setting;

    window.angular.module(_settingctr.app_name)
        .controller(_settingctr.controllers.start, startCtr);

    startCtr.$inject = ['$scope', '$state', '$timeout', '$q', 'logger', 'toastr', _settingctr.services.globalService, _settingctr.services.sharepointJsom];


    function startCtr($scope, $state, $timeout, $q, logger, toastr, globalService, sharepointJsom) {
        var vm = this;
        /*properties*/
        vm.data = {};
        vm.isReady = false;
        /*method*/
        vm.fnInit = fnInit;
        vm.fnLoadItem = fnLoadItem;
        vm.fnChoose = fnChoose;
        vm.fnChooseItems = fnChooseItems;
        vm.fnRemoveItems = fnRemoveItems;
        vm.fnCopyToFtp = fnCopyToFtp;
        vm.securityStore = $('input[type=hidden][id*="_hdSecureStoreID"]').val();
        vm.fnUpdateItem = fnUpdateItem;

        function fnInit() {
            globalService.showWaiting();
            sharepointJsom.getAllListProperties().then(function (results) {
                vm.data = $.extend(vm.data, { lists: results });
                console.log(results);
                globalService.closeWaiting();
            }, function (err) {
                console.log(err);
                globalService.closeWaiting();
            });
        }
        function fnLoadItem(list, node) {
            globalService.showWaiting();
            if (globalService.isUndefinedOrNull(list.hasFieldIsSend)) {
                var hasFieldIsSend = false;
                sharepointJsom.getFields(list.title).then(function (fields) {                    
                    var n = fields.length;
                    for (var i = 0; i < n; i++) {
                        if (fields[i].internalName == 'IsSend' && fields[i].fieldType === SP.FieldType.prototype.boolean)/*SP.FieldType.prototype*/ {
                            hasFieldIsSend = true;
                            break;
                        }
                    }
                    list = $.extend(list, { hasFieldIsSend: hasFieldIsSend });
                    console.log(hasFieldIsSend);
                    fnGetItemOfList(list, node);
                }, function (err) {
                    list = $.extend(list, { hasFieldIsSend: hasFieldIsSend });
                    fnGetItemOfList(list, node);
                });
            } else {
                fnGetItemOfList(list, node);
            }
        }
        function fnGetItemOfList(list, node) {
            var query = "<View><Query></Query></View>";//<View Scope='RecursiveAll'>
            //var include = 'Include(Title, FileRef, FileLeafRef, File, Folder, FileSystemObjectType)';
            var include = 'Include(ID,Title, FileRef, FileLeafRef, File, FileSystemObjectType)';
            if (globalService.isUndefinedOrNull(node)) {
                if (globalService.isUndefinedOrNull(vm.data.selectedList)) {
                    vm.data = $.extend(vm.data, { selectedList: null });
                }
                if (globalService.isUndefinedOrNull(vm.data.currentFolder)) {
                    vm.data = $.extend(vm.data, { currentFolder: null });
                }

                vm.data.selectedList = list;
                vm.data.currentFolder = vm.data.selectedList;
                if (globalService.isUndefinedOrNull(list.items)) {
                    sharepointJsom.getItemsWithCamlQuery(vm.data.selectedList.title, query, include).then(function (results) {
                        var listEnumerator = results.getEnumerator();
                        var arr = fnBindToArr(listEnumerator);
                        vm.data.selectedList = $.extend(vm.data.selectedList, { items: arr });
                        vm.data.currentFolder = vm.data.selectedList;
                        globalService.closeWaiting();
                    }, function (err) {
                        logger.error(err.statusText + ' (' + err + ')');/*.config.url*/
                        globalService.closeWaiting();
                    });
                } else
                    globalService.closeWaiting();
            } else {
                if (globalService.isUndefinedOrNull(node.items)) {
                    sharepointJsom.getItemsWithCamlQuery(vm.data.selectedList.title, query, include, node.serverRelativeUrl).then(function (results) {
                        var listEnumerator = results.getEnumerator();
                        var arr = fnBindToArr(listEnumerator);
                        vm.data = $.extend(vm.data, { currentFolder: null });
                        node = $.extend(node, { items: arr });
                        vm.data.currentFolder = node;
                        globalService.closeWaiting();
                    }, function (err) {
                        logger.error(err.statusText + ' (' + err + ')');/*.config.url*/
                        globalService.closeWaiting();
                    });
                } else {
                    vm.data.currentFolder = node;
                    globalService.closeWaiting();
                }
            }
        }
        function fnBindToArr(listEnumerator) {
            var arr = [];
            while (listEnumerator.moveNext()) {
                var item = listEnumerator.get_current();
                if (item.get_fileSystemObjectType() === SP.FileSystemObjectType.folder) {
                    arr.push({ listTitle: vm.data.selectedList.title, hasFieldIsSend: vm.data.selectedList.hasFieldIsSend, id: item.get_item("ID"), title: item.get_item('Title'), serverRelativeUrl: item.get_item('FileRef'), fileSystemObjectType: item.get_fileSystemObjectType() });
                } else if (item.get_fileSystemObjectType() === SP.FileSystemObjectType.file) {
                    arr.push({ listTitle: vm.data.selectedList.title, id: item.get_item("ID"),hasFieldIsSend: vm.data.selectedList.hasFieldIsSend, title: item.get_item('FileLeafRef'), serverRelativeUrl: item.get_file().get_serverRelativeUrl(), fileSystemObjectType: item.get_fileSystemObjectType() });
                }
            }
            return arr;
        }
        function fnUpdateItem(item) {

        }
        function fnChoose(item) {
            if (globalService.isUndefinedOrNull(vm.data.selectedItems)) {
                vm.data = $.extend(vm.data, { selectedItems: [] });
            }
            if (globalService.isUndefinedOrNull(item.isSelected)) {
                item = $.extend(item, { isSelected: false });
            }
            item.isSelected = true;
            item.isCheck = false;
            vm.data.selectedItems.push(item);
        }
        function fnChooseItems() {
            var items = vm.data.currentFolder.items;
            var n = items.length;
            for (var i = 0; i < n; i++) {
                if (items[i].isCheck) {
                    fnChoose(items[i]);
                }
            }
            return false;
        }
        function fnRemoveItem(item) {
            var items = vm.data.selectedItems;
            var n = items.length;
            for (var i = 0; i < n; i++) {
                if (items[i].serverRelativeUrl == item.serverRelativeUrl) {
                    item.isSelected = false;
                    item.isCheck = false;
                    item.isCheckRemove = false;
                    items.splice(i, 1);
                    break;
                }
            }
        }
        function fnRemoveItems() {
            var items = vm.data.selectedItems;
            var n = items.length;
            for (var i = 0; i < n; i++) {
                if (items[i].isCheckRemove) {
                    fnRemoveItem(items[i]);
                    n--;
                    i--;
                }
            }
        }
        function fnCopyFileToFtp(item) {
            var d = $q.defer();
            $.ajax({
                type: "GET",
                url: '/_vti_bin/T5Service/UploadService.svc/Upload',
                contentType: "application/json; charset=utf-8",
                data: { "secureID": vm.securityStore, "url": item.serverRelativeUrl },
                dataType: 'json',
                success: function (msg) {
                    d.resolve(msg);
                },
                error: function (err) {
                    d.reject(err);
                }
            });
            return d.promise;
        }
        function fnCopyToFtp() {
            globalService.showWaiting();
            var items = vm.data.selectedItems;
            var n = items.length;
            var arrfn = [];
            for (var i = 0; i < n; i++) {
                arrfn.push(fnCopyFileToFtp(items[i]));
            }
            $q.all(arrfn).then(function (checkIn_Results) {
                var n = checkIn_Results.length;
                for (var i = 0; i < n; i++) {
                    if (checkIn_Results[i].isSuccess == false)
                        toastr.error(checkIn_Results[i].Message);
                    else {
                        toastr.success(checkIn_Results[i].Message);
                    }
                }
                var arrUpdateItem = [];
                for (var i = 0; i < n; i++) {
                    if (items[i].hasFieldIsSend)
                        arrUpdateItem.push(sharepointJsom.updateItem(items[i].listTitle, items[i].id, [{ field: 'IsSend', value: true }]));//{field:'internalName', value: value want to update}
                }
                $q.all(arrUpdateItem).then(function (checkIn_Results) {
                    toastr.success("Update item success!");
                    globalService.closeWaiting();
                }, function (err) {
                    toastr.error(err);
                    globalService.closeWaiting();
                });
            }, function (checkIn_Err) {
                toastr.error(checkIn_Err);
                globalService.closeWaiting();
            });
        }
    }

})(window.define, window.angular);