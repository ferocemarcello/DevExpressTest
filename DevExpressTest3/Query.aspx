﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Query.aspx.cs" Inherits="DevExpressTest3.Query"%>

<%@ Register assembly="DevExpress.XtraReports.v16.2.Web, Version=16.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.XtraReports.Web" tagprefix="dx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content -Type" content="text/html; charset=utf-8"/>
    <title>New Team Query Builder</title>
    <style type="text/css">
        #test {
            width: 59px;
        }
    </style>
</head>
<body>

    <form id="form1" runat="server">
        <script type="text/javascript">
            <%-- funzione che a partire dal parametro selectedFields, sul queryBuilder, aggiorna i checkable box per i campi parametrici e li setta checked o non checked--%>
            function InitFilterFields(queryBuilder, selectedFields) {
                var tables = queryBuilder.designerModel.model().tables();
                for (var tableIndex = 0; tableIndex < tables.length; tableIndex++) {
                    var columns = tables[tableIndex]._columns();
                    for (var columnIndex = 0; columnIndex < columns.length; columnIndex++) {
                        if (selectedFields.indexOf(tables[tableIndex].name()+"."+columns[columnIndex].name()) > -1)
                            columns[columnIndex].isFilter(true);
                    }
                }
            }
            <%-- funzione che ritorna una stringa dei campi parametrizzati, dove ogni campo parametrizzato è separato da un altro da un ';'
            ogni cella è costituita da [nomeschema.]nometabella/nomevista.nomecolonna--%>
            function GetFilterFields(queryBuilder) {
                var fields = new Array();
                var tables = queryBuilder.designerModel.model().tables();
                for (var tableIndex = 0; tableIndex < tables.length; tableIndex++) {
                    var columns = tables[tableIndex]._columns();
                    for (var columnIndex = 0; columnIndex < columns.length; columnIndex++) {
                        if (columns[columnIndex].isFilter()) {
                            var tab = tables[tableIndex].name();
                            var col = columns[columnIndex].name();
                            fields.push(tab+"."+col);
                        } 
                    }
                }
                return fields.join(";");
            }
            
            var hfFilterFieldsClientID = "<%= hfFilterFields.ClientID %>";
            var hfselectedFieldsID = "<%= hfselectedFields.ClientID %>";
            <%--viene chiamata al momento del salvataggio e salva quali sono i campi parametrizzati in hfFilterFields--%>
            function QueryBuilder_BeginCallback(s, e) {
                document.getElementById(hfFilterFieldsClientID).value = GetFilterFields(s);
            }

            function QueryBuilder_Init(s) {
                <%--fa un po di spazio per il campo descrizione--%>
                    var tablesTop = s.designerModel.tabPanel.tabs[0].model.tablesTop;
                    tablesTop(tablesTop() + 25);
                <%--s.designerModel.model().description().text = document.getElementById(hfDescriptionClientID).value;--%>
                <%--assegna al campo selectedfields il valore di hfselectedfields, ovvero i campi parametrizzati presenti nel db.
            successivamente, quando la query ha finito di caricare, tramite la funzione InitFilterfields, checka i checkboxappropriati per i parametri--%>
                var selectedFields = document.getElementById(hfselectedFieldsID).defaultValue;
                s.designerModel.isLoading.subscribe(function (isLoading) {
                    if (!isLoading) {
                        InitFilterFields(s, selectedFields.split(';'));
                    }
                });
            }
            </script>

        <dx:ASPxQueryBuilder ID="NewTeamQueryBuilder" runat="server" OnSaveQuery="NewTeamQueryBuilder_SaveQuery">
            <ClientSideEvents Init="QueryBuilder_Init" BeginCallback="QueryBuilder_BeginCallback"/>
        </dx:ASPxQueryBuilder>
        <script>
                //Custom editor for Description field
                var DescriptionEditor = (function (_super) {
                    __extends(DescriptionEditor, _super);
                    function DescriptionEditor(info, level, parentDisabled) {
                        var _this = this;
                        _super.call(this, info, level, parentDisabled);
                        this.popupVisible = ko.observable(false);
                        this.textAreaValue = ko.observable("");
                        this.buttonItems = [
                            {
                                toolbar: 'bottom',
                                location: 'after',
                                widget: 'dxButton',
                                options: {
                                    text: 'OK',
                                    onClick: function () {
                                        _this.save();
                                    }
                                }
                            },
                            {
                                toolbar: 'bottom',
                                location: 'after',
                                widget: 'dxButton',
                                options: {
                                    text: 'Cancel',
                                    onClick: function () {
                                        _this.hidePopup();
                                    }
                                }
                            }
                        ];
                    }
                    //metodi per gestire la descrizione, tramite hfDescription
                    DescriptionEditor.prototype.showPopup = function () {
                        this.textAreaValue(hfDescription.value);
                        this.popupVisible(true);
                    };
                    DescriptionEditor.prototype.hidePopup = function () {
                        this.popupVisible(false);
                    };
                    DescriptionEditor.prototype.save = function () {
                        hfDescription.value = this.textAreaValue();
                        this.hidePopup();
                    };
                    return DescriptionEditor;
                })(DevExpress.JS.Widgets.Editor);
                var descriptionSerializationsInfo = {
                    modelName: "@Description",
                    propertyName: "description",
                    displayName: "Description",
                    defaultVal: "",
                    editor: { custom: "dxqb-property-editor", header: "dx-custom-description-editor", editorType: DescriptionEditor },
                    from: function () { return ko.observable("expand it"); },
                    toJsonObject: function () { return ""; }
                };
                DevExpress.Designer.QueryBuilder.querySerializationsInfo.push(descriptionSerializationsInfo);

                DevExpress.Designer.QueryBuilder.columnSerializationInfo.push({
                    modelName: "@IsFilter",
                    propertyName: "isFilter",
                    displayName: "Parameter",
                    defaultVal: false,
                    editor: { custom: "dxqb-property-editor", header: "dx-boolean" },
                    from: function () { return ko.observable(""); },
                    toJsonObject: function () { return ""; }
                });
            </script>
            <!-- template for description editor -->
            <script type="text/html" id="dx-custom-description-editor">
                <div>
                    <div class="no-margin-right" data-bind="dxEllipsisEditor: { value: value, disabled: disabled, buttonAction: function () { showPopup(); } }, attr: { title: value }"></div>
                    <div class="dx-expressioneditor dx-popup-general" data-bind="dxPopup: {
                        showTitle: true,
                        title: 'Description',
                        visible: popupVisible,
                        toolbarItems: buttonItems,
                        fullScreen: false,
                        height: '95%',
                        width: '95%',
                        container: $($element).closest('.dx-viewport'),
                        position: { of: $($element).closest('.dx-viewport') }
                    }">
                        <div style="height: 100%" data-bind="dxTextArea: { value: textAreaValue }"></div>
                </div>
            </div>
            </script>
        <asp:HiddenField ID="hfDescription" runat="server"/> 
        <asp:HiddenField ID="hfFilterFields" runat="server" />
        <asp:HiddenField ID="hfselectedFields" runat="server" />    
    </form>
</body>
</html>
