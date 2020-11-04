Imports System.Globalization

Public Class ImageOutput
    Inherits FunctionBase


    Private fRed As Double
    Private fGreen As Double
    Private fBlue As Double

    Dim iRed, iGreen, iBlue As Int32
    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        If Double.IsNaN(fRed) Then
            iRed = 0
        ElseIf fRed < 0 Then
            iRed = 0
        ElseIf fRed > 1 Then
            iRed = &HFF0000
        Else
            iRed = CInt(fRed * 255) << 16
        End If

        If Double.IsNaN(fGreen) Then
            iGreen = 0
        ElseIf fGreen < 0 Then
            iGreen = 0
        ElseIf fGreen > 1 Then
            iGreen = &HFF00
        Else
            iGreen = CInt(fGreen * 255) << 8
        End If

        If Double.IsNaN(fBlue) Then
            iBlue = 0
        ElseIf fBlue < 0 Then
            iBlue = 0
        ElseIf fBlue > 1 Then
            iBlue = &HFF
        Else
            iBlue = CInt(fBlue * 255)
        End If

        Return iRed + iGreen + iBlue
    End Function

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Image"
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return ""
        End Get
    End Property

    Public Overrides ReadOnly Property Type As FunctionType
        Get
            Return FunctionType.ImageOutput
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
            tbl.Rows.Add(0, "Red", 0.2, 0, fRed)
            tbl.Rows.Add(1, "Green", 0.2, 0, fGreen)
            tbl.Rows.Add(2, "Blue", 0.2, 0, fBlue)
            Return tbl
        End Get
    End Property



    Public Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fRed = Value
            Case 1
                fGreen = Value
            Case 2
                fBlue = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fRed += Value
            Case 1
                fGreen += Value
            Case 2
                fBlue += Value
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
        sb.Append($"Storage[{OGLStorageIndex + 0}] = 0.0;")
        sb.Append($"Storage[{OGLStorageIndex + 1}] = 0.0;")
        sb.Append($"Storage[{OGLStorageIndex + 2}] = 0.0;")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = "fragColor"
        Dim sExecuteCode As String = $"{sTempVarName} = vec4 (Storage[{OGLStorageIndex + 0}],Storage[{OGLStorageIndex + 1}],Storage[{OGLStorageIndex + 2}], 1.0);"

        Return sExecuteCode
    End Function



End Class
