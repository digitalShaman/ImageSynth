Public Class frmEnterValue

    Public IsValid As Boolean = False
    Public Value As Double

    Private fDefault As Double

    Public Sub Display(ByVal Instance As ConfigInstance, ByVal ParameterIndex As Int32)
        Dim tbl As DataTable = Instance.[Function].ParameterInfos
        Dim row As DataRow = tbl.Rows(ParameterIndex)
        lblDesc.Text = "Neuer Wert für " & CStr(row("Name")) & ":"
        Value = CDbl(row("Current"))
        fDefault = CDbl(row("Default"))
        If CDbl(row("Step")) = 1 Then
            txtValue.Text = Value.ToString("N0")
        Else
            txtValue.Text = Value.ToString("N8")
        End If
        IsValid = False
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Not Double.TryParse(txtValue.Text, Value) Then
            MessageBox.Show("Kein gültiger Wert!")
            Exit Sub
        End If
        IsValid = True
        Me.Hide()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Value = fDefault
        IsValid = True
        Me.Hide()
    End Sub
End Class