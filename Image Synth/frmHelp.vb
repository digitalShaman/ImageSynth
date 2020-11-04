Public Class frmHelp


    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Me.Hide()
    End Sub

    Private Sub frmHelp_Load(sender As Object, e As EventArgs) Handles Me.Load
        txtHelp.Text = My.Resources.Help
    End Sub
End Class