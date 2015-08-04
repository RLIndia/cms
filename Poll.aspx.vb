Imports WebApplication
Partial Class Poll
  Inherits System.Web.UI.Page

  Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
    If Request.UrlReferrer IsNot Nothing Then
      Dim Context As PollManager.PollContext = CType(ValInt(Request.QueryString("c")), PollContext)
      Dim Vote As Integer = ValInt(Request.QueryString("v"))
      PollManager.Vote(Context, Vote, Request.QueryString("1"), Request.QueryString("2"), Request.QueryString("3"))
      Response.Redirect(Request.UrlReferrer.AbsoluteUri)
    End If
  End Sub
End Class

