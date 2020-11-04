Imports System.Globalization

Public Class Clamp
    Inherits FunctionBase

    Private fMin As Double = 0
    Private fMax As Double = 1
    Private fFilteredSignal As Double = 0
    Private fIntensity As Double = 1
    Private fAdd As Double = 0

    Public Overrides Function Evaluate(x As Double, y As Double) As Double
        If Double.IsNaN(fFilteredSignal) Then
            fFilteredSignal = 0
        End If
        Dim fResult As Double
        If fFilteredSignal < fMin Then
            fResult = fMin
        ElseIf fFilteredSignal > fMax Then
            fResult = fMax
        Else
            fResult = fFilteredSignal
        End If
        Return (fResult + fAdd) * fIntensity
    End Function

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Clamp"
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

            tbl.Rows.Add(0, "Add", 0.1, 0, fAdd)
            tbl.Rows.Add(1, "Intensity", 0.1, 1, fIntensity)
            tbl.Rows.Add(2, "Minimum", 0.1, 0, fMin)
            tbl.Rows.Add(3, "Maximum", 0.1, 1, fMax)
            tbl.Rows.Add(4, "Signal", 0.1, 0, fFilteredSignal)
            Return tbl
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Liegt das Signal unter Minimum wird Minimum ausgegeben, liegt das Signal über Maximum, wird Maximum ausgegeben. Sonst Signal."
        End Get
    End Property


    Public Overrides Sub SetParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                fMin = Value
            Case 3
                fMax = Value
            Case 4
                fFilteredSignal = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(Index As Integer, Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                fMin += Value
            Case 3
                fMax += Value
            Case 4
                fFilteredSignal += Value
        End Select
    End Sub





    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateClamp(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float EvaluateClamp(int i, vec2 Location) {
    if (isnan(Storage[i + 4])) {
        Storage[i + 4] = 0.0;
    }
    float fResult = (clamp(Storage[i + 4], Storage[i + 2], Storage[i + 3]) + Storage[i + 0]) * Storage[i + 1];
    return fResult;
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fMin.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={fMax.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={fFilteredSignal.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateClamp({OGLStorageIndex}, textureCoordinate);"

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
