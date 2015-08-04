<%@ Page Title="" Language="VB" MasterPageFile="~/MasterPage.master" AutoEventWireup="false" Inherits="ConfigEditor" EnableViewStateMac="false" EnableViewState="false" ViewStateMode="Disabled" ValidateRequest="False" MaintainScrollPositionOnPostback="true" CodeFile="ConfigEditor.aspx.vb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
	<asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
	<asp:HiddenField ID="KeyObject" runat="server" />
	<br />
	<asp:Button ID="SaveButton" runat="server" Text="#405" />
</asp:Content>