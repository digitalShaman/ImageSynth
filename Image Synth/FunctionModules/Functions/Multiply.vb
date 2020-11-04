Imports System.Globalization

Public Class Multiply
    Inherits FunctionBase

    Private fFactor1 As Double = 1
    Private fFactor2 As Double = 1
    Private fFactor3 As Double = 1
    Private fFactor4 As Double = 1

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Return fFactor1 * fFactor2 * fFactor3 * fFactor4
    End Function

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Multiply"
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Gibt Factor 1 * Factor 2 usw. aus."
        End Get
    End Property


    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.Function
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
            tbl.Rows.Add(0, "Factor 1", 0.1, 1, fFactor1)
            tbl.Rows.Add(1, "Factor 2", 0.1, 1, fFactor2)
            tbl.Rows.Add(2, "Factor 3", 0.1, 1, fFactor3)
            tbl.Rows.Add(3, "Factor 4", 0.1, 1, fFactor4)
            Return tbl

        End Get
    End Property



    Public Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fFactor1 = Value
            Case 1
                fFactor2 = Value
            Case 2
                fFactor3 = Value
            Case 3
                fFactor4 = Value
            Case Else
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fFactor1 += Value
            Case 1
                fFactor2 += Value
            Case 2
                fFactor3 += Value
            Case 3
                fFactor4 += Value
        End Select
    End Sub






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
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fFactor1.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fFactor2.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fFactor3.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={fFactor4.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function


    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName} = Storage[{OGLStorageIndex + 0}] * Storage[{OGLStorageIndex + 1}] * Storage[{OGLStorageIndex + 2}] * Storage[{OGLStorageIndex + 3}];"

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
