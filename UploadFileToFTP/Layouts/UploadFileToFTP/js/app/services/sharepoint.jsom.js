'use strict';
(function (define, angular) {
    var _settingservice = nift_app_setting;
    window.angular.module(_settingservice.app_name)
		.factory(_settingservice.services.sharepointJsom, sharepointJsomFn);
    sharepointJsomFn.$inject = ['$q', '$http', '$timeout', _settingservice.services.globalService];
    function sharepointJsomFn($q, $http, $timeout, global) {
        /*init properties*/
        var defaults = {};
        defaults = $.extend(defaults, {
            siteRelativeUrl: _spPageContextInfo.webAbsoluteUrl,
            clientContext: SP.ClientContext.get_current(),
        });
        defaults["oWebsite"] = defaults.clientContext.get_web();
        defaults["lists"] = defaults.oWebsite.get_lists();
        /****************************************************/
        return {
            getAllListProperties: fnGetAllListProperties,
            getFields: fnGetFields,
            getItems: fnGetItems,
            getItemsWithCamlQuery: fnGetItemsWithCamlQuery,
            //createItem: fnCreateItem,
            updateItem: fnUpdateItem,
            deleteItem: fnDeleteItem
        }

        function fnGetAllListProperties() {
            var dfd = $q.defer();
            var lists = defaults.oWebsite.get_lists();
            var ctx = defaults.clientContext;
            ctx.load(lists);
            ctx.executeQueryAsync(
                function () {
                    var arr = [];
                    var listEnumerator = lists.getEnumerator();
                    /*
                    http://joelblogs.co.uk/2011/06/16/sharepoint-2010-base-types-list-template-and-definition-ids-and-content-types-ids/
                    http://sharepoint.stackexchange.com/questions/90418/how-to-check-if-listtemplatetype-enum-is-list-or-library
                    */
                    while (listEnumerator.moveNext()) {
                        var oList = listEnumerator.get_current();
                        if (SP.ListTemplateType.documentLibrary === oList.get_baseTemplate() && oList.get_hidden() === false)
                            arr.push({ 'title': oList.get_title(), 'BaseType': oList.get_baseType() });
                    }
                    dfd.resolve(arr);
                }, function (sender, args) {
                    dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
                }
            );
            return dfd.promise;
        }
        function fnGetItems(listTitle) {
            var dfd = $q.defer();
            var ctx = SP.ClientContext.get_current();
            var list = ctx.get_web().get_lists().getByTitle(listTitle);
            var items = list.getItems('');
           
            ctx.load(items);
            ctx.executeQueryAsync(function () {
                var arr = [];
                var listEnumerator = items.getEnumerator();
                while (listEnumerator.moveNext()) {
                    var item = listEnumerator.get_current();
                    arr.push(item.get_fieldValues());
                }
                dfd.resolve(arr);
            }, function (sender, args) {
                dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
            });
            return dfd.promise;
        }
        function fnGetFields(listTitle) {
            var dfd = $q.defer();
            var ctx = defaults.clientContext;
            var fields = ctx.loadQuery(ctx.get_web()
				.get_lists()
				.getByTitle(listTitle)
				.get_fields());

            ctx.executeQueryAsync(function () {
                var arrObj = [];
                fields.forEach(function (field, index) {                   
                    arrObj.push({ internalName: field.get_internalName(), title: field.get_title(), hidden: field.get_hidden(), fieldType: field.get_fieldTypeKind() });                    
                })
                dfd.resolve(arrObj);
            }, function (sender, args) {
                dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
            });
            return dfd.promise;
        };
        /*
        example:
            query: 
                '<View>
                    <Query>
                        <Where>
                            <Geq>
                                <FieldRef Name=\'ID\'/>' + 
                                '<Value Type=\'Number\'>1</Value>
                            </Geq>
                        </Where>
                    </Query>' + 
                    '<RowLimit>10</RowLimit>
                </View>'
            include: Include(File, FileSystemObjectType)
        */
        function fnGetItemsWithCamlQuery(listTitle, query, include, folderServerRelativeUrl) {
            var dfd = $q.defer();
            var list = defaults.lists.getByTitle(listTitle);
            var ctx = defaults.clientContext;
            var items = null;
            var camlQuery = null;
            if (!global.isUndefinedOrNull(folderServerRelativeUrl)) {
                camlQuery = new SP.CamlQuery();
                camlQuery.set_folderServerRelativeUrl(folderServerRelativeUrl);
            }
            if (!global.isUndefinedOrNull(query)) {
                if (global.isUndefinedOrNull(camlQuery))
                    camlQuery = new SP.CamlQuery();
                camlQuery.set_viewXml(query);
            }
            if (global.isUndefinedOrNull(camlQuery))
                items = list.getItems('');
            else
                items = list.getItems(camlQuery);

            if (global.isUndefinedOrNull(include))
                ctx.load(items);
            else
                ctx.load(items, include);

            ctx.executeQueryAsync(
                function () {
                    dfd.resolve(items);
                },
                function (sender, args) { dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace()); }
            );
            return dfd.promise;
        }
        /*
            each obj in arrObj: {field:'internalName', value: value want to update}
        */
        //function fnUpdateItem(listTitle, id, arrObj) {
        //    var dfd = $q.defer();
        //    //var id = 13;
        //    var ctx = SP.ClientContext.get_current();
        //    var item = ctx.get_web().get_lists().getByTitle(listTitle).getItemById(id);
        //    angular.forEach(arrObj, function (obj, key) {
        //        item.set_item(obj.field, obj.value);
        //    });
        //    //item.set_item("Title", "Amimal");
        //    //item.set_item("FirstName", "News Topic");
        //    item.update();
        //    ctx.executeQueryAsync(function () {
        //        dfd.resolve(item);
        //    }, function (sender, args) {
        //        dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
        //    });
        //    return dfd.promise;
        //}
        function fnUpdateItem(listName, id, arrObj) {
            var dfd = $q.defer();
            var ctx = new SP.ClientContext.get_current();
            var list = ctx
                        .get_web()
                        .get_lists()
                        .getByTitle(listName);
            var item = list.getItemById(id);
            ctx.load(item)
            ctx.executeQueryAsync(
                Function.createDelegate(this, function (data, args) {
                    updateListitemEventAfterData(item, arrObj).then(function (data) {
                        dfd.resolve(data);
                    }, function (err) {
                        dfd.reject(err);
                    });
                }),
                Function.createDelegate(this, function (sender, args) {
                    dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
                }));
            return dfd.promise;
        }
        function updateListitemEventAfterData(item, arrObj) {
            var dfd = $q.defer();
            var ctx = new SP.ClientContext.get_current();
            angular.forEach(arrObj, function (obj, key) {
                item.set_item(obj.field, obj.value);
            });
            item.update();
            ctx.executeQueryAsync(
                Function.createDelegate(this, function (data, args) {
                    dfd.resolve(data);
                }),
                Function.createDelegate(this, function (sender, args) {
                    dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
                }));
            return dfd.promise;
        }
        function fnCreateItem(listTitle) {
            var dfd = $q.defer();
            var list = defaults.lists.getByTitle(listTitle);
            var ctx = defaults.clientContext;
            var itemCreateInfo = new SP.ListItemCreationInformation();
            var contact = list.addItem(itemCreateInfo);
            contact.set_item("Title", "Peel");
            contact.set_item("FirstName", "Emma");
            contact.update();
            ctx.load(contact);
            ctx.executeQueryAsync(function () {
                dfd.resolve(contact.get_id());
            }, function (sender, args) {
                dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
            });
            return dfd.promise;
        }
        
        function fnDeleteItem(listTitle) {
            var dfd = $q.defer();
            var id = 13;
            var item = defaults.lists.getByTitle(listTitle).getItemById(id);
            var ctx = defaults.clientContext;
            item.deleteObject();
            ctx.load(item);
            ctx.executeQueryAsync(function () {
                dfd.resolve(null);
            }, function (sender, args) {
                dfd.reject('Request failed. ' + args.get_message() + '\n' + args.get_stackTrace());
            });
            return dfd.promise;
        }
    }
})(window.define, window.angular);