<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DevExpressTest3.QueryViewer"%>

<%@ Register Assembly="DevExpress.Web.Bootstrap.v16.2, Version=16.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>New Team Query Viewer</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:SqlDataSource ID="GridViewSqlDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:Tesis2002ConnectionString %>" ProviderName="System.Data.SqlClient"></asp:SqlDataSource>
        <asp:SqlDataSource ID="QueryListDataSource" runat="server"  SelectCommand="SELECT Nome FROM Analytics.QuerySalvate" ConnectionString="<%$ ConnectionStrings:Tesis2002ConnectionString %>" ProviderName="System.Data.SqlClient"></asp:SqlDataSource>
        <dx:ASPxGridViewExporter ID="NewTeamGridViewExporter" runat="server" GridViewID="NewTeamGridView"></dx:ASPxGridViewExporter>
        <table runat="server">
            <tr>
                <td>
                    <div>
                        <dx:ASPxButton ID="QueryBuilderButton" runat="server" OnClick="QueryBuilderButton_Click" Text="Go to the Query Builder">
                            </dx:ASPxButton>
                            <dx:ASPxRadioButtonList ID="RadioButtonQueryList" runat="server" OnValueChanged="RadioButtonQueryList_ValueChanged" DataSourceID="QueryListDataSource" TextField="Nome" AutoPostBack="true"></dx:ASPxRadioButtonList>
                    </div>
                </td>
                <td>
                    <div>
                    </div>
                    <div style="text-align:right">
                        <asp:ImageButton ID="PdfLogoImageButton" runat="server" ImageUrl="~/Images/pdf.png" Visible="false"  Height="100px" Width="100px" OnClick="FormatLogo_Click" DescriptionUrl="pdf" ImageAlign="Right"/>
                        <asp:ImageButton ID="XlsxLogoImageButton" runat="server" ImageUrl="~/Images/excel.png" Visible="false" Height="100px" Width="100px" OnClick="FormatLogo_Click" DescriptionUrl="xlsx"  ImageAlign="Right"/>
                        <br/><br/><br/><br/><br/><br/>
                        <dx:ASPxLabel ID="FileNameLabel" runat="server" Text="Type destination file name(without extension)" Visible="false"></dx:ASPxLabel>
                        <asp:TextBox ID="FileNameTextBox" runat="server" Visible="false" BorderWidth="1px" Width="300px"></asp:TextBox>
                    </div>
                </td>
            </tr>
        </table>
        <dx:ASPxLabel ID="ResultQueryLabel" runat="server" Visible="false"></dx:ASPxLabel>          
        <asp:Table ID="ParamTable" Visible="false" runat="server" EnableViewState="true">
        </asp:Table>
        <dx:ASPxButton ID="ValidateParametersButton" runat="server" Text="Validate Parameters" Visible="False" AutoPostBack="False" OnClick="ValidateParametersButton_Click">
        </dx:ASPxButton>
        <dx:ASPxGridView ID="NewTeamGridView" SettingsFilterControl-ShowAllDataSourceColumns="true" ViewStateMode="Disabled" EnableViewState="false" runat="server" AutoGenerateColumns="false"  OnDataBinding="NewTeamGridView_DataBinding" Visible="false">
            <Settings VerticalScrollBarMode="Auto" HorizontalScrollBarMode="Auto" ShowColumnHeaders="true" ShowFilterBar="Visible" ShowFilterRow="true"/>
            <SettingsBehavior AllowSort="true" SortMode="Value"/>
            <SettingsDataSecurity AllowDelete="False" AllowEdit="False" AllowInsert="False" />
            <SettingsSearchPanel Visible="True"/>
        </dx:ASPxGridView>

    </div>
       
    </form>
    
</body>
</html>
