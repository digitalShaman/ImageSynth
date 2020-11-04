Imports System.Globalization

Public Class Add
    Inherits FunctionBase

    Private fValue1 As Double
    Private fValue2 As Double

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Add"
        End Get
    End Property

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Function
        End Get
    End Property

    Public Overrides ReadOnly Property ParameterInfos As DataTable
        Get
            Dim tbl As New DataTable
            tbl.Columns.Add("Index", GetType(Int32))
            tbl.Columns.Add("Name", GetType(String))
            tbl.Columns.Add("Step", GetType(Double))
            tbl.Columns.Add("Default", GetType(Double))
            tbl.Columns.Add("Current", GetType(Double))
            tbl.Rows.Add(0, "Value", 0.1, 0, fValue1)
            tbl.Rows.Add(1, "Input", 0.1, 0, fValue2)
            Return tbl
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Addiert einen Wert zum Signal."
        End Get
    End Property

    Public Overrides Sub SetParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fValue1 = Value
            Case 1
                fValue2 = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fValue1 += Value
            Case 1
                fValue2 += Value
        End Select
    End Sub

    Public Overrides Function Evaluate(x As Double, y As Double) As Double
        Return fValue1 + fValue2
    End Function





    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        DeclareCode &= GetDeclareCode()
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return ""
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fValue1.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fValue2.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function


    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName} = Storage[{OGLStorageIndex + 0}] + Storage[{OGLStorageIndex + 1}];"

        'Ergebnis an folge funktionen weitergeben
        For Each oChild As OutputTo In Outputs
            If Not (AnySolo AndAlso oChild.ToFunction.Type = FunctionType.ImageOutput) AndAlso oChild.ToFunction.OGLStorageIndex <> -1 Then
                'aber nur, wenn kein solo oder wenn solo dann aber nicht an imageoutput
                Dim oFunc As FunctionBase = CType(oChild.ToFunction, FunctionBase)
                sExecuteCode &= oFunc.GetOpenGlAddCode(oChild.ToPin, sTempVarName)
            End If
        Next

        Return sExecuteCode
    End Function


End Class
