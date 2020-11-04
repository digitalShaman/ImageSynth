Public Class Repeater
    Inherits FunctionBase

    Public Overrides Sub AddParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Throw New NotImplementedException
    End Sub

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Throw New NotImplementedException
    End Function

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Wiederholt die Berechnungen der angeschlossenen Module wobei jedes Mal neue Parameter übergeben werden."
        End Get
    End Property

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Repeater"
        End Get
    End Property

    Public Overrides ReadOnly Property ParameterInfos As System.Data.DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Parameter 0", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 1", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 2", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 3", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 4", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 5", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 6", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 7", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 8", 0.1, 0, 0)
            tbl.Rows.Add(0, "Parameter 9", 0.1, 0, 0)
            Return tbl
        End Get
    End Property

    Public Overloads Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Throw New NotImplementedException
    End Sub

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Repeater
        End Get
    End Property

    Public DataSequence As New DataTable

    Public Sub New()
        NewTable()
    End Sub

    Private Sub NewTable()
        DataSequence = New DataTable With {.TableName = "DataSequence"}
        DataSequence.Columns.Add("Parameter 0 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 1 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 2 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 3 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 4 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 5 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 6 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 7 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 8 Value", GetType(Double))
        DataSequence.Columns.Add("Parameter 9 Value", GetType(Double))
    End Sub

    Public Function SerializeTable() As String
        Dim sb As New System.Text.StringBuilder
        For Each row As DataRow In DataSequence.Rows
            For i As Int32 = 0 To 9
                Dim sValue As String = ""
                If row(i).Equals(DBNull.Value) Then
                    sValue = ""
                Else
                    sValue = CDbl(row(i)).ToString(Globalization.CultureInfo.InvariantCulture)
                End If
                sb.AppendFormat("{0}{1}", sValue, Convert.ToChar(9))
            Next
            sb.Append(Environment.NewLine)
        Next
        Return sb.ToString
    End Function

    Public Sub DeserializeTable(ByVal Data As String)
        Dim sRows() As String = Data.Split({Environment.NewLine}, StringSplitOptions.None)
        NewTable()
        For Each sRow As String In sRows
            If sRow <> "" Then
                Dim sParts() As String = sRow.Split(Convert.ToChar(9))
                Dim row As DataRow = DataSequence.NewRow
                DataSequence.Rows.Add(row)
                For i As Int32 = 0 To 9
                    If sParts(i) <> "" Then
                        Dim fValue As Double = Double.Parse(sParts(i), Globalization.CultureInfo.InvariantCulture)
                        row(i) = fValue
                    End If
                Next
            End If
        Next
    End Sub
End Class
