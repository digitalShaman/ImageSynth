Public Class frmRepeaterValues

    Delegate Sub UpdateImageFunction()
    Private tbl As DataTable
    Private funcUpdateImage As UpdateImageFunction


    Public Sub Display(ByVal Rep As Repeater, Update As UpdateImageFunction)
        tbl = Rep.DataSequence
        funcUpdateImage = Update

        Rep.Outputs.OrderBy(Function(o) o.FromPin).Select(Function(o) o.ToConnectorName)

        For Each oOutput As OutputTo In Rep.Outputs
            tbl.Columns(oOutput.FromPin).ColumnName = oOutput.ToConnectorName
        Next

        dgvData.DataSource = tbl
        dgvData.AllowUserToOrderColumns = False
        Me.ShowDialog()
    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Me.Hide()
    End Sub


    Private Sub dgvData_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvData.CellValueChanged
        funcUpdateImage()
    End Sub
End Class