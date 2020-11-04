Public Class OutputTo

    Public ToFunction As IFunction
    Public ToPin As Int32
    Public FromPin As Int32 = -1


    'nur zum laden/speichern nötig:
    Public ToFunctionId As Guid
    Public ToConnectorName As String

    Public Sub ZeroValue()
        ToFunction.SetParameterValue(ToPin, 0)
    End Sub

    Public Function InitAfterLoad(ByVal AllInstances As List(Of ConfigInstance), ByVal From As IFunction) As Boolean
        'aus function guid und connector name function instance und enum machen
        ToFunction = AllInstances.Find(Function(o2) (o2.[Function].Guid = ToFunctionId)).[Function]

        Dim tbl As DataTable = ToFunction.ParameterInfos
        Dim row As DataRow = tbl.AsEnumerable.FirstOrDefault(Function(row1) CStr(row1("Name")) = ToConnectorName)
        If row IsNot Nothing Then
            Dim iConnector As Int32 = CInt(row("Index"))
            ToPin = iConnector
            If From.Type <> FunctionType.Repeater Then
                ToFunction.RequiredInputCount += 1
            End If
            Return True
        End If
        Return False
    End Function

    Public Sub PrepareSave()
        'sicherstellen, dass connector name gesetzt ist und die folgeparameter ihre default werte haben
        Dim tbl As DataTable = ToFunction.ParameterInfos
        Dim row As DataRow = tbl.AsEnumerable.FirstOrDefault(Function(row1) CInt(row1("Index")) = ToPin)
        If row IsNot Nothing Then
            ToConnectorName = CStr(row("Name"))
            ToFunction.SetParameterValue(ToPin, CDbl(row("Default")))
        End If
    End Sub

End Class