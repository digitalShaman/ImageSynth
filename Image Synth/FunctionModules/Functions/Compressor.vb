Imports System.Globalization

Public Class Compressor
    Inherits FunctionBase

    Private fAdd As Double = 0
    Private fIntensity As Double = 1

    Private fLevel1 As Double = 0.2
    Private fFactor1 As Double = 0
    Private fLevel2 As Double = 0.5
    Private fFactor2 As Double = 1
    Private fLevel3 As Double = 1
    Private fFactor3 As Double = 0.8
    Private fSignal As Double = 0

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Compressor"
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

            tbl.Rows.Add(2, "Level 1", 0.1, 0.2, fLevel1)
            tbl.Rows.Add(3, "Factor 1", 0.1, 0, fFactor1)
            tbl.Rows.Add(4, "Level 2", 0.1, 0.5, fLevel2)
            tbl.Rows.Add(5, "Factor 2", 0.1, 1, fFactor2)
            tbl.Rows.Add(6, "Level 3", 0.1, 1, fLevel3)
            tbl.Rows.Add(7, "Factor 3", 0.1, 0.8, fFactor3)
            tbl.Rows.Add(8, "Signal", 0.1, 0, fSignal)
            Return tbl
        End Get
    End Property

    Public Overrides ReadOnly Property Help As String
        Get
            Return "Liegt der Absolutwert des Signals unter Level 1, wird er linear ansteigend mit einem Wert von 0 bis Factor 1 multipliziert wobei Factor 1 " &
                   "bei einem Wert gleich Level 1 zur Anwendung kommt. Liegt der Signalwert über Level 1 und unter Level 2, wird er ansteigend mit einem Wert " &
                   "von Factor 1 bis Factor 2 multipliziert. Liegt der Signalwert über Level 2 und unter Level 3, wird er ansteigend mit einem Wert von Factor 2 " &
                   "bis Factor 3 multipliziert. Liegt der Signalwert über Level 3 wird er mit Factor 3 multipliziert."
        End Get
    End Property

    Public Overrides Sub SetParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd = Value
            Case 1
                fIntensity = Value
            Case 2
                fLevel1 = Value
            Case 3
                fFactor1 = Value
            Case 4
                fLevel2 = Value
            Case 5
                fFactor2 = Value
            Case 6
                fLevel3 = Value
            Case 7
                fFactor3 = Value
            Case 8
                fSignal = Value
        End Select
    End Sub

    Public Overrides Sub AddParameterValue(ByVal Index As Integer, ByVal Value As Double)
        Select Case Index
            Case 0
                fAdd += Value
            Case 1
                fIntensity += Value
            Case 2
                fLevel1 += Value
            Case 3
                fFactor1 += Value
            Case 4
                fLevel2 += Value
            Case 5
                fFactor2 += Value
            Case 6
                fLevel3 += Value
            Case 7
                fFactor3 += Value
            Case 8
                fSignal += Value
        End Select
    End Sub

    Public Overrides Function Evaluate(ByVal x As Double, ByVal y As Double) As Double
        Dim fSingalAmplitude As Double = Math.Abs(fSignal)
        Dim fFactor As Double
        If fSingalAmplitude < fLevel1 Then
            If fLevel1 = 0 Then
                fFactor = fFactor1 'div 0 prevention
            Else
                fFactor = (fSingalAmplitude / fLevel1) * fFactor1
            End If
        ElseIf fSingalAmplitude < fLevel2 Then
            If (fLevel2 - fLevel1) = 0 Then
                fFactor = 2 'div 0 prevention
            Else
                Dim f As Double = ((fSingalAmplitude - fLevel1) / (fLevel2 - fLevel1))
                fFactor = fFactor1 * (1 - f) + fFactor2 * f
            End If
        ElseIf fSingalAmplitude < fLevel3 Then
            If (fLevel3 - fLevel2) = 0 Then
                fFactor = fFactor3
            Else
                Dim f As Double = ((fSingalAmplitude - fLevel2) / (fLevel3 - fLevel2))
                fFactor = fFactor2 * (1 - f) + fFactor3 * f
            End If
        Else
            fFactor = fFactor3
        End If

        Return (fSignal * fFactor + fAdd) * fIntensity
    End Function





    Overrides Sub AddOpenGlCode(ByRef DeclareCode As String, ByRef InitCode As String, ByRef ExecuteCode As String, AnySolo As Boolean)
        If Not DeclareCode.Contains("float EvaluateCompressor(int i, vec2 Location)") Then
            DeclareCode &= GetDeclareCode()
        End If
        InitCode &= GetInitCode()
        ExecuteCode &= GetExecuteCode(AnySolo)
    End Sub

    Private Function GetDeclareCode() As String
        Return "
float EvaluateCompressor(int i, vec2 Location) {
    float fSingalAmplitude = abs(Storage[i + 8]);
    float fFactor;
    if (fSingalAmplitude < Storage[i + 2]) {
        if (Storage[i + 2] == 0.0) {
            fFactor = Storage[i + 3];
        } else {
            fFactor = (fSingalAmplitude / Storage[i + 2]) * Storage[i + 3];
        }
    } else if (fSingalAmplitude < Storage[i + 4]) {
        if (Storage[i + 4] - Storage[i + 2] == 0.0) {
            fFactor = 2.0;
        } else {
            float f = ((fSingalAmplitude - Storage[i + 2]) / (Storage[i + 4] - Storage[i + 2]));
            fFactor = Storage[i + 3] * (1.0 - f) + Storage[i + 5] * f;
        }
    } else if (fSingalAmplitude < Storage[i + 6]) {
        if (Storage[i + 6] - Storage[i + 4] == 0.0) {
            fFactor = Storage[i + 7];
        } else {
            float f = ((fSingalAmplitude - Storage[i + 4]) / (Storage[i + 6] - Storage[i + 4]));
            fFactor = Storage[i + 5] * (1.0 - f) + Storage[i + 7] * f;
        }
    } else {
        fFactor = Storage[i + 7];
    }

    float fResult = (Storage[i + 8] * fFactor + Storage[i + 0]) * Storage[i + 1];
    return fResult;
}"
    End Function

    Private Function GetInitCode() As String
        Dim sb As New Text.StringBuilder
        Dim sFormat As String = "0.0######"
        sb.Append($"Storage[{OGLStorageIndex + 0}]={fAdd.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 1}]={fIntensity.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 2}]={fLevel1.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 3}]={fFactor1.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 4}]={fLevel2.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 5}]={fFactor2.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 6}]={fLevel3.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 7}]={fFactor3.ToString(sFormat, CultureInfo.InvariantCulture)};")
        sb.Append($"Storage[{OGLStorageIndex + 8}]={fSignal.ToString(sFormat, CultureInfo.InvariantCulture)};")

        Return sb.ToString
    End Function

    Private Function GetExecuteCode(AnySolo As Boolean) As String
        Dim sTempVarName As String = $"value{OGLStorageIndex}"
        Dim sExecuteCode As String = $"float {sTempVarName}=EvaluateCompressor({OGLStorageIndex}, textureCoordinate);"

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
